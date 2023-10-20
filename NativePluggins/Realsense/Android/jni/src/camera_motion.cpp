#include <camera_motion.h>
#include <debugCPP.h>
#include <opencv2/opencv.hpp>
// #include <opencv2/xfeatures2d.hpp>
#include <librealsense2/rs.hpp>

#include "cv-helpers.hpp"
#include <rs_motion.h>

#include <vector>
#include <string>
#include <chrono>
#include <iostream>

#include <android/log.h>



rs2::pipeline pipeline;
rs2::pipeline_profile profile;
rs2::config cfg;
rs2::pipeline imu_pipeline;
rs2::config imu_cfg;
float3 algoPrev;
rotation_estimator algo;
cv::Ptr<cv::Feature2D> featureExtractor;
cv::Ptr<cv::Feature2D> featureDescriptor;
cv::Ptr<cv::FlannBasedMatcher> matcher;

cv::Mat imgColorPrev;
std::vector<cv::KeyPoint> prevFeatures;
cv::Mat prevDescriptors;
cv::Mat t_f = cv::Mat::zeros(3, 1, CV_64F);
// cv::Mat traj;
KeyframeContainer container;

int no_move_counter = 0;
int frames_after_loop = 100;
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
bool addKeyFrame = false;

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


std::vector<std::chrono::milliseconds> durations;

// namespace po = boost::program_options;

void featureDetection(cv::Mat img, std::vector<cv::KeyPoint>& keypoints1) {
    featureExtractor->detect(img, keypoints1);
    
    // std::cout << "Nº features detected: " << keypoints1.size() << std::endl;
}

void computeC2MC1(const cv::Mat &R1, const cv::Mat &tvec1, const cv::Mat &R2, const cv::Mat &tvec2,
                  cv::Mat &R_1to2, cv::Mat &tvec_1to2) {
    R_1to2 = R2 * R1.t();
    tvec_1to2 = R2 * (-R1.t()*tvec1) + tvec2;
}

