using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosFrame = RosMessageTypes.MagicLantern.FrameCompressedMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class ImageDisplay : MonoBehaviour
{
    public GameObject quad;

    void Start()
    {
       
        ROSConnection.GetOrCreateInstance().Subscribe<RosFrame>("image", ShowImage);
       
    }

    void ShowImage(RosFrame imageMessage)
    { 

      // Create a texture. Texture size does not matter, since
        // LoadImage will replace with with incoming image size.
        Texture2D tex = new Texture2D(320, 240);
        tex.LoadImage(imageMessage.data);
        GetComponent<Renderer>().material.mainTexture = tex;
    }
}