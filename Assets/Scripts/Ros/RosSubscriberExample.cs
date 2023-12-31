using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosColor = RosMessageTypes.MagicLantern.UnityColorMsg;


public class RosSubscriberExample : MonoBehaviour
{
    public GameObject cube;

    void Start()
    {
        
        ROSConnection.GetOrCreateInstance().Subscribe<RosColor>("color", ColorChange);
       
    }

    void ColorChange(RosColor colorMessage)
    {
        cube.GetComponent<Renderer>().material.color = new Color32((byte)colorMessage.r, (byte)colorMessage.g, (byte)colorMessage.b, (byte)colorMessage.a);
        cube.transform.Translate(0,0,colorMessage.r);
    }
}