int findBestMatchingKeyframe(const cv::Mat& descriptors1,
                             std::vector<cv::DMatch>& goodMatches,
                             std::vector<std::vector<cv::DMatch>>& matches
                            ) {
    int bestKeyframeId = -1;
    int mostGoodMatches = 0;

    std::vector<cv::DMatch> goodMatches_aux;
    for (int keyframeIndex = 0; keyframeIndex < container.getKeyframeCount(); keyframeIndex++) {
        matcher->knnMatch(descriptors1, container.getKeyframe(keyframeIndex)->getDescriptors(), matches, 2);
        if (!goodMatches_aux.empty()) {
            goodMatches_aux.clear();
        }
        for (size_t i = 0; i < matches.size(); i++) {
            if (matches[i].size() >= 2 && matches[i][0].distance < ratioTresh * matches[i][1].distance) {
                goodMatches_aux.push_back(matches[i][0]);
            }
        }
        matches.clear();
        if (goodMatches_aux.size() >= 200 && goodMatches_aux.size() > mostGoodMatches) {
            mostGoodMatches = goodMatches.size();
            bestKeyframeId = keyframeIndex;
            goodMatches = goodMatches_aux;
        }
    }

    return bestKeyframeId;
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

std::vector<cv::KeyPoint> filterKeypointsByROI(std::vector<cv::KeyPoint> &keypoints, std::vector<cv::KeyPoint> &filteredKeypoints, cv::Rect &zone) {
        for (size_t i = 0; i < keypoints.size(); i++) {
            if (zone.contains(keypoints[i].pt))
                {
                continue;
                }
            filteredKeypoints.push_back(keypoints[i]);
            // filteredDescriptors.push_back(descriptors.row(i));
        }
        // return std::make_pair(filteredKeypoints, filteredDescriptors);
        return filteredKeypoints;
}


void cleanupCamera() {
    t_f = cv::Mat::zeros(3, 1, CV_64F);
    pipeline.stop();
    imu_pipeline.stop();
    imgColorPrev.release();
    prevFeatures.clear();
    prevDescriptors = cv::Mat();
    std::string message = "Memory resources cleaned up!";
    Debug::Log(message, Color::Red);
}

void bagFileStreamConfig(const char* bagFileAddress) {
    try {
        cfg.enable_device_from_file(bagFileAddress, true);
    }
    catch (const rs2::error& e) {
        std::string error_message = e.what();
        Debug::Log(error_message, Color::Red);
    }
}

void colorStreamConfig(int width, int height, int fps) {
    cfg.enable_stream(RS2_STREAM_COLOR, -1, width, height, RS2_FORMAT_RGB8 , fps);
}

void depthStreamConfig(int width, int height, int fps) {
    cfg.enable_stream(RS2_STREAM_DEPTH, -1, width, height, RS2_FORMAT_Z16, fps);
}

void initCamera() {
     try {
        profile = pipeline.start(cfg);
        }
    catch (const rs2::error& e) {
        std::string error_message = e.what();
        Debug::Log(error_message, Color::Red);
    }
}

void initImu() {
    imu_cfg.enable_stream(RS2_STREAM_ACCEL, RS2_FORMAT_MOTION_XYZ32F);
    imu_cfg.enable_stream(RS2_STREAM_GYRO, RS2_FORMAT_MOTION_XYZ32F);
    auto imu_profile = imu_pipeline.start(imu_cfg, [&](rs2::frame imu_frame) {
        auto motion = imu_frame.as<rs2::motion_frame>();
        if (motion && motion.get_profile().stream_type() == RS2_STREAM_GYRO &&
            motion.get_profile().format() == RS2_FORMAT_MOTION_XYZ32F) {
            double ts = motion.get_timestamp();
            rs2_vector gyro_data = motion.get_motion_data();
            algo.process_gyro(gyro_data, ts);
        }
        if (motion && motion.get_profile().stream_type() == RS2_STREAM_ACCEL &&
            motion.get_profile().format() == RS2_FORMAT_MOTION_XYZ32F) {
            rs2_vector accel_data = motion.get_motion_data();
            algo.process_accel(accel_data);
        }
    });
}
void firstIteration() {
    rs2::frameset frames;
    for (int i = 0; i < 30; i++) {
        frames = pipeline.wait_for_frames();
    }
    if (!frames) {
        return;
    }

    rs2::frame colorFrame = frames.get_color_frame();
    if (!colorFrame) {
        return;
    }
    cv::Mat colorMat = frame_to_mat(colorFrame);
    cv::Mat grayImage;
    preprocessImage(grayImage, colorMat);

    std::vector<cv::KeyPoint> kp1;
    cv::Mat descriptors1;

    featureDetection(grayImage, kp1);

    cv::Rect seccionToFilter(sectionX, sectionY, sectionWidht, sectionHeight);
    std::vector<cv::KeyPoint> kp1Filtered;
    filterKeypointsByROI(kp1, kp1Filtered, seccionToFilter);
    std::cout << "keypoints filtered" << std::endl;
    featureDescriptor->compute(grayImage, kp1Filtered, descriptors1);

    int id = 40;
    std::shared_ptr<Keyframe> keyframe1 = std::make_shared<Keyframe>(id,
                                                                     grayImage.clone(),
                                                                     descriptors1.clone(),
                                                                     kp1Filtered, t_f.clone());

    container.addKeyframe(keyframe1);
}


void findFeatures() {
    auto total_time1 = std::chrono::high_resolution_clock::now();
    auto realsense_time1 = std::chrono::high_resolution_clock::now();
    rs2::frameset frames = pipeline.wait_for_frames();

    // if (!frames || !frames.get_depth_frame() || !frames.get_color_frame()) {
    //     Debug::Log("Frames is null", Color::Red);
    //     return;
    // }

   
    // // auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
    // rs2::align alignTo(RS2_STREAM_COLOR);
    // frames = alignTo.process(frames);

    if (!frames) {
        Debug::Log("One or both frames after align are null", Color::Red);
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



    auto depth_profile = depth.get_profile().as<rs2::video_stream_profile>();
    auto colour_profile = colorFrame.get_profile().as<rs2::video_stream_profile>();
    auto _depth_intrin = depth_profile.get_intrinsics();
    auto _color_intrin = colour_profile.get_intrinsics();
    auto depth_to_color_extrin = depth_profile.get_extrinsics_to(profile.get_stream(RS2_STREAM_COLOR));
    auto color_to_depth_extrin = colour_profile.get_extrinsics_to(profile.get_stream(RS2_STREAM_DEPTH));
    auto sensorAuto = profile.get_device().first<rs2::depth_sensor>();
    auto depth_scale = sensorAuto.get_depth_scale();
    int width = colour_profile.width();
    int height = colour_profile.height();
    
    const void* depth_data = depth.get_data();
    const uint16_t* depth_data_uint16 = reinterpret_cast<const uint16_t*>(depth_data);

    auto realsense_time2 = std::chrono::high_resolution_clock::now();
    auto realsense_duration = std::chrono::duration_cast<std::chrono::milliseconds>(realsense_time2 - realsense_time1);
    __android_log_print(ANDROID_LOG_INFO, "Unity", "Realsense Duration: %lld milliseconds", realsense_duration.count());
    // std::string message1 = "The realsense duration time is: " + std::to_string(realsense_duration.count()) + " milliseconds";
    // Debug::Log(message1, Color::Red);

    
    if (!(algoPrev.x == 0 && algoPrev.y == 0 && algoPrev.z == 0)) {
        float current_angleX = algo.get_theta().x - algoPrev.x;
        float current_angleY = algo.get_theta().y - algoPrev.y;
        float current_angleZ = algo.get_theta().z - algoPrev.z;
        if ((abs(current_angleX) > 1e-04 && abs(current_angleY) > 1e-04 && abs(current_angleZ) > 1e-04)) {
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

    cv::Mat colorMat = frame_to_mat(colorFrame);

    cv::Mat grayImage;
    preprocessImage(grayImage, colorMat);

    auto feature_time1 = std::chrono::high_resolution_clock::now();
    std::vector<cv::KeyPoint> kp1;
    cv::Mat descriptors1;
    featureDetection(grayImage, kp1);

    cv::Rect seccionToFilter(sectionX, sectionY, sectionWidht, sectionHeight);
    std::vector<cv::KeyPoint> kp1Filtered;
    filterKeypointsByROI(kp1, kp1Filtered, seccionToFilter);
    std::cout << "keypoints filtered" << std::endl;
    featureDescriptor->compute(grayImage, kp1Filtered, descriptors1);
    
    auto feature_time2 = std::chrono::high_resolution_clock::now();
    auto feature_duration = std::chrono::duration_cast<std::chrono::milliseconds>(feature_time2 - feature_time1);
    __android_log_print(ANDROID_LOG_INFO, "Unity", "Feature Duration: %lld milliseconds", feature_duration.count());

    // std::string message2 = "The feature duration time is: " + std::to_string(feature_duration.count()) + " milliseconds";
    // Debug::Log(message2, Color::Red);


    cv::Mat imageFeatures;
    imageFeatures = colorMat;

    for (int i = 0; i < kp1Filtered.size(); i++) {
        circle(imageFeatures, kp1Filtered[i].pt, 1, cv::Scalar(0, 255, 0), 1);
    }

    cv::Mat t_prev = cv::Mat::zeros(3, 1, CV_64F);
    cv::Mat t_1to2 = cv::Mat::zeros(3, 1, CV_64F);
    bool addTF = false;

    // traj = cv::Mat::zeros(height, width, CV_8UC3);
    // int rows = 10;
    // int cols = 10;

    // // Calculate the width and height of each cell
    // int cellWidth = traj.cols / cols;
    // int cellHeight = traj.rows / rows;

    // // Draw vertical lines
    // for (int i = 1; i < cols; ++i) {
    //     int x = i * cellWidth;
    //     cv::line(traj, cv::Point(x, 0), cv::Point(x, traj.rows), cv::Scalar(255, 255, 255), 1);
    // }

    // // Draw horizontal lines
    // for (int i = 1; i < rows; ++i) {
    //     int y = i * cellHeight;
    //     cv::line(traj, cv::Point(0, y), cv::Point(traj.cols, y), cv::Scalar(255, 255, 255), 1);
    // }

    int quantity_frames = 200;
    int bestKeyframeId = -1;
    if (!imgColorPrev.empty()) {
        std::cout << "trying to match" << std::endl;
        matcher = cv::makePtr<cv::FlannBasedMatcher>(new cv::flann::LshIndexParams(5, 20, 2));
        std::vector<std::vector<cv::DMatch>> matches;
        std::vector<cv::DMatch> good_matches;
        std::vector<cv::Point2f> pts1, pts2;
        bool is_loop = false;
        std::cout << "Trying to find a match" << std::endl;
        if (kp1Filtered.size() >= 2 && prevFeatures.size() >= 2) {
            if (frames_after_loop >= quantity_frames) {
                
                bestKeyframeId = findBestMatchingKeyframe(descriptors1, good_matches, matches);
            }
            if (bestKeyframeId != -1) {
                is_loop = true;
                const auto& kpKeyframe = container.getKeyframe(bestKeyframeId)->getKeypoints();
                for (const cv::DMatch &match : good_matches) {
                    pts1.push_back(kp1Filtered[match.queryIdx].pt);
                    pts2.push_back(kpKeyframe[match.trainIdx].pt);
                    // circle(imageFeatures, pts2.back(), 1, cv::Scalar(255, 0, 0), 1);
                    // cv::Point2f pt1 = pts1.back();
                    // cv::Point2f pt2 = pts2.back();
                    // line(imageFeatures, pt1, pt2, cv::Scalar(255, 0, 0), 1);
                }
            } else {
                auto nokeyframe_t1 = std::chrono::high_resolution_clock::now();

                is_loop = false;
                matches.clear();
                good_matches.clear();
                matcher->knnMatch(descriptors1, prevDescriptors, matches, 2);

                 for (size_t i = 0; i < matches.size(); i++) {
                    if (matches[i].size() >= 2) {
                        if (matches[i][0].distance < ratioTresh * matches[i][1].distance) {
                        good_matches.push_back(matches[i][0]);
                        }
                    }
                }
                if (!good_matches.empty()) {
                    for (const cv::DMatch &match : good_matches) {
                        pts1.push_back(kp1Filtered[match.queryIdx].pt);
                        pts2.push_back(prevFeatures[match.trainIdx].pt);
                        // circle(imageFeatures, pts2.back(), 1, cv::Scalar(255, 0, 0), 1);
                        // cv::Point2f pt1 = pts1.back();
                        // cv::Point2f pt2 = pts2.back();
                        // line(imageFeatures, pt1, pt2, cv::Scalar(255, 0, 0), 1);
                    }
                }
            }
            std::vector<cv::Point2f> uImagePoints, vImagePoints;
            std::vector<cv::Point3f> vObjectPoints;
            for (size_t i = 0; i < pts1.size(); ++i) {
                float vPixel[2];
                vPixel[0] = static_cast<float>(pts1[i].x);
                vPixel[1] = static_cast<float>(pts1[i].y);
                float vPixeldepth[2];
                rs2_project_color_pixel_to_depth_pixel(vPixeldepth, depth_data_uint16, depth_scale, minDepth, maxDepth, &_depth_intrin, &_color_intrin, &depth_to_color_extrin, &color_to_depth_extrin, vPixel);
                // __android_log_print(ANDROID_LOG_INFO, "Unity", "Depth pixels are x: %f and y: %f", vPixeldepth[0], vPixeldepth[1]);
                // std::cout << "Depth pixels are x: " << vPixeldepth[0] << " y: " << vPixeldepth[1] << std::endl;
                // __android_log_print(ANDROID_LOG_INFO, "Unity", "Color pixels are x: %f and y: %f", vPixel[0], vPixel[1]);
                // std::cout << "Color pixels are x: " << vPixel[0] << " y: " << vPixel[1] << std::endl;
                float vDepth = depth.get_distance(vPixeldepth[0], vPixeldepth[1]);
                float vPoint[3];
                rs2_deproject_pixel_to_point(vPoint, &_depth_intrin, vPixeldepth, vDepth);
                // vPoint[0] = vPixeldepth[0];
                // vPoint[1] = vPixeldepth[1];
                // vPoint[2] = vDepth;
                float uPixel[2];
                uPixel[0] = static_cast<float>(pts2[i].x);
                uPixel[1] = static_cast<float>(pts2[i].y);
                if (vDepth > minDepth && vDepth < maxDepth) {
                    vObjectPoints.push_back(cv::Point3f(vPoint[0], vPoint[1], vPoint[2]));
                    vImagePoints.push_back(cv::Point2f(vPixel[0], vPixel[1]));
                    uImagePoints.push_back(cv::Point2f(uPixel[0], uPixel[1]));
                }
            }
            cv::Mat tvec1, tvec2, rvec1, rvec2;
            if (vObjectPoints.size() > min3DPoints) {
                cv::solvePnPRansac(vObjectPoints,
                                    vImagePoints,
                                    cameraMatrix,
                                    distCoeffs,
                                    rvec1,
                                    tvec1,
                                    false,
                                    2000,
                                    5.0f,
                                    0.999,
                                    cv::noArray(),
                                    cv::SOLVEPNP_ITERATIVE);
                cv::solvePnPRansac(vObjectPoints,
                                    uImagePoints,
                                    cameraMatrix,
                                    distCoeffs,
                                    rvec2,
                                    tvec2,
                                    false,
                                    2000,
                                    5.0f,
                                    0.999,
                                    cv::noArray(),
                                    cv::SOLVEPNP_ITERATIVE);

                cv::Mat R1, R2;
                Rodrigues(rvec1, R1);
                Rodrigues(rvec2, R2);

                cv::Mat R_1to2, t_1to2;
                computeC2MC1(R1, tvec1, R2, tvec2, R_1to2, t_1to2);

                float distanceX = cv::norm(t_1to2.at<double>(0)-t_prev.at<double>(0));
                float distanceY = cv::norm(t_1to2.at<double>(1)-t_prev.at<double>(1));
                float distanceZ = cv::norm(t_1to2.at<double>(2)-t_prev.at<double>(2));
                if ((distanceX < maxDistanceF2F) && (distanceY < maxDistanceF2F) && (distanceZ < maxDistanceF2F)) {
                    cv::Mat rvec_1to2;
                    Rodrigues(R_1to2, rvec_1to2);
                    if (is_loop && frames_after_loop >= quantity_frames) {
                        frames_after_loop = 0;
                        t_f = t_1to2 + container.getKeyframe(bestKeyframeId)->getPose();
                        std::cout << "In loop closure" << std::endl;
                    } else {
                        if (no_move_counter <= 50) {
                            t_f += t_1to2;
                            std::cout << "Camera is moving" << std::endl;
                        } else {
                            std::cout << "Camera not moving" << std::endl;
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
            // int x = static_cast<int>((t_f.at<double>(0) / 5) * width);
            // int y = static_cast<int>((t_f.at<double>(2) / 5) * height);
            // cv::circle(traj, cv::Point(x+ width / 2, y+height/2) ,1, CV_RGB(0,255,0), 2);
            }
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
    // auto end_time = std::chrono::high_resolution_clock::now();
    // auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end_time - start_time);
    // durations.push_back(duration);
    // std::cout << "Duration: " << duration.count() << " milliseconds" << std::endl;

    // std::string fps_text = std::to_string(duration.count()) + " ms";

        // Put the text on the image
    // cv::putText(colorMat, fps_text, cv::Point(10, 30), cv::FONT_HERSHEY_SIMPLEX, 1, cv::Scalar(0, 0, 255), 2);

    // int imageWidth = imageFeatures.cols;
    // int imageHeight = imageFeatures.rows;

    // cv::Mat canvas = cv::Mat::zeros(2 * imageHeight, 2 * imageWidth, imageFeatures.type());

    // cv::Mat colorGrayscaleImage;
    // cv::cvtColor(grayImage, colorGrayscaleImage, cv::COLOR_GRAY2BGR);
    // // cv::Mat prueba;
    // // cv::cvtColor(traj, prueba, cv::COLOR_GRAY2BGR);
    // // Copy each image onto the canvas at the desired positions
    // if ( !colorMat.data || !colorGrayscaleImage.data || !imageFeatures.data || !traj.data ) {
    // std::cout<< " --(!) Error reading images " << std::endl;
    // } else {
    //     colorMat.copyTo(canvas(cv::Rect(0, 0, imageWidth, imageHeight)));
    //     colorGrayscaleImage.copyTo(canvas(cv::Rect(imageWidth, 0, imageWidth, imageHeight)));
    //     imageFeatures.copyTo(canvas(cv::Rect(0, imageHeight, imageWidth, imageHeight)));
    //     traj.copyTo(canvas(cv::Rect(imageWidth, imageHeight, imageWidth, imageHeight)));
    //     cv::imshow("Concatenated Images", canvas);
    // }
    auto total_time2 = std::chrono::high_resolution_clock::now();
    auto total_duration = std::chrono::duration_cast<std::chrono::milliseconds>(total_time2 - total_time1);
    __android_log_print(ANDROID_LOG_INFO, "Unity", "Total Duration: %lld milliseconds", total_duration.count());
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
    maxFeaturesSolver = config.maxFeaturesSolver;
    clipLimit = config.clipLimit;
    tilesGridSize = config.tilesGridSize;
    filterTemplateWindowSize = config.filterTemplateWindowSize;
    filterSearchWindowSize = config.filterSearchWindowSize;
    filterStrengH = config.filterStrengH;
    gamma_ = config.gamma_;
}

void addNewKeyFrame() {
    t_f = cv::Mat::zeros(3, 1, CV_64F);
}

// extern "C" const uchar* getJpegBuffer(int frameType, int* bufferSize) {

//     std::vector<uchar> jpegBuffer;
//     if (frameType == 0) {
//         cv::imencode(".jpeg", image2show, jpegBuffer, std::vector<int>{cv::IMWRITE_JPEG_QUALITY, 100});
//     } else if (frameType == 1) {
//         cv::imencode(".jpeg", grayImage, jpegBuffer, std::vector<int>{cv::IMWRITE_JPEG_QUALITY, 100});
//     }
//     // Allocate memory in Unity for the buffer and copy the JPEG data
//     uchar* unityBuffer = new uchar[jpegBuffer.size()];
//     memcpy(unityBuffer, jpegBuffer.data(), jpegBuffer.size());
//     // Set the buffer size
//     *bufferSize = jpegBuffer.size();

//     return unityBuffer;
// }



// int main(int argc, char const *argv[]) {
//     bool record = false;
//     int fps_color = 60;
//     int fps_depth = 60;
//     int width = 640;
//     int height = 480;
//     int width_depth = 640;
//     int height_depth = 480;
//     if (record) {
//         std::string bagFileAddress = "../20230921_163816.bag";
//         cfg.enable_device_from_file(bagFileAddress, false);
//     } else {
//         colorStreamConfig(width, height, fps_color);
//         depthStreamConfig(width_depth, height_depth, fps_depth);
//     }
//     initCamera();
//     initImu();

//     CameraConfig config;
//     config.ratioTresh = 0.5;
//     config.minDepth = 0.6;
//     config.maxDepth = 6;
//     config.min3DPoints = 10;
//     config.maxDistanceF2F = 0.3;
//     config.maxFeaturesSolver = -1;
//     config.clipLimit = 0.5;
//     config.tilesGridSize = 7;
//     config.filterTemplateWindowSize = 5;
//     config.filterSearchWindowSize = 2;
//     config.filterStrengH = 15;
//     config.gamma_ = 0.4;
//     setParams(config);
    
//     int nfeatures = 1000;
//     float scaleFactor = 2;
//     int nlevels = 3;
//     int edgeThreshold = 19;
//     int firstLevel = 0;
//     int WTA_K = 2;
//     int scoreType = 0;
//     int patchSize = 31;
//     int fastThreshold = 20;
//     createORB(nfeatures,
//               scaleFactor,
//               nlevels,
//               edgeThreshold,
//               firstLevel,
//               WTA_K,
//               scoreType,
//               patchSize,
//               fastThreshold);
//     // traj = cv::Mat::zeros(height, width, CV_8UC3);
//     // int rows = 10;
//     // int cols = 10;
//     // // Calculate the width and height of each cell
//     // int cellWidth = traj.cols / cols;
//     // int cellHeight = traj.rows / rows;
//     // // Draw vertical lines
//     // for (int i = 1; i < cols; ++i) {
//     //     int x = i * cellWidth;
//     //     cv::line(traj, cv::Point(x, 0), cv::Point(x, traj.rows), cv::Scalar(255, 255, 255), 1);
//     // }
//     // // Draw horizontal lines
//     // for (int i = 1; i < rows; ++i) {
//     //     int y = i * cellHeight;
//     //     cv::line(traj, cv::Point(0, y), cv::Point(traj.cols, y), cv::Scalar(255, 255, 255), 1);
//     // }
//     auto measure_init_time = std::chrono::steady_clock::now();
//     // const int max_duration_seconds = 30;
//     bool should_break = false;
//     firstIteration();
//     while (!should_break) {
//         auto current_time = std::chrono::steady_clock::now();
//         auto elapsed_seconds = std::chrono::duration_cast<std::chrono::seconds>(current_time - measure_init_time).count();
//         // // Check if 30 seconds have passed, and if so, break the loop
//         // if (elapsed_seconds >= max_duration_seconds)
//         // {
//         //     should_break = true;
//         // }
//         findFeatures();
//         float cameraAngle[3] = {0.0f, 0.0f, 0.0f};

//         // Call the GetCameraOrientation function
//         GetCameraOrientation(cameraAngle);

//         // Output the calculated camera angles
//         std::cout << "Camera Angle X: " << cameraAngle[0] << " degrees" << std::endl;
//         std::cout << "Camera Angle Y: " << cameraAngle[1] << " degrees" << std::endl;
//         std::cout << "Camera Angle Z: " << cameraAngle[2] << " degrees" << std::endl;

//         int key = cv::waitKey(1);
//         if (key >= 0)
//         {
//             if (key == 113) {
//                 should_break = true;
//             }
//          else if (key == 99) {
//                 addKeyFrame = true;  // Set the flag to true
//             }
//         }
//     }
//     long long total_duration = 0;
//     // Start from the second element (index 1) to exclude the first value
//     for (size_t i = 1; i < durations.size(); ++i) {
//         total_duration += durations[i].count();
//     }
//     double average_duration = static_cast<double>(total_duration) / (durations.size()-1);
//     std::cout << "Average Duration: " << average_duration << " milliseconds" << std::endl;
//     while (true) {
//         int key = cv::waitKey(1);
//         if (key >= 0) {
//             if (key == 113) {
//                 break; // Break the second loop when a key is pressed
//             }
//         }
//     }
//     // Calculate the average duration
//     cleanupCamera();
//     return 0;
// }

