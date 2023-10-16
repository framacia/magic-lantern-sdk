// #include <jni.h>
#include "camera_motion.h"
#include <android/log.h>
#include <vector>
#include <string>
#include <chrono>
#include <iostream>
#include <queue>

#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/features2d.hpp>
#include <opencv2/calib3d.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/photo.hpp>
#include "opencv2/imgcodecs.hpp"
#include "opencv2/highgui.hpp"


#include <librealsense2/rs.hpp>

#include <Eigen/Core>
#include <Eigen/Geometry>




rs2::pipeline pipeline;
rs2::pipeline_profile profile;
rs2::config cfg;
//rs2::threshold_filter threshold_depth;

// std::vector<uchar> jpegBuffer;


static cv::Ptr<cv::Feature2D> featureExtractor;
static cv::Ptr<cv::Feature2D> featureDescriptor;
// static cv::Ptr<cv::ORB> featureExtractor;
// static cv::Ptr<cv::DescriptorMatcher> matcher;
// static cv::Ptr<cv::FlannBasedMatcher> matcher;
static cv::Mat cameraMatrix;
cv::Mat imgColorPrev;
cv::Mat image2show;
cv::Mat grayImage;
cv::Mat colorImage;

std::vector<cv::KeyPoint> prevFeatures;
cv::Mat prevDescriptors;
rs2::depth_frame prevDepth = rs2::depth_frame(rs2::frame());

cv::Mat R1, R2;
Eigen::Matrix3f eigenRotationMatrix;
cv::Mat t_f = cv::Mat::zeros(3, 1, CV_64F);
cv::Mat t_prev = cv::Mat::zeros(3, 1, CV_64F);
cv::Mat rvec1, rvec2;
cv::Mat tvec1 = cv::Mat::zeros(3, 1, CV_64F);
cv::Mat tvec2 = cv::Mat::zeros(3, 1, CV_64F);



float ratioTresh;
float minDepth;
float maxDepth;
int min3DPoints;
float maxDistanceF2F;
int maxFeaturesSolver;
float clipLimit;
int tilesGridSize;
int filterTemplateWindowSize;
float filterSearchWindowSize;
int filterStrengH;
float gamma_;




void featureDetection(cv::Mat img, std::vector<cv::KeyPoint>& keypoints1, cv::Mat& descriptors1) {

    // featureExtractor->detectAndCompute(img, cv::noArray(), keypoints1, descriptors1);
    // featureExtractor->detectAndCompute(img, cv::noArray(), keypoints1, descriptors1);
    featureExtractor->detect(img,keypoints1);
    featureDescriptor->compute(img,keypoints1,descriptors1);

    __android_log_print(ANDROID_LOG_INFO, "Unity", "nª features detected: %d", keypoints1.size());
}

void computeC2MC1(const cv::Mat &R1, const cv::Mat &tvec1, const cv::Mat &R2, const cv::Mat &tvec2,
                  cv::Mat &R_1to2, cv::Mat &tvec_1to2)
{
    R_1to2 = R2 * R1.t();
    tvec_1to2 = R2 * (-R1.t()*tvec1) + tvec2;
}


float GetDepthAtCenter() {
    try {
        // Wait for new frames from the camera
        rs2::frameset frames = pipeline.wait_for_frames();

        // Get the depth frame
        rs2::depth_frame depth_frame = frames.get_depth_frame();

        // Get the width and height of the depth frame
        int width = depth_frame.get_width();
        int height = depth_frame.get_height();

        // Access the depth data
        int center_x = width / 2;
        int center_y = height / 2;
        int center_idx = center_y * width + center_x;
        float depth_in_meters = depth_frame.get_distance(center_x, center_y);
        return depth_in_meters;
    }
    catch (const rs2::error& e) {
        __android_log_print(ANDROID_LOG_INFO, "Error", "fallo");
        return 0.0f;
    }
}

void CleanupCamera() {
    // Stop and close the pipeline
    pipeline.stop();
}

void bagFileStreamConfig(const char* bagFileAddress)
{
    try {
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "bagFile path: %s", bagFileAddress);
        cfg.enable_device_from_file(bagFileAddress, true);
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "bagFile path2: %s", bagFileAddress);
    }
    catch (const rs2::error& e) {
        const char* error_message = e.what();
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "Error in bagFileStreamConfig: %s", error_message);
    }
}

void colorStreamConfig(int width, int height, int fps)
{
    cfg.enable_stream(RS2_STREAM_COLOR, -1, width, height, RS2_FORMAT_RGB8 , fps);
}

