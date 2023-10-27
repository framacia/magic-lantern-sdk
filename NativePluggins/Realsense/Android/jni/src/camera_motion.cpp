#include <camera_motion.h>
#include <debugCPP.h>
#include <opencv2/opencv.hpp>
#include <librealsense2/rs.hpp>

#include "cv-helpers.hpp"
#include <globals.h>
#include <localization.h>

#include <vector>
#include <string>
#include <chrono>
#include <iostream>
#include <thread>

float3 algoPrev;
cv::Mat traj;
std::vector<std::chrono::milliseconds> durations;

void bestMatchesFilter(std::vector<cv::DMatch> goodMatches, std::vector<cv::DMatch>& bestMatches) {
    std::sort(goodMatches.begin(), goodMatches.end(), 
                    [](const cv::DMatch& a, const cv::DMatch& b) {
                        return a.distance < b.distance;
                    }
                );
    if (goodMatches.size() >= maxGoodFeatures) {
        bestMatches.assign(goodMatches.begin(), goodMatches.begin() + maxGoodFeatures);
    } else {
        bestMatches = goodMatches;
    }
}
void matchingAndFilteringByDistance(cv::Mat descriptors1, std::vector<cv::KeyPoint> kp1Filtered, std::vector<cv::Point2f>& pts1, std::vector<cv::Point2f>& pts2) {
    std::vector<std::vector<cv::DMatch>> matches;
    std::vector<cv::DMatch> good_matches;
    matcher->knnMatch(descriptors1, prevDescriptors, matches, 2);
        for (size_t i = 0; i < matches.size(); i++) {
            if (matches[i].size() >= 2) {
                if (matches[i][0].distance < ratioTresh * matches[i][1].distance) {
                good_matches.push_back(matches[i][0]);
                }
            }
        }
    
    std::vector<cv::DMatch> best_matches;
    bestMatchesFilter(good_matches, best_matches);
    if (!best_matches.empty()) {
        for (const cv::DMatch &match : best_matches) {
            pts1.push_back(kp1Filtered[match.queryIdx].pt);
            pts2.push_back(prevFeatures[match.trainIdx].pt);
            circle(imageFeatures, pts2.back(), 1, cv::Scalar(255, 0, 0), 1);
            cv::Point2f pt1 = pts1.back();
            cv::Point2f pt2 = pts2.back();
            line(imageFeatures, pt1, pt2, cv::Scalar(255, 0, 0), 1);
        }
    }
    
}


std::vector<cv::KeyPoint> filterKeypointsByROI(std::vector<cv::KeyPoint> &keypoints, std::vector<cv::KeyPoint> &filteredKeypoints, cv::Rect &zone) {
        for (size_t i = 0; i < keypoints.size(); i++) {
            if (zone.contains(keypoints[i].pt)) {
                continue;
                }
            filteredKeypoints.push_back(keypoints[i]);  
        }
        return filteredKeypoints;
}


void featureDetection(cv::Mat img, std::vector<cv::KeyPoint>& keypoints1, cv::Mat& descriptors1, std::vector<cv::KeyPoint>& filteredKeypoints) {
    featureExtractor->detect(img, keypoints1);
    
    // Dimensions for 640x480
    int sectionX = 180;
    int sectionY = 65;
    int sectionWidht = 325;
    int sectionHeight = 200;

    // // Dimensions for 480x270
    // int sectionX = 150;
    // int sectionY = 25;
    // int sectionWidht = 200;
    // int sectionHeight = 130;

    cv::Rect seccionToFilter(sectionX, sectionY, sectionWidht, sectionHeight);
    filterKeypointsByROI(keypoints1, filteredKeypoints, seccionToFilter);
    // std::cout << "keypoints filtered" << std::endl;
    featureDescriptor->compute(img, filteredKeypoints, descriptors1);
    
    // std::cout << "Nº features detected: " << keypoints1.size() << std::endl;
}

void computeC2MC1(const cv::Mat &R1, const cv::Mat &tvec1, const cv::Mat &R2, const cv::Mat &tvec2,
                  cv::Mat &R_1to2, cv::Mat &tvec_1to2) {
    R_1to2 = R2 * R1.t();
    tvec_1to2 = R2 * (-R1.t()*tvec1) + tvec2;
}

