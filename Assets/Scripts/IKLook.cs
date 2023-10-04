using UnityEngine;

public class IKLook : MonoBehaviour
{
    [SerializeField] private Transform bone;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 rotationLimit = new Vector3(180, 180, 180);
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Vector3 rotationOffset;
    //public Vector3 StartDirection;
    private Quaternion startRotation;

    void Awake()
    {
        if (target == null)
            return;

        //StartDirection = target.position - transform.position;
        startRotation = bone.rotation;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 direction = (target.position - bone.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction) * startRotation;

        //If any of the axis goes above limit, stay the same
        float limitedLookRotationX = Mathf.Abs(bone.rotation.x - startRotation.x) > rotationLimit.x ? bone.rotation.x : lookRotation.x;
        float limitedLookRotationY = Mathf.Abs(bone.rotation.y - startRotation.y) > rotationLimit.y ? bone.rotation.y : lookRotation.y;
        float limitedLookRotationZ = Mathf.Abs(bone.rotation.z - startRotation.z) > rotationLimit.z ? bone.rotation.z : lookRotation.z;

        bone.eulerAngles = new Vector3(limitedLookRotationX, limitedLookRotationY, limitedLookRotationZ);
        //bone.rotation = Quaternion.Slerp(bone.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}

