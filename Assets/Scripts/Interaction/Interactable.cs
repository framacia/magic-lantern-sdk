using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using TNRD.Utilities;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    protected Transform camera;
    [HideInInspector] public InteractionTimer iTimer;

    [Header("Interaction")]
    [SerializeField] protected InteractionType interactionType;
    [SerializeField] protected float requiredInteractionTime = 1.5f;

    protected GameObject model;

    [Header("Visual")]
    [SerializeField] protected string interactingText = "Interacting...";
    [SerializeField] protected Material outlineMaterial;
    //[SerializeField] [Tooltip("The material slot in which the outline will be based")] protected int outlineMaterialIndex;
    [SerializeField] protected float outlineThickness = 1.5f;
    protected Material[] originalMaterials;
    protected MeshRenderer renderer;

    [Header("Feedback")]
    public ActionFeedback feedback;

    protected virtual void Start()
    {
        if (camera == null)
            camera = Camera.main?.transform;

        if (iTimer == null)
        {
            iTimer = GetComponentInChildren<InteractionTimer>(true);
        }

        if (feedback == null && GetComponent<ActionFeedback>())
        {
            feedback = GetComponent<ActionFeedback>();
        }

        if (!string.IsNullOrEmpty(interactingText))
            iTimer.stateText.text = interactingText;
        iTimer.RequiredInteractionTime = requiredInteractionTime;

        if (GetComponent<MeshRenderer>())
            renderer = GetComponent<MeshRenderer>();
        else
            renderer = GetComponentInChildren<MeshRenderer>();

        if (renderer)
            originalMaterials = renderer.materials;
    }

    protected virtual void OnEnable()
    {
        NetworkPlayer.OnPlayerLoaded += OnPlayerLoaded;
    }

    protected virtual void OnDisable()
    {
        NetworkPlayer.OnPlayerLoaded -= OnPlayerLoaded;
        if (InteractionTypeController.Instance)
            InteractionTypeController.Instance.OnInteractionTypeChanged -= SwitchInteractionType;
    }

    //I have to do this because gameobjects with NetworkIdentity are deactivated at the beginning so cant be subscribed
    protected void OnPlayerLoaded()
    {
        if (InteractionTypeController.Instance)
            InteractionTypeController.Instance.OnInteractionTypeChanged += SwitchInteractionType;
    }

    protected void SwitchInteractionType(InteractionType newInteractionType)
    {
        interactionType = newInteractionType;
    }

    protected void AddOutlineMaterial(Renderer renderer)
    {
        if (!renderer) return;

        //If material number has already been edited, return
        if (renderer.materials.Length != originalMaterials.Length)
        {
            return;
        }

        Material[] newMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            newMaterials[i] = originalMaterials[i];
        }
        newMaterials[originalMaterials.Length] = outlineMaterial;
        renderer.materials = newMaterials;
        //Change outline thickness
        renderer.materials[originalMaterials.Length].SetFloat("_Thickness", outlineThickness);
    }

    //First overload, uses private model variable
    public void RemoveOutlineMaterial()
    {
        //If material number already original, return
        if (renderer.materials.Length == originalMaterials.Length)
            return;

        renderer.materials = originalMaterials;
    }

    //Second overload, allows for putting another model as parameter
    public void RemoveOutlineMaterial(Renderer renderer)
    {
        if (renderer == null)
            return;

        //If material number already original, return
        if (renderer.materials.Length == originalMaterials.Length)
            return;

        renderer.materials = originalMaterials;
    }

    protected void UpdateOutlineFill(Renderer renderer)
    {
        if(!renderer) return;

        //If material number has already been edited, update fill and return
        if (renderer.materials.Length != originalMaterials.Length)
        {
            renderer.materials[originalMaterials.Length].SetFloat("_FillRate", iTimer.CurrentInteractionTime / iTimer.RequiredInteractionTime);
        }
    }

    protected void StartTimer()
    {
        if (!iTimer.IsInteracting)
            iTimer.StartInteraction();
    }

    protected void StopTimer()
    {
        if (iTimer.IsInteracting)
            iTimer.CancelInteraction();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Sets up timer and model children when this script is added to GameObject
    /// </summary>
    protected virtual void Reset()
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
    }

    protected void OnValidate()
    {
        if (iTimer == null)
        {
            iTimer = GetComponentInChildren<InteractionTimer>(true);
        }

        if (model == null)
        {
            transform.Find("Model");
        }

        if (outlineMaterial == null)
        {
            outlineMaterial = Resources.Load("M_Outline") as Material;
        }
    }
#endif
}
