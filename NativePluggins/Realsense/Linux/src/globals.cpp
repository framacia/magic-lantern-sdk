#include <globals.h>

RealSenseCamera camera;
rotation_estimator algo;

cv::Ptr<cv::Feature2D> featureExtractor;
cv::Ptr<cv::Feature2D> featureDescriptor;
cv::Ptr<cv::FlannBasedMatcher> matcher;

cv::Mat imgColorPrev;
cv::Mat colorMat;
std::vector<cv::KeyPoint> prevFeatures;
cv::Mat prevDescriptors;
cv::Mat t_f =  cv::Mat::zeros(3, 1, CV_64F);
cv::Mat imageFeatures;
KeyframeContainer container;

int no_move_counter = 0;
int frames_after_loop = 100;
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
bool addKeyFrame = false;