void depthStreamConfig(int width, int height, int fps)
{
    cfg.enable_stream(RS2_STREAM_DEPTH, -1, width, height, RS2_FORMAT_Z16, fps);
}

void initCamera() {

     try {
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "defining profile");
        profile = pipeline.start(cfg);
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "profile defined");
        // auto device = profile.get_device();
        // auto playback = device.as<rs2::playback>();
        // playback.set_real_time(true);
        // __android_log_print(ANDROID_LOG_ERROR, "Unity", "real time");

        }

    catch (const rs2::error& e) {
        // Handle errors
    }
}


void findFeatures() {

    auto start_time = std::chrono::high_resolution_clock::now();
    rs2::frameset frames = pipeline.wait_for_frames();   

    if (!frames || !frames.get_depth_frame() || !frames.get_color_frame()) {
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "some of the frames are null");
        return;
    }


    rs2::align alignTo(RS2_STREAM_COLOR);
    frames = alignTo.process(frames);


    if (!frames) {
        __android_log_print(ANDROID_LOG_WARN, "Unity", "frames is null");
        return;
    }

    rs2::frame colorFrame = frames.get_color_frame();
    if (!colorFrame) {
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "colorFrame is null");
        return;
    }
    

    rs2::depth_frame depth = frames.get_depth_frame();

    if (!depth) {
        __android_log_print(ANDROID_LOG_ERROR, "Unity", "depthFrame is null");
        return;
    }
    auto depth_profile = depth.get_profile().as<rs2::video_stream_profile>();
    auto colour_profile = colorFrame.get_profile().as<rs2::video_stream_profile>();

    auto _depth_intrin = depth_profile.get_intrinsics();
    auto _depth_to_color_extrin = depth_profile.get_extrinsics_to(colour_profile);
    auto _color_intrin = colour_profile.get_intrinsics();
    auto _colour_to_depth_extrin = colour_profile.get_extrinsics_to(depth_profile);

    int width = colour_profile.width();
    int height = colour_profile.height();

    cv::Mat cameraMatrix = (cv::Mat_<double>(3,3) << (double)_color_intrin.fx, 0.0, (double)_color_intrin.ppx,
                                                    0.0, (double)_color_intrin.fy, (double)_color_intrin.ppy,
                                                    0.0, 0.0, 1.0);

    cv::Mat distCoeffs = (cv::Mat_<double>(1, 5) << _color_intrin.coeffs[0],
                                                    _color_intrin.coeffs[1],
                                                    _color_intrin.coeffs[2],
                                                    _color_intrin.coeffs[3],
                                                    _color_intrin.coeffs[4]);
    
    cv::Mat colorMat(cv::Size(width, height), CV_8UC3, (void *)colorFrame.get_data(), cv::Mat::AUTO_STEP);

    
    cv::Mat lookUpTable(1, 256, CV_8U);
    uchar* p = lookUpTable.ptr();
    for( int i = 0; i < 256; ++i)
    p[i] = cv::saturate_cast<uchar>(pow(i / 255.0, gamma_) * 255.0);
    // cv::Mat res = colorMat.clone();
    LUT(colorMat, lookUpTable, colorMat);

    // cv::GaussianBlur(colorMat, colorMat, cv::Size(7, 7), 0, 0, cv::BORDER_DEFAULT);
    // cv::fastNlMeansDenoisingColored(colorMat, colorMat, 10, 10, 7);
    
    // You can try more different parameters
    // cv::bilateralFilter(colorMat, colorMat, 9, 75, 75, cv::BORDER_DEFAULT);
    cv::cvtColor(colorMat, grayImage, cv::COLOR_BGR2GRAY);

    
    
    
    cv::Ptr<cv::CLAHE> clahe = cv::createCLAHE();
    clahe->setClipLimit(clipLimit);
    clahe->setTilesGridSize(cv::Size(tilesGridSize, tilesGridSize));

    // cv::Mat imageEnhanced;
    clahe->apply(grayImage, grayImage);


    cv::fastNlMeansDenoising(grayImage, grayImage, filterStrengH, filterTemplateWindowSize, filterSearchWindowSize);
    
    
    // cv::medianBlur(grayImage,grayImage,7);
    
   
    
    // cv::Mat unistorted;
    // cv::undistort(grayImage,unistorted,cameraMatrix,distCoeffs,cv::noArray());
    
    std::vector<cv::KeyPoint> kp1;
    cv::Mat descriptors1;


    featureDetection(grayImage, kp1, descriptors1);

    // cv::Mat Image2Show;
    image2show = colorMat.clone();

    for (int i = 0; i < kp1.size(); i++) {
        circle(image2show, kp1[i].pt, 1, cv::Scalar(0, 255, 0), 1);
    }

    if (!imgColorPrev.empty()) {


        cv::FlannBasedMatcher matcher(new cv::flann::LshIndexParams(6, 12, 1));
        std::vector<std::vector<cv::DMatch>> matches;
        
            matcher.knnMatch(descriptors1, prevDescriptors, matches, 2);

            __android_log_print(ANDROID_LOG_INFO, "Unity", "nª features matched: %d", matches.size());

            std::vector<cv::DMatch> good_matches;
            for (size_t i = 0; i < matches.size(); i++) {
                if (matches[i].size() >= 2){
                    if (matches[i][0].distance < ratioTresh * matches[i][1].distance) {
                    good_matches.push_back(matches[i][0]);
                }
                }
                
            }
            if (!good_matches.empty()){
                __android_log_print(ANDROID_LOG_INFO, "Unity", "nª good matches: %d", good_matches.size());
                std::vector<cv::DMatch> best_matches;
                std::sort(good_matches.begin(), good_matches.end(), [](const cv::DMatch& a, const cv::DMatch& b) {
                    return a.distance < b.distance;
                });

                if (maxFeaturesSolver == -1) {
                    best_matches = good_matches; 
                } else {
                     if (good_matches.size() > maxFeaturesSolver) {
                    best_matches.assign(good_matches.begin(), good_matches.begin() + maxFeaturesSolver);
                } else {
                    best_matches = good_matches; 
                }
                }
               
                std::vector<cv::Point2f> pts1, pts2;
                for (const cv::DMatch &match: good_matches) {
                    pts1.push_back(kp1[match.queryIdx].pt);
                    pts2.push_back(prevFeatures[match.trainIdx].pt);

                    circle(image2show, pts2.back(), 1, cv::Scalar(255, 0, 0), 1);
                    cv::Point2f pt1 = pts1.back();
                    cv::Point2f pt2 = pts2.back();
                    line(image2show, pt1, pt2, cv::Scalar(255, 0, 0), 1);
                }

                std::vector<cv::Point2f> uImagePoints, vImagePoints;
                std::vector<cv::Point3f> uObjectPoints, vObjectPoints;
                for (size_t i = 0; i < pts1.size(); ++i) {
                    float vPixel[2];
                    vPixel[0] = static_cast<float>(pts1[i].x);
                    vPixel[1] = static_cast<float>(pts1[i].y);
                    float vDepth = depth.get_distance(vPixel[0], vPixel[1]);
                    float vPoint[3];
                    rs2_deproject_pixel_to_point(vPoint, &_depth_intrin, vPixel, vDepth);

                    float uPixel[2];
                    uPixel[0] = static_cast<float>(pts2[i].x);
                    uPixel[1] = static_cast<float>(pts2[i].y);
                    float uDepth = depth.get_distance(uPixel[0], uPixel[1]);
                    float uPoint[3];
                    rs2_deproject_pixel_to_point(uPoint, &_depth_intrin, uPixel, uDepth);
                    

                    if ((vDepth > minDepth && vDepth < maxDepth) && (uDepth > minDepth && uDepth < maxDepth)) {  // Check for valid depth value
                        vObjectPoints.push_back(cv::Point3f(vPoint[0], vPoint[1], vPoint[2]));
                        vImagePoints.push_back(cv::Point2f(pts2[i].x, pts2[i].y));

                        uObjectPoints.push_back(cv::Point3f(uPoint[0], uPoint[1], uPoint[2]));
                        uImagePoints.push_back(cv::Point2f(pts1[i].x, pts1[i].y));
                    }
                }
                
                if ((uObjectPoints.size() > min3DPoints) && (vObjectPoints.size() > min3DPoints)) {
                    cv::solvePnPRansac(vObjectPoints, vImagePoints, cameraMatrix, distCoeffs, rvec1, tvec1);
                    cv::solvePnPRansac(uObjectPoints, uImagePoints, cameraMatrix, distCoeffs, rvec2, tvec2);

                    float distanceX = cv::norm(tvec1.at<double>()-t_prev.at<double>(0));
                    float distanceY = cv::norm(tvec1.at<double>(1)-t_prev.at<double>(1));
                    float distanceZ = cv::norm(tvec1.at<double>(2)-t_prev.at<double>(2));
                    

                    if ((distanceX < maxDistanceF2F) && (distanceY < maxDistanceF2F) && (distanceZ < maxDistanceF2F)) {
                        Rodrigues(rvec1, R1);
                        Rodrigues(rvec2, R2);

                        cv::Mat R_1to2, t_1to2;
                        computeC2MC1(R1, tvec1, R2, tvec2, R_1to2, t_1to2);

                        cv::Mat rvec_1to2;
                        Rodrigues(R_1to2, rvec_1to2);

                        t_f += t_1to2;

                        // std::ostringstream tvec_stream;
                        // tvec_stream << t_f;
                        // __android_log_print(ANDROID_LOG_INFO, "Unity", "Translation Vector (tvec): %s", tvec_stream.str().c_str());

                        // for (int i = 0; i < 3; i++) {
                        //     for (int j = 0; j < 3; j++) {
                        //         eigenRotationMatrix(i, j) = R_1to2.at<double>(i, j);
                        //         }
                        // }            
                }
            }
        }
        
        
    }


    imgColorPrev = grayImage.clone();
    prevFeatures = kp1;
    prevDescriptors = descriptors1;
    prevDepth = depth;

    t_prev = tvec1.clone();

    auto end_time = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
    __android_log_print(ANDROID_LOG_WARN, "Unity", "Elapsed time in findFeatures: %lld ms", duration.count());
}