void preprocessImage(cv::Mat& inputImage, cv::Mat& colorMat) {
    // cv::Mat improvedColorMat;
    // cv::Mat lookUpTable(1, 256, CV_8U);
    // uchar* p = lookUpTable.ptr();
    // for( int i = 0; i < 256; ++i)
    // p[i] = cv::saturate_cast<uchar>(pow(i / 255.0, gamma_) * 255.0);
    // LUT(colorMat, lookUpTable, improvedColorMat);

    cv::cvtColor(colorMat, inputImage, cv::COLOR_BGR2GRAY);
    // cv::Ptr<cv::CLAHE> clahe = cv::createCLAHE();
    // clahe->setClipLimit(clipLimit);
    // clahe->setTilesGridSize(cv::Size(tilesGridSize, tilesGridSize));
    // clahe->apply(inputImage, inputImage);
    // cv::fastNlMeansDenoising(inputImage,
                                // inputImage,
                                // filterStrengH,
                                // filterTemplateWindowSize,
                                // filterSearchWindowSize);
}

void firstIteration() {
    rs2::frameset frames = camera.waitForFrames();

    for (int i = 0; i < 30; i++) {
        frames = camera.waitForFrames();
    }
    if (!frames) {
        Debug::Log("One or both frames are null", Color::Red);
        return;
    }

    rs2::frame colorFrame = frames.get_color_frame();
    if (!colorFrame) {
        Debug::Log("Color frame is null", Color::Red);
        return;
    }
    colorMat = frame_to_mat(colorFrame);
    cv::Mat grayImage;
    preprocessImage(grayImage, colorMat);

    std::vector<cv::KeyPoint> kp1;
    cv::Mat descriptors1;
    std::vector<cv::KeyPoint> kp1Filtered;
    featureDetection(grayImage, kp1, descriptors1, kp1Filtered);

    int id = 40;
    std::shared_ptr<Keyframe> keyframe1 = std::make_shared<Keyframe>(id,
                                                                     grayImage.clone(),
                                                                     descriptors1.clone(),
                                                                     kp1Filtered, t_f.clone());

    container.addKeyframe(keyframe1);
}


