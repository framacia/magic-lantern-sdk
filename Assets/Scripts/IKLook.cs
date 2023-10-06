using UnityEngine;

public class IKLook : MonoBehaviour
{
    [SerializeField] private Transform bone;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 rotationLimit = new Vector3(120, 120, 120);
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Vector3 rotationOffset;
    //public Vector3 StartDirection;
    private Quaternion startBoneRotation;
    private Vector3 startParentRotation;

    void Awake()
    {
        if (target == null)
            return;

        //StartDirection = target.position - transform.position;
        startBoneRotation = bone.rotation;
        startParentRotation = transform.eulerAngles;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float prevBoneRotX = bone.eulerAngles.x;
        float prevBoneRotY = bone.eulerAngles.y;
        float prevBoneRotZ = bone.eulerAngles.z;

        Vector3 direction = (target.position - bone.position).normalized;
        Vector3 lookRotation = (Quaternion.LookRotation(direction) * startBoneRotation).eulerAngles;

        bone.eulerAngles = lookRotation - startParentRotation;
        if (Mathf.Abs((bone.localEulerAngles.x + 360) % 360 - (startBoneRotation.eulerAngles.x + 360) % 360) > rotationLimit.x)
        {
            print("local x " + (bone.localEulerAngles.x + 360) % 360);
            //bone.eulerAngles = new Vector3(prevBoneRotX, bone.eulerAngles.y, bone.eulerAngles.z);
            bone.eulerAngles = new Vector3(bone.eulerAngles.x, prevBoneRotY, bone.eulerAngles.z);
        }
        //if (Mathf.Abs((bone.localEulerAngles.y + 360) % 360 - (startBoneRotation.eulerAngles.y + 360) % 360) > rotationLimit.y)
        //{
        //    bone.eulerAngles = new Vector3(bone.eulerAngles.x, prevBoneRotY, bone.eulerAngles.z);
        //}
        //if (Mathf.Abs((bone.localEulerAngles.z + 360) % 360 - (startBoneRotation.eulerAngles.z + 360) % 360) > rotationLimit.z)
        //{
        //    bone.eulerAngles = new Vector3(bone.eulerAngles.x, bone.eulerAngles.y, prevBoneRotZ);
        //}

        //bone.localEulerAngles = new Vector3(
        //    Mathf.Clamp(bone.localEulerAngles.x, -360, rotationLimit.x),
        //    Mathf.Clamp(bone.localEulerAngles.y, -rotationLimit.y, rotationLimit.y),
        //    Mathf.Clamp(bone.localEulerAngles.z, -rotationLimit.z, rotationLimit.z));
    }
}

