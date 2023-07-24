using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosPose = RosMessageTypes.MagicLantern.PosRotMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class PoseFollower : MonoBehaviour
{
    public GameObject cube;
    public Vector3 rosStartEuler = Vector3.zero;
    public Vector3 camStartEuler;

    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<RosPose>("pose", PoseChange);
        camStartEuler = cube.transform.localEulerAngles;
    }

    void PoseChange(RosPose poseMessage)
    {
        if (!RosErrorFlagReader.noError)
        {
            return;
        }


        // Vector3 rosPos = new Vector3(-poseMessage.pos_y, poseMessage.pos_z, poseMessage.pos_x);
        Quaternion rosRot = new Quaternion(poseMessage.rot_y, -poseMessage.rot_z, -poseMessage.rot_x, poseMessage.rot_w);
        if (rosStartEuler == Vector3.zero)
        {
            rosStartEuler = rosRot.eulerAngles;
            Debug.Log(rosStartEuler);
            return;
        }

        Vector3 eulerDiff = rosRot.eulerAngles - rosStartEuler;
        cube.transform.localRotation = Quaternion.Euler(camStartEuler + eulerDiff);

        //TODO Code here to check the framerate of the Pose Updates
    }
}