void GetTranslationVector(float* t_f_data) {
    if (t_f.empty()) {
        // Handle the case where the matrix is empty or not initialized
    
        return;
    }

    // Copy the data from the cv::Mat to the provided double array
    for (int i = 0; i < 3; i++) {
        t_f_data[i] = static_cast<float>(t_f.at<double>(i));
    }

   
    // Eigen::Quaternionf quaternion(eigenRotationMatrix);

    // // Extract quaternion components
    // float w = quaternion.w();
    // float x = quaternion.x();
    // float y = quaternion.y();
    // float z = quaternion.z();

    // // Store the components in a float array
    // t_f_data[3] = w;
    // t_f_data[4] = x;
    // t_f_data[5] = y;
    // t_f_data[6] = z;


}

void createORB(int nfeatures, float scaleFactor, int nlevels, int edgeThreshold, int firstLevel, int WTA_K, int scoreType, int patchSize, int fastThreshold) {
    
    cv::ORB::ScoreType score;

    if (scoreType == 1){
        cv::ORB::ScoreType score = cv::ORB::FAST_SCORE;
    } else
    {
        cv::ORB::ScoreType score = cv::ORB::HARRIS_SCORE;
    }
    featureExtractor = cv::FastFeatureDetector::create(fastThreshold);
    featureDescriptor = cv::ORB::create(nfeatures, scaleFactor, nlevels, edgeThreshold, firstLevel, WTA_K, score, patchSize, fastThreshold);
    }

