using System.Security.Permissions;
using UnityEngine;

public class IKLook : MonoBehaviour
{
    [SerializeField] private Transform bone;
    [SerializeField] private Transform target;
    //[SerializeField] private Vector3 rotationLimit = new Vector3(120, 120, 120);
    [SerializeField] private float rotationLimit = 60f;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Vector3 rotationOffset;
    //public Vector3 StartDirection;
    private Quaternion startBoneRotation;
    private Vector3 startParentRotation;
    private float previousBoneRotY;

    void Start()
    {
        if (target == null)
            return;

        //StartDirection = target.position - transform.position;
        startBoneRotation = bone.rotation;
        startParentRotation = transform.eulerAngles;

        previousBoneRotY = bone.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 direction = (target.position - bone.position).normalized;
        Vector3 lookRotation = (Quaternion.LookRotation(direction) * startBoneRotation).eulerAngles;

        //Account for initial rotation
        bone.eulerAngles = lookRotation - startParentRotation;
        //Remap to invert X axis
        bone.eulerAngles = new Vector3(-bone.eulerAngles.x, bone.eulerAngles.y, bone.eulerAngles.z);

        float currentBoneTransformDiffY = Mathf.DeltaAngle(bone.eulerAngles.y, transform.localEulerAngles.y) - 90;

        print(currentBoneTransformDiffY);

        if (currentBoneTransformDiffY > rotationLimit || currentBoneTransformDiffY < -rotationLimit)
        {
            //bone.eulerAngles = new Vector3(bone.eulerAngles.x, previousBoneRotY, bone.eulerAngles.z);
            //bone.eulerAngles = previousBoneRotation;

            bone.localEulerAngles = new Vector3(0, 0, 0);
        }
        else //If rotation is within acceptable range, overwrite previousBoneTransformDiffY
        {
            if (bone.localEulerAngles.x > rotationLimit || bone.localEulerAngles.x < -rotationLimit)
                return;

            previousBoneRotY = bone.eulerAngles.y;
        }
    }
}