void findFeatures() {
    rs2::frameset frames = camera.waitForFrames();
    if (!frames) {
        Debug::Log("One or both frames are null", Color::Red);
        return;
    }

    rs2::frame colorFrame = frames.get_color_frame();
    if (!colorFrame) {
        Debug::Log("Color frame is null", Color::Red);
        return;
    }

    rs2::depth_frame depth = frames.get_depth_frame();

    if (!depth) {
        Debug::Log("Depth frame is null", Color::Red);
        return;
    }
    auto total_time1 = std::chrono::high_resolution_clock::now();
    auto realsense_time1 = std::chrono::high_resolution_clock::now();
    auto depth_profile = depth.get_profile().as<rs2::video_stream_profile>();
    auto colour_profile = colorFrame.get_profile().as<rs2::video_stream_profile>();
    auto _depth_intrin = depth_profile.get_intrinsics();
    auto _color_intrin = colour_profile.get_intrinsics();
    auto depth_to_color_extrin = depth_profile.get_extrinsics_to(camera.getProfile().get_stream(RS2_STREAM_COLOR));
    auto color_to_depth_extrin = colour_profile.get_extrinsics_to(camera.getProfile().get_stream(RS2_STREAM_DEPTH));
    auto sensorAuto = camera.getProfile().get_device().first<rs2::depth_sensor>();
    auto depth_scale = sensorAuto.get_depth_scale();
    int width = colour_profile.width();
    int height = colour_profile.height();
    
    const void* depth_data = depth.get_data();
    const uint16_t* depth_data_uint16 = reinterpret_cast<const uint16_t*>(depth_data);

    auto realsense_time2 = std::chrono::high_resolution_clock::now();
    auto realsense_duration = std::chrono::duration_cast<std::chrono::milliseconds>(realsense_time2 - realsense_time1);
    // __android_log_print(ANDROID_LOG_INFO, "Unity", "Realsense Duration: %lld milliseconds", realsense_duration.count());
    // std::string message1 = "The realsense duration time is: " + std::to_string(realsense_duration.count()) + " milliseconds";
    // Debug::Log(message1, Color::Red);
    
    
    if (!(algoPrev.x == 0 && algoPrev.y == 0 && algoPrev.z == 0)) {
        float current_angleX = algo.get_theta().x - algoPrev.x;
        float current_angleY = algo.get_theta().y - algoPrev.y;
        float current_angleZ = algo.get_theta().z - algoPrev.z;

        // std::cout << "current Angle X: " << current_angleX << std::endl;
        // std::cout << "current Angle Y: " << current_angleY << std::endl;
        // std::cout << "current Angle Z: " << current_angleZ << std::endl;

        if ((abs(current_angleX) > noMovementThresh && abs(current_angleY) > noMovementThresh && abs(current_angleZ) > noMovementThresh)) {
            no_move_counter = 0;
        } else {
            no_move_counter += 1;
        }
    }

    cv::Mat cameraMatrix = (cv::Mat_<double>(3, 3) <<
                                    static_cast<double>(_color_intrin.fx), 0.0, static_cast<double>(_color_intrin.ppx),
                                    0.0, static_cast<double>(_color_intrin.fy), static_cast<double>(_color_intrin.ppy),
                                    0.0, 0.0, 1.0);

    cv::Mat distCoeffs = (cv::Mat_<double>(1, 5) << _color_intrin.coeffs[0],
                                                    _color_intrin.coeffs[1],
                                                    _color_intrin.coeffs[2],
                                                    _color_intrin.coeffs[3],
                                                    _color_intrin.coeffs[4]);

    colorMat = frame_to_mat(colorFrame);

    cv::Mat grayImage;
    preprocessImage(grayImage, colorMat);

    auto feature_time1 = std::chrono::high_resolution_clock::now();
    std::vector<cv::KeyPoint> kp1;
    cv::Mat descriptors1;
    std::vector<cv::KeyPoint> kp1Filtered;
    featureDetection(grayImage, kp1, descriptors1, kp1Filtered);
        
    auto feature_time2 = std::chrono::high_resolution_clock::now();
    auto feature_duration = std::chrono::duration_cast<std::chrono::milliseconds>(feature_time2 - feature_time1);
    // __android_log_print(ANDROID_LOG_INFO, "Unity", "Feature Duration: %lld milliseconds", feature_duration.count());

    // std::string message2 = "The feature duration time is: " + std::to_string(feature_duration.count()) + " milliseconds";
    // Debug::Log(message2, Color::Red);
    imageFeatures = colorMat.clone();
    cv::Mat t_prev = cv::Mat::zeros(3, 1, CV_64F);
    cv::Mat t_1to2 = cv::Mat::zeros(3, 1, CV_64F);
    bool addTF = false;

    #ifdef BUILD_EXECUTABLE
        for (int i = 0; i < kp1Filtered.size(); i++) {
            circle(imageFeatures, kp1Filtered[i].pt, 1, cv::Scalar(0, 255, 0), 1);
        }

        traj = cv::Mat::zeros(height, width, CV_8UC3);
        int rows = 10;
        int cols = 10;

        // Calculate the width and height of each cell
        int cellWidth = traj.cols / cols;
        int cellHeight = traj.rows / rows;

        // Draw vertical lines
        for (int i = 1; i < cols; ++i) {
            int x = i * cellWidth;
            cv::line(traj, cv::Point(x, 0), cv::Point(x, traj.rows), cv::Scalar(255, 255, 255), 1);
        }

        // Draw horizontal lines
        for (int i = 1; i < rows; ++i) {
            int y = i * cellHeight;
            cv::line(traj, cv::Point(0, y), cv::Point(traj.cols, y), cv::Scalar(255, 255, 255), 1);
        }
    #endif

    int bestKeyframeId = -1;
    if (!imgColorPrev.empty()) {
        // std::cout << "trying to match" << std::endl;
        auto matcher_time1 = std::chrono::high_resolution_clock::now();
        matcher = cv::makePtr<cv::FlannBasedMatcher>(new cv::flann::LshIndexParams(5, 20, 2));
        std::vector<cv::Point2f> pts1, pts2;
        std::vector<cv::Point2f> pts1Keyframe, pts2Keyframe;
        std::vector<cv::Point2f> pts1ToEstimate, pts2ToEstimate;

        bool is_loop = false;
        // std::cout << "Trying to find a match" << std::endl;
        // std::cout << kp1Filtered.size() << " " << prevFeatures.size() << std::endl;
        if (kp1Filtered.size() >= 2 && prevFeatures.size() >= 2) {
            // if (frames_after_loop >= framesUntilLoopClosure) {
            std::thread keyFrameThread([&]() {
                bestKeyframeId = findBestMatchingKeyframe(descriptors1, kp1Filtered, pts1Keyframe, pts2Keyframe);
                 });

            matchingAndFilteringByDistance(descriptors1, kp1Filtered, pts1, pts2);
            keyFrameThread.join();
            // std::cout << "best KeyFrame Id: " << bestKeyframeId << std::endl;
            if (bestKeyframeId != -1) {
                is_loop = true;
                pts1ToEstimate = pts1Keyframe;
                pts2ToEstimate = pts2Keyframe;
            } else {
                is_loop = false;
                pts1ToEstimate = pts1;
                pts2ToEstimate = pts2;
            }
            auto matcher_time2 = std::chrono::high_resolution_clock::now();
            auto matcher_duration = std::chrono::duration_cast<std::chrono::milliseconds>(matcher_time2 - matcher_time1);
            // std::cout << "Matcher duration: " << matcher_duration.count() << " miliseconds" << std::endl;
            // __android_log_print(ANDROID_LOG_INFO, "Unity", "Matcher Duration: %lld milliseconds", matcher_duration.count());

            auto transformation_time1 = std::chrono::high_resolution_clock::now();
            std::vector<cv::Point2f> uImagePoints, vImagePoints;
            std::vector<cv::Point3f> vObjectPoints;
            if (pts1ToEstimate.size() >= 6 && pts2ToEstimate.size() >= 2) {
                for (size_t i = 0; i < pts1ToEstimate.size(); ++i) {
                float vPixel[2];
                vPixel[0] = static_cast<float>(pts1ToEstimate[i].x);
                vPixel[1] = static_cast<float>(pts1ToEstimate[i].y);
                float vPixeldepth[2];
                rs2_project_color_pixel_to_depth_pixel(vPixeldepth, depth_data_uint16, depth_scale, minDepth, maxDepth, &_depth_intrin, &_color_intrin, &depth_to_color_extrin, &color_to_depth_extrin, vPixel);
                float vDepth = depth.get_distance(vPixeldepth[0], vPixeldepth[1]);
                float vPoint[3];
                rs2_deproject_pixel_to_point(vPoint, &_depth_intrin, vPixeldepth, vDepth);
                float uPixel[2];
                uPixel[0] = static_cast<float>(pts2ToEstimate[i].x);
                uPixel[1] = static_cast<float>(pts2ToEstimate[i].y);
                    if (vDepth > minDepth && vDepth < maxDepth) {
                        vObjectPoints.push_back(cv::Point3f(vPoint[0], vPoint[1], vPoint[2]));
                        vImagePoints.push_back(cv::Point2f(vPixel[0], vPixel[1]));
                        uImagePoints.push_back(cv::Point2f(uPixel[0], uPixel[1]));
                    }
                }
            }
            cv::Mat tvec1, tvec2, rvec1, rvec2;
            try {
                if (vObjectPoints.size() >= min3DPoints && vImagePoints.size() >= min3DPoints && uImagePoints.size() >= min3DPoints) {
                    cv::solvePnPRansac(vObjectPoints,
                                        vImagePoints,
                                        cameraMatrix,
                                        distCoeffs,
                                        rvec1,
                                        tvec1,
                                        false,
                                        500,
                                        8.0f,
                                        0.99,
                                        cv::noArray(),
                                        cv::SOLVEPNP_ITERATIVE);
                    cv::solvePnPRansac(vObjectPoints,
                                        uImagePoints,
                                        cameraMatrix,
                                        distCoeffs,
                                        rvec2,
                                        tvec2,
                                        false,
                                        500,
                                        8.0f,
                                        0.99,
                                        cv::noArray(),
                                        cv::SOLVEPNP_ITERATIVE);

                    cv::Mat R1, R2;
                    cv::Rodrigues(rvec1, R1);
                    cv::Rodrigues(rvec2, R2);

                    cv::Mat R_1to2, t_1to2;
                    computeC2MC1(R1, tvec1, R2, tvec2, R_1to2, t_1to2);

                    float distanceX = cv::norm(t_1to2.at<double>(0)-t_prev.at<double>(0));
                    float distanceY = cv::norm(t_1to2.at<double>(1)-t_prev.at<double>(1));
                    float distanceZ = cv::norm(t_1to2.at<double>(2)-t_prev.at<double>(2));
                    
                    if ((distanceX < maxDistanceF2F) && (distanceY < maxDistanceF2F) && (distanceZ < maxDistanceF2F)) {
                        cv::Mat rvec_1to2;
                        cv::Rodrigues(R_1to2, rvec_1to2);
                        // if (is_loop && frames_after_loop >= framesUntilLoopClosure) {
                        if (is_loop) {
                            if (no_move_counter <= framesNoMovement) {
                                // frames_after_loop = 0;
                                t_f = t_1to2 + container.getKeyframe(bestKeyframeId)->getPose();
                                // std::cout << "In loop closure and moving" << std::endl;
                            } else {
                                // std::cout << "In loop Closure and not moving" << std::endl;
                            }
                        } else {
                            if (no_move_counter <= framesNoMovement) {
                                t_f += t_1to2;
                                // std::cout << "Camera is moving" << std::endl;
                            } else {
                                // std::cout << "Camera not moving" << std::endl;
                            }
                        }
                        if (addKeyFrame) {
                            addKeyFrame = false;
                            std::shared_ptr<Keyframe> keyframe2 = std::make_shared<Keyframe>(1,
                                                                                            grayImage.clone(),
                                                                                            descriptors1.clone(),
                                                                                            kp1Filtered,
                                                                                            t_f.clone());
                            container.addKeyframe(keyframe2);
                            // std::cout << "New KeyFrame Added" << std::endl;
                            Debug::Log("New Keyframe Added", Color::Orange);
                        }
                        addTF = true;
                    }   
                }
            } catch (const cv::Exception& e) {
                    std::cerr << "OpenCV Exception: " << e.what() << std::endl;
                    std::string error_message =  "OpenCV Exception: " + std::string(e.what());
                    Debug::Log(error_message, Color::Red);
            }
            auto transformation_time2 = std::chrono::high_resolution_clock::now();
            auto transformation_duration = std::chrono::duration_cast<std::chrono::milliseconds>(transformation_time2 - transformation_time1);
            // __android_log_print(ANDROID_LOG_INFO, "Unity", "Transformation Duration: %lld milliseconds", transformation_duration.count());
    
        }
    }
    imgColorPrev = grayImage;
    prevFeatures = kp1Filtered;
    prevDescriptors = descriptors1;
    algoPrev = algo.get_theta();
    if (addTF) {
        t_prev = t_1to2;
        addTF = false;
    }
    frames_after_loop += 1;
    auto total_time2 = std::chrono::high_resolution_clock::now();
    auto total_duration = std::chrono::duration_cast<std::chrono::milliseconds>(total_time2 - total_time1);

    #ifdef BUILD_EXECUTABLE
    int x = static_cast<int>((t_f.at<double>(0) / 5) * width);
    int y = static_cast<int>((t_f.at<double>(2) / 5) * height);
    cv::circle(traj, cv::Point(x+ width / 2, y+height/2) ,1, CV_RGB(0,255,0), 2);
    // Put the text on the image
    std::string fps_text = std::to_string(total_duration.count()) + " ms";
   
    cv::putText(colorMat, fps_text, cv::Point(10, 30), cv::FONT_HERSHEY_SIMPLEX, 1, cv::Scalar(0, 0, 255), 2);
    
    int imageWidth = imageFeatures.cols;
    int imageHeight = imageFeatures.rows;

    cv::Mat canvas = cv::Mat::zeros(2 * imageHeight, 2 * imageWidth, imageFeatures.type());

    cv::Mat colorGrayscaleImage;
    cv::cvtColor(grayImage, colorGrayscaleImage, cv::COLOR_GRAY2BGR);
    // cv::Mat prueba;
    // cv::cvtColor(traj, prueba, cv::COLOR_GRAY2BGR);
    // Copy each image onto the canvas at the desired positions
    if ( !colorMat.data || !colorGrayscaleImage.data || !imageFeatures.data || !traj.data ) {
    std::cout<< " --(!) Error reading images " << std::endl;
    } else {
        colorMat.copyTo(canvas(cv::Rect(0, 0, imageWidth, imageHeight)));
        colorGrayscaleImage.copyTo(canvas(cv::Rect(imageWidth, 0, imageWidth, imageHeight)));
        imageFeatures.copyTo(canvas(cv::Rect(0, imageHeight, imageWidth, imageHeight)));
        traj.copyTo(canvas(cv::Rect(imageWidth, imageHeight, imageWidth, imageHeight)));
        cv::imshow("Concatenated Images", canvas);
    }

    std::cout << "Total duration: " << total_duration.count() << " miliseconds" << std::endl;
    #endif
    
}


