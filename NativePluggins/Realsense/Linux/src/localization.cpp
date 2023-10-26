#include <localization.h>
#include <camera_motion.h>

int findBestMatchingKeyframe(cv::Mat descriptors1, std::vector<cv::KeyPoint> kp1Filtered, std::vector<cv::Point2f>& pts1, std::vector<cv::Point2f>& pts2) {
    int bestKeyframeId = -1;
    int mostGoodMatches = 0;
    std::vector<std::vector<cv::DMatch>> matches;
    std::vector<cv::DMatch> goodMatches;

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
        if (goodMatches_aux.size() >= minFeaturesLoopClosure && goodMatches_aux.size() > mostGoodMatches) {
            mostGoodMatches = goodMatches.size();
            bestKeyframeId = keyframeIndex;
            goodMatches = goodMatches_aux;
        }
    }

    if (bestKeyframeId != -1) {
        const auto& kpKeyframe = container.getKeyframe(bestKeyframeId)->getKeypoints();
        std::vector<cv::DMatch> best_matches;
        bestMatchesFilter(goodMatches, best_matches);
        for (const cv::DMatch &match : best_matches) {
            pts1.push_back(kp1Filtered[match.queryIdx].pt);
            pts2.push_back(kpKeyframe[match.trainIdx].pt);
            circle(imageFeatures, pts2.back(), 1, cv::Scalar(255, 0, 0), 1);
            cv::Point2f pt1 = pts1.back();
            cv::Point2f pt2 = pts2.back();
            line(imageFeatures, pt1, pt2, cv::Scalar(255, 0, 0), 1);
        }
    }

    return bestKeyframeId;
}
