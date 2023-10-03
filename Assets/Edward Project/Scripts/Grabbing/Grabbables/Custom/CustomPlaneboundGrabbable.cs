using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPlaneboundGrabbable : PlaneboundGrabbable
{

    protected override void FixedUpdate(){
        if(state != GrabbableState.PLACED && state != GrabbableState.LOCKED){
            RecalculatePlane();
            base.FixedUpdate();
        }
    }

    protected void RecalculatePlane(){
        //------ LEFT PLANE
        if(this.rb.position.x < -3.5 && Mathf.Abs(Vector3.Dot(constraintPlane.normal, Vector3.forward)) > 0.9f){
            constraintPlane = new Plane(Vector3.right, new Vector3(-3.5f, 0f, 0f));
        }
        //------ MIDDLE PLANE
        else if(this.rb.position.z > 3.51f && Mathf.Abs(Vector3.Dot(constraintPlane.normal, Vector3.forward)) < 0.9f){
            constraintPlane = new Plane(Vector3.back, new Vector3(0.0f, 0f, 3.5f));
        }
        //------ RIGHT PLANE
        else if(this.rb.position.x > 3.5 && Mathf.Abs(Vector3.Dot(constraintPlane.normal, Vector3.forward)) > 0.9f){
            constraintPlane = new Plane(Vector3.left, new Vector3(3.5f, 0f, 0f));
        }
    }

    public override void UpdateGrabbedPosition()
    {
        base.UpdateGrabbedPosition();
        rb.MovePosition(new Vector3(rb.position.x, Mathf.Clamp(rb.position.y, 0.3f, 2.7f), Mathf.Max(0.0f, rb.position.z)));
    }
}
