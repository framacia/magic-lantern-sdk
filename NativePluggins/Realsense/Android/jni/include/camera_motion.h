#ifndef CAMERA_MOTION_H
#define CAMERA_MOTION_H

// #include <jni.h>
#include <android/log.h>
#include <librealsense2/rs.hpp>

#include <vector>
#include <string>
#include <chrono>
#include <iostream>
#include <queue>

#include "opencv2/core/core_c.h"
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/features2d.hpp>
#include <opencv2/calib3d.hpp>


void featureDetection(cv::Mat img, std::vector<cv::KeyPoint>& keypoints1, cv::Mat& descriptors1);

void computeC2MC1(const cv::Mat &R1, const cv::Mat &tvec1, const cv::Mat &R2, const cv::Mat &tvec2,
                  cv::Mat &R_1to2, cv::Mat &tvec_1to2);

#ifdef __cplusplus
extern "C" {
    #endif

    void colorStreamConfig(int width, int height, int fps);
    void depthStreamConfig(int width, int height, int fps);
    void bagFileStreamConfig(const char* bagFileAddress);
    void initCamera();
    void findFeatures();
    float GetDepthAtCenter();
    void CleanupCamera();
    void GetTranslationVector(float* t_f_data);
    
    void createORB(int  	nfeatures,
                   float  	scaleFactor,
                   int  	nlevels,
                   int  	edgeThreshold,
                   int  	firstLevel,
                   int  	WTA_K,
                   int  	scoreType,
                   int  	patchSize,
                   int  	fastThreshold); 	
    
    void createSIFT(int  	nfeatures,
		            int  	nOctaveLayers,
		            double  	contrastThreshold,
		            double  	edgeThreshold,
		            double  	sigma,
		            bool  	enable_precise_upscale);

    void createFAST_BRIEF(int  	nfeatures,
		            int  	nOctaveLayers,
		            double  	contrastThreshold,
		            double  	edgeThreshold,
		            double  	sigma,
		            bool  	enable_precise_upscale);

    void setParams(float newRatioTresh, float newMinDepth,
                    float newMaxDepth, int newMin3DPoints,
                    float newMaxDistanceF2F, int newMaxFeaturesSolver,
                    float newClipLimit, int newTilesGridSize,
                    int newFilterTemplateWindowSize, float newFilterSearchWindowSize,
                    int newFilterStrengH, float newGamma); 		

    const uchar* getJpegBuffer(int frameType, int* bufferSize);	
    

#ifdef __cplusplus
}
#endif


#endif // CAMERA_MOTION_H
