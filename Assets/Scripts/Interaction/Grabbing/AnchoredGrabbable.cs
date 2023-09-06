using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnchoredGrabbable : Grabbable
{
    protected float distanceFromTarget;
    protected Quaternion targetRotation;

    [field: SerializeField]
    protected bool matchRotation {get; private set;} = false;

    //TODO Should be in parent class
    [field: Header("Event")]
    [SerializeField] private UnityEvent OnObjectGrabbedEvent;

    public override void Grab(){
        distanceFromTarget = CalculateDistanceFromTarget();
        rb.useGravity = false;
        OnObjectGrabbedEvent?.Invoke();
        base.Grab();
    }

    public override void Release(){
        rb.useGravity = usesGravity;
        base.Release();
    }
    
    public override void UpdateGrabbedPosition(){
        
        Vector3 targetPosition = grabTarget.position + CalculateCameraToTargetVector() * distanceFromTarget;
        targetRotation = grabTarget.rotation;

        this.rb.MovePosition(Vector3.Lerp(this.rb.position, targetPosition, Time.fixedDeltaTime * lerpScale));
        if(matchRotation){
            this.rb.MoveRotation(Quaternion.Lerp(this.rb.rotation, targetRotation, Time.fixedDeltaTime * lerpScale));
        }
    }

    private float CalculateDistanceFromTarget(){
        Vector3 targetToThis = this.transform.position - grabTarget.position;
        Vector3 camToTarget = CalculateCameraToTargetVector();

        //Project the calculated vector on the cam.forward -> grabber.target to get the desired length
        float d = Vector3.Dot(targetToThis, camToTarget); //no need to divide for camToTarget.magnitude since camToTarget is always normalized
        
        return d;
    }
}
