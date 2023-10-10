using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneboundGrabbable : Grabbable
{
    protected Plane constraintPlane;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        constraintPlane = new Plane(this.transform.forward, this.transform.position);
    }
    public override void Grab()
    {
        rb.useGravity = false;
        rb.isKinematic = false;
        base.Grab();
    }

    public override void Release()
    {
        rb.useGravity = usesGravity;
        base.Release();
    }

    public override void UpdateGrabbedPosition()
    {
        Ray ray = new Ray(cam.transform.position, CalculateCameraToTargetVector());
        float distance;
        if (constraintPlane.Raycast(ray, out distance))
        {
            Vector3 targetPosition = cam.transform.position + ray.direction * distance;
            this.rb.MovePosition(Vector3.Lerp(this.rb.position, targetPosition, Time.fixedDeltaTime * lerpScale));
        }
    }

    protected virtual void FixedUpdate()
    {
        if (state == GrabbableState.FREE)
        {
            CalculatePlaneboundPosition();
        }
    }

    protected void CalculatePlaneboundPosition()
    {
        //Calculate distance from the plane and project it to the normal to apply the correction
        Vector3 correction = Vector3.Project(this.transform.position - (constraintPlane.distance * -constraintPlane.normal), constraintPlane.normal);
        this.rb.MovePosition(rb.position - correction);
    }
}
