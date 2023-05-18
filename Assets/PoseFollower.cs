using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosPose = RosMessageTypes.UnityRoboticsDemo.PosRotMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class PoseFollower : MonoBehaviour
{
    public GameObject cube;

    void Start()
    {
        
        ROSConnection.GetOrCreateInstance().Subscribe<RosPose>("pose", PoseChange);
       
    }

    void PoseChange(RosPose poseMessage)
    { 
        Vector3 rosPos = new Vector3(-poseMessage.pos_y, poseMessage.pos_z, poseMessage.pos_x);
        Quaternion rosRot = new Quaternion(poseMessage.rot_y, -poseMessage.rot_z,  -poseMessage.rot_x, poseMessage.rot_w);
        cube.transform.position = rosPos;
        cube.transform.rotation = rosRot;
    }
}