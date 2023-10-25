using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HumanoidIKLook : MonoBehaviour
{
    Animator animator;
    [SerializeField] GameObject target;
    [SerializeField] bool followMainCamera;
    [SerializeField, Range(0, 1)] float lookAtWeight = 1;
    [SerializeField] Vector3 rotationLimit = new Vector3(0.4f, 0.6f, 0.3f);
    [SerializeField, Range(0, 10)] float distanceLimit = 2.6f;
    [SerializeField] float dummyPivotHeightOffset = 1.7f;

    //Dummy pivot
    GameObject objPivot;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        //Dummy pivot
        objPivot = new GameObject("DummyPivot");
        objPivot.transform.parent = transform;
        objPivot.transform.localPosition = new Vector3(0, dummyPivotHeightOffset, 0);

        if (followMainCamera)
            target = Camera.main?.gameObject;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (followMainCamera)
            target = Camera.main?.gameObject;

        if (!target)
            return;

        objPivot.transform.LookAt(target.transform);

        //Target distance limit
        float distance = Vector3.Distance(objPivot.transform.position, target.transform.position);

        //Target rotation limit
        float pivotRotY = objPivot.transform.localRotation.y;

        if (Mathf.Abs(pivotRotY) > rotationLimit.y || distance > distanceLimit)
        {
            //lookAtWeight = 0f; //Stop tracking
            lookAtWeight = Mathf.Lerp(lookAtWeight, 0, Time.deltaTime * 1.5f); //Stop tracking
        }
        else
        {
            lookAtWeight = Mathf.Lerp(lookAtWeight, 1, Time.deltaTime * 1.5f);
        }
    }

    private void OnAnimatorIK()
    {
        if (!animator || target == null) return;

        animator.SetLookAtWeight(lookAtWeight);
        animator.SetLookAtPosition(target.transform.position);
    }
}
