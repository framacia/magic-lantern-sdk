using System.Security.Permissions;
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
    private float previousBoneTransformDiffY;

    void Start()
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

        Vector3 direction = (target.position - bone.position).normalized;
        Vector3 lookRotation = (Quaternion.LookRotation(direction) * startBoneRotation).eulerAngles;

        //Account for initial rotation
        bone.eulerAngles = lookRotation - startParentRotation;
        //Remap to invert X axis
        bone.eulerAngles = new Vector3(-bone.eulerAngles.x, bone.eulerAngles.y, bone.eulerAngles.z);

        float currentBoneTransformDiffY = ((bone.eulerAngles.y - transform.localEulerAngles.y - 180) + 360) % 360;

        if (currentBoneTransformDiffY > 190 || currentBoneTransformDiffY < 0)
        {
            bone.eulerAngles = new Vector3(bone.eulerAngles.x, previousBoneTransformDiffY, bone.eulerAngles.z);
            //bone.eulerAngles = previousBoneRotation;
        }
        else //If rotation is within acceptable range, overwrite previousBoneTransformDiffY
        {
            previousBoneTransformDiffY = currentBoneTransformDiffY;
        }

        //if (bone.localEulerAngles.x > 30 || bone.localEulerAngles.x < -30)
        //{
        //    bone.eulerAngles = new Vector3(bone.eulerAngles.x, previousBoneTransformDiffY, bone.eulerAngles.z);
        //}
        //else
        //{
        //    previousBoneTransformDiffY = currentBoneTransformDiffY;
        //}

        //print("prev: " + previousBoneTransformDiffY);
        //print("bone: " + bone.eulerAngles.y);
        //print("diff: " + currentBoneTransformDiffY);
    }
}