void createORB(int nfeatures,
               float scaleFactor,
               int nlevels,
               int edgeThreshold,
               int firstLevel,
               int WTA_K,
               int scoreType,
               int patchSize,
               int fastThreshold) {
    cv::ORB::ScoreType score;
    if (scoreType == 1) {
        cv::ORB::ScoreType score = cv::ORB::FAST_SCORE;
    } else {
        cv::ORB::ScoreType score = cv::ORB::HARRIS_SCORE;
    }
    // featureExtractor = cv::FastFeatureDetector::create(fastThreshold, true);
    // featureExtractor = cv::GFTTDetector::create(nfeatures);
    featureExtractor = cv::ORB::create(nfeatures,
                                       scaleFactor,
                                       nlevels,
                                       edgeThreshold,
                                       firstLevel,
                                       WTA_K,
                                       score,
                                       patchSize,
                                       fastThreshold);
    featureDescriptor = cv::ORB::create(nfeatures,
                                        scaleFactor,
                                        nlevels,
                                        edgeThreshold,
                                        firstLevel,
                                        WTA_K,
                                        score,
                                        patchSize,
                                        fastThreshold);
    // featureDescriptor = cv::xfeatures2d::BriefDescriptorExtractor::create();
}

void GetTranslationVector(float* t_f_data) {
    if (t_f.empty()) {
        return;
    }
    for (int i = 0; i < 3; i++) {
        t_f_data[i] = static_cast<float>(t_f.at<double>(i));
    }
}

