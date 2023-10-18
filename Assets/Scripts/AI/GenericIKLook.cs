using System.Security.Permissions;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;

public class GenericIKLook : MonoBehaviour
{
    [SerializeField] private Transform bone;
    [SerializeField] private Transform target;
    [SerializeField] private float rotationLimit = 60f;
    [SerializeField] private float lerpDuration;
    [SerializeField] private Vector3 rotationOffset;
    private Quaternion startBoneRotation;
    private Vector3 startParentRotation;
    private bool lerpRunning;
    private bool currentlyOverLimit;

    void Start()
    {
        if (target == null)
            return;

        startBoneRotation = bone.rotation;
        startParentRotation = transform.eulerAngles;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        //If currently lerping, let it finish
        if (lerpRunning) return;

        //Calculate angle between agent body transform and target object
        float yRotationDiff = GetYRotationToTarget(transform.gameObject, target.gameObject);

        //If above rotation limit, lerp to 0
        if (Mathf.Abs(yRotationDiff) > rotationLimit)
        {
            //This check is used so we can lerp to the first valid target rotation AFTER it has been over limit (first look at rotation after resetting to 0)
            currentlyOverLimit = true;

            //Start lerping to 0
            StartCoroutine(LerpRotation(Quaternion.identity, lerpDuration, true));
            lerpRunning = true;
        }
        else
        {
            Vector3 direction = (target.position - bone.position).normalized;
            Vector3 lookRotation = (Quaternion.LookRotation(direction) * startBoneRotation).eulerAngles;

            //Account for initial rotation
            lookRotation = lookRotation - startParentRotation;
            //Remap to invert X axis
            lookRotation = new Vector3(-lookRotation.x, lookRotation.y, lookRotation.z);

            if (!currentlyOverLimit)
            {
                bone.eulerAngles = lookRotation;
                //This check is used so we can lerp to the first valid target rotation AFTER it has been over limit (first look at rotation after resetting to 0)
                currentlyOverLimit = false;
            }
            else
            {
                StartCoroutine(LerpRotation(Quaternion.Euler(lookRotation), lerpDuration, false));
                lerpRunning = true;
            }
        }
    }

    IEnumerator LerpRotation(Quaternion targetRotation, float duration, bool isLocal)
    {
        float time = 0;
        Quaternion startRotation = Quaternion.identity;
        if (isLocal)
            startRotation = bone.localRotation;
        else
            startRotation = bone.rotation;

        while (time < duration)
        {
            if (isLocal)
                bone.localRotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            else
                bone.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
        if (isLocal)
            bone.localRotation = targetRotation;
        else
            bone.rotation = targetRotation;
        lerpRunning = false;
    }

    public static float GetYRotationToTarget(GameObject source, GameObject target)
    {
        // Get the local positions of the source and target with respect to their common parent.
        Vector3 localSourcePos = source.transform.InverseTransformPoint(target.transform.position);

        // Calculate the local rotation in degrees.
        float localYRotation = Mathf.Atan2(localSourcePos.x, localSourcePos.z) * Mathf.Rad2Deg;

        return localYRotation;
    }
}