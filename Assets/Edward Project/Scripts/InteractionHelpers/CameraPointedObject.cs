using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using TNRD.Utilities;
#endif

public class CameraPointedObject : MonoBehaviour
{
    public float targetAngle = 5f;
    private Transform camera;

    public InteractionTimer iTimer;
    [SerializeField] private string interactingText = "Interacting...";

    public bool checkObstaclesRaycast = false;
    public LayerMask blockingLayers = 0;

    private GameObject model;
    [SerializeField] private UnityEvent OnObjectInteractedEvent;

    private void Awake()
    {
        if (camera == null)
            camera = Camera.main.transform;

        if (iTimer == null)
        {
            iTimer = GetComponentInChildren<InteractionTimer>(true);
        }

        if (!string.IsNullOrEmpty(interactingText))
            iTimer.stateText.text = interactingText;

        iTimer.OnFinishInteraction += OnObjectInteracted;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Sets up timer and model children when this script is added to GameObject
    /// </summary>
    private void Reset()
    {
        if (iTimer == null)
        {
            GameObject iTimerPrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Interaction Timer"), transform) as GameObject;

            iTimer = iTimerPrefab.GetComponent<InteractionTimer>();
        }

        if (model == null)
        {
            //model = new GameObject("Model", typeof(MeshFilter), typeof(MeshRenderer));
            model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "Model";
            //model.GetComponent<MeshFilter>().mesh = PrimitiveType.Cube as Mesh;
            model.transform.SetParent(transform);
            model.transform.position = transform.position;
        }

        if(string.IsNullOrEmpty(interactingText))
        {
            interactingText = "Interacting...";
        }
        IconManager.SetIcon(gameObject, LabelIcon.Purple);
    }
#endif

    // Update is called once per frame
    void Update()
    {
        Vector3 camToThis = this.transform.position - camera.position;
        float angle = Vector3.Angle(camera.forward, camToThis);


        //Check if there is a collider blocking the raycast. The camera pointed object does not use collider.
        bool raycastResult = false;
        if (checkObstaclesRaycast)
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

    void OnObjectInteracted()
    {
        if (OnObjectInteractedEvent != null)
            OnObjectInteractedEvent.Invoke();
    }
}
