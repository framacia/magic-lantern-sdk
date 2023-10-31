#ifndef CAMERA_MOTION_H
#define CAMERA_MOTION_H

#include <opencv2/core.hpp>
#include <Eigen/Core>
#include <Eigen/Geometry>
// #include <opencv2/xfeatures2d.hpp>
#include <vector>
#include <memory>


class Keyframe {
public:
    // Default constructor
    Keyframe() {
        // Initialize member variables with default values or leave them uninitialized.
    }

    // Parameterized constructor
    Keyframe(int id, const cv::Mat& frame, const cv::Mat& descriptors,
             const std::vector<cv::KeyPoint>& keypoints, const cv::Mat& worldTranslation,
             const std::string& imageName, const cv::Mat& relativeTrans)
        : id(id), frame(frame), descriptors(descriptors), keypoints(keypoints),
          worldTranslation(worldTranslation), imageName(imageName), relativeTrans(relativeTrans) {
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
        return worldTranslation;
    }

    std::string getImageName() const {
        return imageName;
    }

    cv::Mat getRelativeTrans() const {
        return relativeTrans;
    }

    void serialize(cv::FileStorage& fs) const {
        fs << "{" << "id" << id;
        fs << "frame" << frame;
        fs << "descriptors" << descriptors;
        fs << "keypoints" << "[";
        for (const auto& keypoint : keypoints) {
            fs << "{:" << "x" << keypoint.pt.x << "y" << keypoint.pt.y << "}";
        }
        fs << "]";
        fs << "worldTranslation" << worldTranslation;
        fs << "imageName" << imageName;
        fs << "relativeTrans" << relativeTrans;
        fs << "}";
    }

    void deserialize(cv::FileNode node) {
        node["id"] >> id;
        node["frame"] >> frame;
        node["descriptors"] >> descriptors;
        cv::FileNode keypointsNode = node["keypoints"];
        for (cv::FileNodeIterator it = keypointsNode.begin(); it != keypointsNode.end(); ++it) {
            cv::KeyPoint keypoint;
            (*it)["x"] >> keypoint.pt.x;
            (*it)["y"] >> keypoint.pt.y;
            keypoints.push_back(keypoint);
        }
        node["worldTranslation"] >> worldTranslation;
        node["imageName"] >> imageName;
        node["relativeTrans"] >> relativeTrans;
    }

public:
    int id;
    cv::Mat frame;
    cv::Mat descriptors;
    std::vector<cv::KeyPoint> keypoints;
    cv::Mat worldTranslation;
    std::string imageName;
    cv::Mat relativeTrans;
};

class KeyframeContainer {
public:
    std::vector<std::shared_ptr<Keyframe>> keyframes;

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

    int getKeyframeCount() const {
        return keyframes.size();
    }

    void clear() {
        keyframes.clear();
    }

    void serialize(const std::string& filename) const {
        cv::FileStorage fs(filename, cv::FileStorage::WRITE);
        fs << "Keyframes" << "[";
        for (const std::shared_ptr<Keyframe>& keyframe : keyframes) {
            keyframe->serialize(fs);
        }
        fs << "]";
        fs.release();
    }

    void deserialize(const std::string& filename) {
        cv::FileStorage fs(filename, cv::FileStorage::READ);
        cv::FileNode keyframesNode = fs["Keyframes"];
        keyframes.clear();
        for (cv::FileNodeIterator it = keyframesNode.begin(); it != keyframesNode.end(); ++it) {
            std::shared_ptr<Keyframe> keyframe = std::make_shared<Keyframe>();
            keyframe->deserialize(*it);
            keyframes.push_back(keyframe);
        }
        fs.release();
    }
};

struct systemConfig {
    float ratioTresh;
    float minDepth;
    float maxDepth;
    int min3DPoints;
    float maxDistanceF2F;
    int minFeaturesLoopClosure;
    int framesUntilLoopClosure;
    float noMovementThresh;
    int framesNoMovement;
    int maxGoodFeatures;
};

void bestMatchesFilter(std::vector<cv::DMatch> goodMatches, std::vector<cv::DMatch>& bestMatches);

void matchingAndFilteringByDistance(cv::Mat descriptors1, std::vector<cv::KeyPoint> kp1Filtered, std::vector<cv::Point2f>& pts1, std::vector<cv::Point2f>& pts2);

void filterKeypointsByROI(std::vector<cv::KeyPoint> keypoints, cv::Mat descriptors, std::vector<cv::KeyPoint> &filteredKeypoints, cv::Mat& filteredDescriptors, cv::Rect &zone);

void featureDetection(cv::Mat img, std::vector<cv::KeyPoint>& keypoints1, cv::Mat& descriptors1);

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
    float getDepthAtCenter();
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

    void setParams(systemConfig config); 		

    const uchar* getJpegBuffer(int* bufferSize);	

    void resetOdom();

    void addKeyframe();
    
    
    

#ifdef __cplusplus
}
#endif


#endif // CAMERA_MOTION_H