void createFAST(int nfeatures, int nOctaveLayers, double contrastThreshold, double edgeThreshold, double sigma, bool enable_precise_upscale)
{

    featureExtractor = cv::FastFeatureDetector::create(20);
    featureDescriptor = cv::SIFT::create(nfeatures, nOctaveLayers, contrastThreshold, edgeThreshold, sigma);
}

void setParams(float newRatioTresh, float newMinDepth,
                float newMaxDepth, int newMin3DPoints,
                float newMaxDistanceF2F, int newMaxFeaturesSolver,
                float newClipLimit, int newTilesGridSize,
                int newFilterTemplateWindowSize, float newFilterSearchWindowSize,
                int newFilterStrengH, float newGamma)
{
    ratioTresh = newRatioTresh;
    minDepth = newMinDepth;
    maxDepth = newMaxDepth;
    min3DPoints = newMin3DPoints;
    maxDistanceF2F = newMaxDistanceF2F;
    maxFeaturesSolver = newMaxFeaturesSolver;
    clipLimit = newClipLimit;
    tilesGridSize = newTilesGridSize;
    filterTemplateWindowSize = newFilterSearchWindowSize;
    filterSearchWindowSize = newFilterSearchWindowSize;
    filterStrengH = newFilterStrengH;
    gamma_ = newGamma;

}

extern "C" const uchar* getJpegBuffer(int frameType, int* bufferSize) {

    std::vector<uchar> jpegBuffer;
    if (frameType == 0) {
        cv::imencode(".jpeg", image2show, jpegBuffer, std::vector<int>{cv::IMWRITE_JPEG_QUALITY, 100});
    } else if (frameType == 1) {
        cv::imencode(".jpeg", grayImage, jpegBuffer, std::vector<int>{cv::IMWRITE_JPEG_QUALITY, 100});
    }
    // Allocate memory in Unity for the buffer and copy the JPEG data
    uchar* unityBuffer = new uchar[jpegBuffer.size()];
    memcpy(unityBuffer, jpegBuffer.data(), jpegBuffer.size());
    // Set the buffer size
    *bufferSize = jpegBuffer.size();

    return unityBuffer;
}