void GetCameraOrientation(float* cameraAngle) {
    if (algo.get_theta().x == 0 && algo.get_theta().y == 0 && algo.get_theta().z == 0) {
        return;
    }    
    cameraAngle[0] = static_cast<float>(-(algo.get_theta().x * 180 / PI_FL));
    cameraAngle[1] = static_cast<float>(algo.get_theta().y * 180 / PI_FL);
    cameraAngle[2] = static_cast<float>(-(algo.get_theta().z * 180 / PI_FL) - 90);
}

void setParams(CameraConfig config) {
    ratioTresh = config.ratioTresh;
    minDepth = config.minDepth;
    maxDepth = config.maxDepth;
    min3DPoints = config.min3DPoints;
    maxDistanceF2F = config.maxDistanceF2F;
    minFeaturesLoopClosure = config.minFeaturesLoopClosure;
    framesUntilLoopClosure = config.framesUntilLoopClosure;
    noMovementThresh = config.noMovementThresh / 10000;
    framesNoMovement = config.framesNoMovement;
    maxGoodFeatures = config.maxGoodFeatures;
}

void resetOdom() {
    t_f = cv::Mat::zeros(3, 1, CV_64F);
}

void addKeyframe() {
    addKeyFrame = true;
}

// extern "C" const uchar* getJpegBuffer(int* bufferSize) {
//     std::vector<uchar> jpegBuffer;
//     cv::imencode(".jpeg", colorMat, jpegBuffer, std::vector<int>{cv::IMWRITE_JPEG_QUALITY, 100});
//     uchar* unityBuffer = new uchar[jpegBuffer.size()];
//     memcpy(unityBuffer, jpegBuffer.data(), jpegBuffer.size());
//     *bufferSize = jpegBuffer.size();

//     return unityBuffer;
// }

