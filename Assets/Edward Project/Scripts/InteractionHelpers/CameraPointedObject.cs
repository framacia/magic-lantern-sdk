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

public class CameraPointedObject : MonoBehaviour
{
    public float targetAngle = 5f;
    private Transform camera;

    [HideInInspector] public InteractionTimer iTimer;
    [SerializeField] private string interactingText = "Interacting...";

    public bool checkObstaclesRaycast = false;
    public LayerMask blockingLayers = 0;

    private GameObject model;

    [SerializeField] Material outlineMaterial;
    private Material[] originalMaterials;
    private MeshRenderer renderer;

    [SerializeField] private UnityEvent OnObjectInteractedEvent;

    [SerializeField] InteractionType interactionType;

    private void Start()
    {
        if (camera == null)
            camera = Camera.main?.transform;

        if (iTimer == null)
        {
            iTimer = GetComponentInChildren<InteractionTimer>(true);
        }

        if (!string.IsNullOrEmpty(interactingText))
            iTimer.stateText.text = interactingText;

        renderer = GetComponent<MeshRenderer>();
        originalMaterials = renderer.materials;
    }

    private void OnEnable()
    {
        iTimer.OnFinishInteraction += OnObjectInteracted;
        NetworkPlayer.OnPlayerLoaded += OnPlayerLoaded;
    }

    private void OnDisable()
    {
        iTimer.OnFinishInteraction -= OnObjectInteracted;
        NetworkPlayer.OnPlayerLoaded -= OnPlayerLoaded;
        InteractionTypeController.Instance.OnInteractionTypeChanged -= SwitchInteractionType;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (iTimer == null)
        {
            iTimer = GetComponentInChildren<InteractionTimer>(true);
        }

        if (model == null)
        {
            transform.Find("Model");
        }
    }

    /// <summary>
    /// Sets up timer and model children when this script is added to GameObject
    /// </summary>
    private void Reset()
    {
        if (GetComponentInChildren<InteractionTimer>(true) == null)
        {
            GameObject iTimerPrefab = PrefabUtility.InstantiatePrefab(Resources.Load("Interaction Timer"), transform) as GameObject;

            iTimer = iTimerPrefab.GetComponent<InteractionTimer>();
        }

        if (transform.Find("Model") == null)
        {
            //model = new GameObject("Model", typeof(MeshFilter), typeof(MeshRenderer));
            model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            model.name = "Model";
            //model.GetComponent<MeshFilter>().mesh = PrimitiveType.Cube as Mesh;
            model.transform.SetParent(transform);
            model.transform.position = transform.position;
            model.SetActive(false);
        }

        if (string.IsNullOrEmpty(interactingText))
        {
            interactingText = "Interacting...";
        }
        IconManager.SetIcon(gameObject, LabelIcon.Purple);
    }
#endif

    // If this is not FixedUpdate, IMU cam rotation may not be registered, also it's better to raycast on FixedUpdate
    void FixedUpdate()
    {
        if (camera == null)
        {
            camera = Camera.main?.transform;
            return;
        }

        Vector3 camToThis = this.transform.position - camera.position;
        float angle = Vector3.Angle(camera.forward, camToThis);

        //Check if there is a collider blocking the raycast. The camera pointed object does not use collider.
        bool raycastResult = false;
        if (checkObstaclesRaycast)
        {
            RaycastHit hit = new RaycastHit();

            raycastResult = Physics.Raycast(camera.position, camToThis, out hit, camToThis.magnitude, blockingLayers, QueryTriggerInteraction.Ignore);

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

    void AddOutlineMaterial()
    {
        //If material number has already been edited, update fill and return
        if (renderer.materials.Length != originalMaterials.Length)
        {
            renderer.materials[originalMaterials.Length].SetFloat("_FillRate", iTimer.CurrentInteractionTime / iTimer.RequiredInteractionTime);
            return;
        }

        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[originalMaterials.Length] = outlineMaterial;
        renderer.materials = newMaterials;
    }

    void RemoveOutlineMaterial()
    {
        //If material number already original, return
        if (renderer.materials.Length == originalMaterials.Length)
            return;

        renderer.materials = originalMaterials;
    }

    void SwitchInteractionType(InteractionType newInteractionType)
    {
        interactionType = newInteractionType;
    }

    //I have to do this because gameobjects with NetworkIdentity are deactivated at the beginning so cant be subscribed
    private void OnPlayerLoaded()
    {
        InteractionTypeController.Instance.OnInteractionTypeChanged += SwitchInteractionType;
    }
}
