using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using TNRD.Utilities;
#endif

public class CameraPointedObject : Interactable
{
    [Header("Camera Pointed Object")]
    public float targetAngle = 5f;
    public bool checkObstaclesRaycast = false;
    public LayerMask blockingLayers = 0;

    [Header("Event")]
    [SerializeField] protected UnityEvent OnObjectInteractedEvent;

#if UNITY_EDITOR

    /// <summary>
    /// Sets up timer and model children when this script is added to GameObject
    /// </summary>
    private void Reset()
    {
        base.Reset();
        IconManager.SetIcon(gameObject, LabelIcon.Purple);
    }
#endif

    // If this is not FixedUpdate, IMU cam rotation may not be registered, also it's better to raycast on FixedUpdate
    void FixedUpdate()
    {
        if (camera == null)
        {
            camera = Camera.main?.transform;
            return; //return here to avoid null reference exception
        }

        Vector3 camToThis = this.transform.position - camera.transform.position;
        float angle = Vector3.Angle(camera.transform.forward, camToThis);

        //Check if there is a collider blocking the raycast. The camera pointed object does not use collider.
        bool raycastResult = false;
        if (checkObstaclesRaycast)
        {
            RaycastHit hit = new RaycastHit();

            raycastResult = Physics.Raycast(GetComponent<Camera>().transform.position, camToThis, out hit, camToThis.magnitude, blockingLayers, QueryTriggerInteraction.Ignore);

            //If raycast hit itself, ignore
            if (hit.collider?.gameObject == this.gameObject)
                raycastResult = false;
        }

        if (angle <= targetAngle && !raycastResult)
        {
            //If Dwell mode, start timer
            if (interactionType == InteractionType.Dwell)
                StartTimer();

            //If button mode
            if (interactionType == InteractionType.Button && Input.GetMouseButtonDown(0))
                OnObjectInteracted();

            if (outlineMaterial == null)
                return;

            AddOutlineMaterial();
        }
        else
        {
            if (interactionType == InteractionType.Dwell)
                StopTimer();

            RemoveOutlineMaterial();
        }
    }

    protected void OnObjectInteracted()
    {
        if (OnObjectInteractedEvent != null)
            OnObjectInteractedEvent.Invoke();

        feedback?.Play();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        iTimer.OnFinishInteraction += OnObjectInteracted;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        iTimer.OnFinishInteraction -= OnObjectInteracted;
    }
}
