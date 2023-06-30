using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPointedObject : MonoBehaviour
{
    public float targetAngle = 5f;
    public new Transform camera;
    public InteractionTimer iTimer;

    public bool testRaycast = false;
    public LayerMask blockingLayers = 0;

    // Update is called once per frame
    void Update()
    {
        Vector3 camToThis = this.transform.position - camera.position;
        float angle = Vector3.Angle(camera.forward, camToThis);


        //Check if there is a collider blocking the raycast. The camera pointed object does not use collider.
        bool raycastResult = false;
        if (testRaycast)
        {
            raycastResult = Physics.Raycast(camera.position, camToThis, camToThis.magnitude, blockingLayers, QueryTriggerInteraction.Ignore);
        }

        if (angle <= targetAngle && !raycastResult)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    void StartTimer()
    {
        if (!iTimer.IsInteracting)
            iTimer.StartInteraction();
    }

    void StopTimer()
    {
        if (iTimer.IsInteracting)
            iTimer.CancelInteraction();
    }
}
