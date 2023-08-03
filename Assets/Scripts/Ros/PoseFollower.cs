using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosPose = RosMessageTypes.MagicLantern.PosRotMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class PoseFollower : MonoBehaviour
{
    void Start()
    {        
        ROSConnection.GetOrCreateInstance().Subscribe<RosPose>("pose", PoseChange);
    }

    void PoseChange(RosPose poseMessage)
    { 
        if (!RosErrorFlagReader.noError)
        {
            return;
        }

        Vector3 rosPos = new Vector3(-poseMessage.pos_y, poseMessage.pos_z, poseMessage.pos_x);
        Quaternion rosRot = new Quaternion(poseMessage.rot_y, -poseMessage.rot_z,  -poseMessage.rot_x, poseMessage.rot_w);
        transform.position = rosPos;
        transform.rotation = rosRot;

        //TODO Code here to check the framerate of the Pose Updates
    }
}