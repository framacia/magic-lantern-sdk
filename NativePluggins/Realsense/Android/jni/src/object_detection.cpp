#include <globals.h>
#include <object_detection.h>




void readImagesInFolder(const std::string& folderPath, std::vector<cv::Mat>& images, std::vector<std::string>& imageNames) {
    cv::String pattern = folderPath + "/*.png";
    std::vector<cv::String> imagePaths;
    cv::glob(pattern, imagePaths);

    for (const cv::String& imagePath : imagePaths) {
        cv::Mat image = cv::imread(imagePath, cv::IMREAD_COLOR);
        if (!image.data) {
            std::cerr << "Could not open or find the image: " << imagePath << std::endl;
        } else {
            images.push_back(image);
            std::string imageName = imagePath.substr(imagePath.find_last_of("/") + 1);
            imageNames.push_back(imageName);
        }
    }
}

cv::Mat centerObject(cv::Mat objectImage) {
    int width = 640;
    int height = 480;
    cv::Mat objectFullImage(height, width, CV_8UC3, cv::Scalar(0, 0, 0));

    int smaller_width = objectImage.cols;
    int smaller_height = objectImage.rows;

    int x_offset = (width - smaller_width) / 2;
    int y_offset = (height - smaller_height) / 2;

    cv::Mat roi = objectFullImage(cv::Rect(x_offset, y_offset, smaller_width, smaller_height));

    objectImage.copyTo(roi);

    return objectFullImage;


}

void extractObjectsFeatures(std::vector<cv::Mat>& images, std::vector<std::string>& imageNames) {
     for (size_t i = 0; i < images.size(); i++) {
        int id = i;
        
        
        std::vector<cv::KeyPoint> kp1;
        cv::Mat descriptors1;
        
        featureDetection(images[i], kp1, descriptors1);
        std::string imageName = imageNames[i];
        std::shared_ptr<Object> object1 = std::make_shared<Object>(id,
                                                                   images[i].clone(),
                                                                   descriptors1.clone(),
                                                                   kp1,
                                                                   imageName);

        objectContainer.addObject(object1);
    }

}



int findObject(cv::Mat descriptors1, std::vector<cv::KeyPoint> kp1, std::vector<cv::Point2f>& pts1, std::vector<cv::Point2f>& pts2) {
    int bestObjectId = -1;
    int mostGoodMatches = 0;
    std::vector<std::vector<cv::DMatch>> matches;
    std::vector<cv::DMatch> goodMatches;
    std::vector<cv::DMatch> goodMatches_aux;

    const int objectCount = objectContainer.getObjectCount();
    for (int objectIndex = 0; objectIndex < objectCount; objectIndex++) {
        matcher->knnMatch(descriptors1, objectContainer.getObject(objectIndex)->getDescriptors(), matches, 2);

        if (!goodMatches_aux.empty()) {
            goodMatches_aux.clear();
        }

        for (size_t i = 0; i < matches.size(); i++) {
            if (matches[i].size() >= 2 && matches[i][0].distance < ratioTresh * matches[i][1].distance) {
                goodMatches_aux.push_back(matches[i][0]);
            }
        }

        matches.clear();


        if (goodMatches_aux.size() >= 30 && goodMatches_aux.size() > mostGoodMatches) {
            goodMatches.clear();
            bestObjectId = objectIndex;
            goodMatches = goodMatches_aux;
        }
    }

    if (bestObjectId != -1) {
        const auto& kpObject = objectContainer.getObject(bestObjectId)->getKeypoints();
        std::vector<cv::DMatch> best_matches;
        bestMatchesFilter(goodMatches, best_matches);
        
        if (!best_matches.empty()) {
            for (const cv::DMatch& match : best_matches) {
                pts1.push_back(kp1[match.queryIdx].pt);
                pts2.push_back(kpObject[match.trainIdx].pt);
                circle(imageFeatures, pts1.back(), 1, cv::Scalar(0, 0, 255), 5);
                cv::Point2f pt1 = pts1.back();
                cv::Point2f pt2 = pts2.back();
                line(imageFeatures, pt1, pt2, cv::Scalar(0, 0, 255), 1);
            }
            
        }
       
        
    }

    return bestObjectId;
}



