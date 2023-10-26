#ifndef LOCALIZATION_H
#define LOCALIZATION_H

#include <camera_motion.h>
#include <opencv2/opencv.hpp>
#include <librealsense2/rs.hpp>
#include <globals.h>

#include <vector>
#include <string>
#include <chrono>
#include <iostream>

int findBestMatchingKeyframe(cv::Mat descriptors1, std::vector<cv::KeyPoint> kp1Filtered, std::vector<cv::Point2f>& pts1, std::vector<cv::Point2f>& pts2);

#endif // LOCALIZATION_H
