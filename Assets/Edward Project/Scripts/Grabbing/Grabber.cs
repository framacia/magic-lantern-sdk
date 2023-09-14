using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public Rigidbody rb;
    public Transform target; //grabbables will try to attach themselves to this point and not the object root
    private Grabbable grabbedObject;
    private Grabbable pendingGrabbedObject;
    private Grabbable lastGrabbedObject;
    private bool canGrabLastGrabbedObject;
    
    [Tooltip("Minimum separation distance between the placed object and the hand to be able to grab it again right after placing it (to prevent instant grabing right after placement)")]
    public float minimumSquaredDistanceToRegrabPlacedObject = 0.1f;
    private List<Grabbable> grabbablesInsideTrigger = new List<Grabbable>();
    public Camera cam {get; private set;}

    [Header("Audio and feedback")]
    public ActionFeedback feedback;

    public void Start(){
        cam = Camera.main;
    }

    private void GrabObject()
    {
        ForceGrabObject(pendingGrabbedObject);
        grabbedObject.iTimer.OnFinishInteraction -= GrabObject; //Unsubscribe self
        pendingGrabbedObject = null;
    }

    public void ForceGrabObject(Grabbable other){
        grabbedObject = other;
        grabbedObject.OnPlace += ForceReleaseObject;
        feedback?.PlayRandomTriggerFeedback();
        other.Grab();
    }

    // Update is called once per frame
    private void ReleaseObject()
    {
        if(grabbedObject != null) //Check for forced releases, it might not be grabbing
            grabbedObject.Release();
        ForceReleaseObject();
    }

    public void ForceReleaseObject(){
        //Clear the placement subscription to avoid bugs
        if(grabbedObject != null)
            grabbedObject.OnPlace -= ForceReleaseObject;

        pendingGrabbedObject = null;
        lastGrabbedObject = grabbedObject;
        grabbedObject = null;

        canGrabLastGrabbedObject = false;
    }
    //I have to do this extremely stupid collision checks because Unity
    //doesn't register trigger exits on modifyed mesh colliders
    private void FixedUpdate(){

        if(grabbedObject != null){
            grabbedObject.UpdateGrabbedPosition();
        }

        if(!canGrabLastGrabbedObject)
            CheckIfCanGrabLastGrabbedObject();

        if(pendingGrabbedObject == null)
            return;

        //Search for the pending grabbed
        Grabbable g = grabbablesInsideTrigger.Find(x => x == pendingGrabbedObject);
        //PendingGrabbable is not inside the trigger;
        if(g == null){
            ReleasePendingGrabbedObject();
        }
        grabbablesInsideTrigger.Clear();
    }
    private void OnTriggerEnter(Collider other) {
        Grabbable g = other.GetComponent<Grabbable>();
        if(g == null)
            return;
        if(grabbedObject == null && pendingGrabbedObject == null){
            if(g == lastGrabbedObject && !canGrabLastGrabbedObject)
                return;
            if(g.StartGrabbingAttempt(target)){
                g.iTimer.OnFinishInteraction += GrabObject; //Subscribe to the timer end event
                pendingGrabbedObject = g;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        Grabbable g = other.GetComponent<Grabbable>();
        if(g != null)
            grabbablesInsideTrigger.Add(g);
    }

    private void ReleasePendingGrabbedObject(){
        pendingGrabbedObject.iTimer.OnFinishInteraction -= GrabObject;
        pendingGrabbedObject.StopGrabbingAttempt();
        pendingGrabbedObject = null;
    }

    public void CheckIfCanGrabLastGrabbedObject(){
        if(!canGrabLastGrabbedObject){
            if(lastGrabbedObject == null)
                canGrabLastGrabbedObject = true;
            else{
                Vector3 projection = Vector3.ProjectOnPlane(this.transform.position - lastGrabbedObject.transform.position, cam.transform.forward);
                canGrabLastGrabbedObject = projection.sqrMagnitude > minimumSquaredDistanceToRegrabPlacedObject;
            }
        }
    }

    public void ClearHand(){
        if(grabbedObject != null)
            ReleaseObject();
        else if(pendingGrabbedObject != null)
            ReleasePendingGrabbedObject();
    }

    public void OnDisable(){
        ClearHand();
    }

    private void OnDestroy() {
        ClearHand();
    }
}