using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondCustomPlaneboundPlaceable : PlaneboundGrabbable
{
    protected override void Start()
    {
        base.Start();
        constraintPlane = new Plane(Vector3.right, new Vector3(3.5f, 0f, 0f));
    }
    protected override void FixedUpdate(){
        if(state != GrabbableState.PLACED &&state != GrabbableState.LOCKED){
            RecalculatePlane();
            base.FixedUpdate();
        }
    }

    protected void RecalculatePlane(){
        //------ RIGHT PLANE
        if(this.rb.position.x > 3.5 && Mathf.Abs(Vector3.Dot(constraintPlane.normal, Vector3.back)) > 0.9f){
            constraintPlane = new Plane(Vector3.right, new Vector3(3.5f, 0f, 0f));
        }
        //------ MIDDLE PLANE
        else if(this.rb.position.z < -3.51f && Mathf.Abs(Vector3.Dot(constraintPlane.normal, Vector3.back)) < 0.9f){
            constraintPlane = new Plane(Vector3.back, new Vector3(0.0f, 0f, -3.5f));
        }
    }

    public override void UpdateGrabbedPosition()
    {
        base.UpdateGrabbedPosition();
        rb.MovePosition(new Vector3(Mathf.Max(rb.position.x, 0.5f), Mathf.Clamp(rb.position.y, 0.3f, 2.7f), Mathf.Min(-0.5f, rb.position.z)));
    }
}
