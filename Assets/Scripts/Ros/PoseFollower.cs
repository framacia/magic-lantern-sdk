using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosPose = RosMessageTypes.MagicLantern.PosRotMsg;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;


public class PoseFollower : MonoBehaviour
{
    public Vector3 rosStartEuler = Vector3.zero;
    public Vector3 camStartEuler;
    public Vector3 rosStartPos = Vector3.zero;
    public Vector3 camStartPos;

    Vector3 EulerRot(Quaternion qRot)
    {
        Vector3 rot = qRot.eulerAngles;
        return new Vector3(-rot.x, rot.y, rot.z);
    }

    void Start()
    {
        ROSConnection.GetOrCreateInstance().Subscribe<RosPose>("pose", PoseChange);
        camStartEuler = EulerRot(transform.localRotation);
        camStartPos = transform.localPosition;
    }

    void PoseChange(RosPose poseMessage)
    {
        // if (!RosErrorFlagReader.noError)
        // {
        //     return;
        // }

        Vector3 rosPos = new Vector3(-poseMessage.pos_y, poseMessage.pos_z, poseMessage.pos_x);
        Quaternion rosRot = new Quaternion(-poseMessage.rot_z, poseMessage.rot_y, -poseMessage.rot_x, poseMessage.rot_w);

        if (rosStartEuler == Vector3.zero)
        {
            rosStartEuler = EulerRot(rosRot);
            rosStartPos = rosPos;
            Debug.Log(rosStartEuler);
            return;
        }

        Vector3 eulerDiff = EulerRot(rosRot) - rosStartEuler;
        // Vector3 posDiff = rosPos - rosStartPos;
        transform.localRotation = Quaternion.Euler(camStartEuler + eulerDiff);

        //rosPos = Quaternion.AngleAxis(45, Vector3.right) * rosPos; //Apply 45 degree rotation on X looking down due to camera position on lantern
        transform.localPosition = rosPos - camStartPos;


        //TODO Code here to check the framerate of the Pose Updates
    }
}