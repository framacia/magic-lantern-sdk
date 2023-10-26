#include <reprojection.h>
#include <camera_motion.h>
#include <librealsense2/rs.hpp>

void pixelToPoint(std::vector<cv::Point2f> pts1, std::vector<cv::Point2f> pts2) {
    std::vector<cv::Point2f> uImagePoints, vImagePoints;
    std::vector<cv::Point3f> vObjectPoints;
    for (size_t i = 0; i < pts1.size(); ++i) {
        float vPixel[2];
        vPixel[0] = static_cast<float>(pts1[i].x);
        vPixel[1] = static_cast<float>(pts1[i].y);
        float vPixeldepth[2];
        rs2_project_color_pixel_to_depth_pixel(vPixeldepth, depth_data_uint16, depth_scale, minDepth, maxDepth, &_depth_intrin, &_color_intrin, &depth_to_color_extrin, &color_to_depth_extrin, vPixel);
        float vDepth = depth.get_distance(vPixeldepth[0], vPixeldepth[1]);
        float vPoint[3];
        rs2_deproject_pixel_to_point(vPoint, &_depth_intrin, vPixeldepth, vDepth);
        float uPixel[2];
        uPixel[0] = static_cast<float>(pts2[i].x);
        uPixel[1] = static_cast<float>(pts2[i].y);
        if (vDepth > minDepth && vDepth < maxDepth) {
            vObjectPoints.push_back(cv::Point3f(vPoint[0], vPoint[1], vPoint[2]));
            vImagePoints.push_back(cv::Point2f(vPixel[0], vPixel[1]));
            uImagePoints.push_back(cv::Point2f(uPixel[0], uPixel[1]));
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
float vDepth = depth.get_distance(vPixeldepth[0], vPixeldepth[1]);
float vPoint[3];
rs2_deproject_pixel_to_point(vPoint, &_depth_intrin, vPixeldepth, vDepth);
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