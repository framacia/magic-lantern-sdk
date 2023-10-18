using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HumanoidIKLook : MonoBehaviour
{
    Animator animator;
    [SerializeField] Transform target;
    [SerializeField, Range(0, 1)] float lookAtWeight = 1;
    [SerializeField, Range(0, 1)] float rotationLimit = 0.6f;
    [SerializeField, Range(0, 10)] float distanceLimit = 2.6f;

    //Dummy pivot
    GameObject objPivot;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        //Dummy pivot
        objPivot = new GameObject("DummyPivot");
        objPivot.transform.parent = transform;
        objPivot.transform.localPosition = new Vector3(0, 1.7f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        objPivot.transform.LookAt(target);

        //Target distance limit
        float distance = Vector3.Distance(objPivot.transform.position, target.position);

        //Target rotation limit
        float pivotRotY = objPivot.transform.localRotation.y;
        if (Mathf.Abs(pivotRotY) > rotationLimit || distance > distanceLimit)
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
        animator.SetLookAtPosition(target.position);

    }
}
