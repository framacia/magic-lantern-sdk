#ifndef REALSENSECAMERA_H
#define REALSENSECAMERA_H

#include <librealsense2/rs.hpp>
#include <debugCPP.h>


class RealSenseCamera {
public:
    RealSenseCamera();
    ~RealSenseCamera();

    void initCamera();
    void initImu();
    void cleanupCamera();
    void bagFileStreamConfig(const char* bagFileAddress);
    void colorStreamConfig(int width, int height, int fps);
    void depthStreamConfig(int width, int height, int fps);
    rs2::frameset waitForFrames();
    rs2::pipeline_profile getProfile();


    // Add more methods to access parameters and frames as needed

private:
    rs2::pipeline pipeline;
    rs2::pipeline_profile profile;
    rs2::config cfg;
    rs2::pipeline imu_pipeline;
    rs2::config imu_cfg;
    rotation_estimator algo;
};


#ifdef __cplusplus
extern "C" {
    #endif

    void colorStreamConfig(int width, int height, int fps);
    void depthStreamConfig(int width, int height, int fps);
    void bagFileStreamConfig(const char* bagFileAddress);
    void initCamera();
    void cleanupCamera();
    
#ifdef __cplusplus
}
#endif

#endif // REALSENSECAMERA_H