using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosFrame = RosMessageTypes.MagicLantern.FrameMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class ImageDisplay : MonoBehaviour
{
    public GameObject image;

    void Start()
    {
       
        ROSConnection.GetOrCreateInstance().Subscribe<RosFrame>("image", ShowImage);
       
    }

    void ShowImage(RosFrame imageMessage)
    { 
        print("the width of the image is: " + imageMessage.width); 
        
    }
}