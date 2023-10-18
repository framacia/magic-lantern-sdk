#ifndef CAMERA_MOTION_H
#define CAMERA_MOTION_H

#include <opencv2/core.hpp>
#include <opencv2/xfeatures2d.hpp>
#include <vector>


class Keyframe {
public:
    // Constructor
    Keyframe(int id, const cv::Mat& frame, const cv::Mat& descriptors,
             const std::vector<cv::KeyPoint>& keypoints, const cv::Mat& pose)
        : id(id), frame(frame), descriptors(descriptors), keypoints(keypoints), pose(pose) {
    }

    // Getter functions for member variables
    int getId() const {
        return id;
    }

    cv::Mat getFrame() const {
        return frame;
    }

    cv::Mat getDescriptors() const {
        return descriptors;
    }

    std::vector<cv::KeyPoint> getKeypoints() const {
        return keypoints;
    }

    cv::Mat getPose() const {
        return pose;
    }

private:
    int id;
    cv::Mat frame;
    cv::Mat descriptors;
    std::vector<cv::KeyPoint> keypoints;
    cv::Mat pose;
};

class KeyframeContainer {
public:
    void addKeyframe(std::shared_ptr<Keyframe> keyframe) {
        keyframes.push_back(keyframe);
    }
    
    std::shared_ptr<Keyframe> getKeyframe(int index) const {
        if (index >= 0 && index < keyframes.size()) {
            return keyframes[index];
        } else {
            throw std::out_of_range("Index out of bounds");
        }
    }

    // Get the total number of keyframes in the container
    int getKeyframeCount() const {
        return keyframes.size();
    }

    void clear() {
        keyframes.clear();
    }

private:
    std::vector<std::shared_ptr<Keyframe>> keyframes; // Use shared_ptr
};

struct CameraConfig {
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
};

void featureDetection(cv::Mat img, std::vector<cv::KeyPoint>& keypoints1, cv::Mat& descriptors1);

void computeC2MC1(const cv::Mat &R1, const cv::Mat &tvec1, const cv::Mat &R2, const cv::Mat &tvec2,
                  cv::Mat &R_1to2, cv::Mat &tvec_1to2);

int findBestMatchingKeyframe(const cv::Mat& descriptors1, std::vector<cv::DMatch>& goodMatches,
                             std::vector<std::vector<cv::DMatch>>& matches, cv::DescriptorMatcher& matcher);
void preprocessImage(cv::Mat& inputImage, cv::Mat& colorMat);

#ifdef __cplusplus
extern "C" {
    #endif

    void colorStreamConfig(int width, int height, int fps);
    void depthStreamConfig(int width, int height, int fps);
    void bagFileStreamConfig(const char* bagFileAddress);
    void initCamera();
    void initImu();
    void firstIteration();
    void findFeatures();
    // float GetDepthAtCenter();
    void cleanupCamera();
    void GetTranslationVector(float* t_f_data);
    void GetCameraOrientation(float* cameraAngle);
    
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

    void setParams(CameraConfig config); 		

    const uchar* getJpegBuffer(int frameType, int* bufferSize);	

    void addNewKeyFrame();
    
    
    

#ifdef __cplusplus
}
#endif


#endif // CAMERA_MOTION_H
