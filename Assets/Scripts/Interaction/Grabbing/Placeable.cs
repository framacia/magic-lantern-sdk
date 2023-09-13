using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Placeable : Interactable
{
    [field: Header("Object Properties")]
    [field: SerializeField]
    public Transform target { get; private set; }
    public GameObject displayMesh;
    public Collider col;

    [field: Header("Placement settings")]
    [field: SerializeField]
    public bool LockObjectOnPlacement { get; protected set; } = false;
    public bool HasObject { get { return placedObject != null; } }

    public GameObject pendingPlacedObject { get; protected set; }
    public GameObject placedObject { get; protected set; }
    public GameObject lastPlacedObject { get; protected set; }
    public bool canPlaceLastPlacedObject { get; protected set; } = true; //used to prevent perpetual place and grab
    [Tooltip("Minimum separation distance between the object that was picked and the placeable target that needs to happen before being able to place it again (this avoid instants placement upon pickup)")]
    public float minimumSquaredDistanceToPlaceAgain = 1f;
    [field: Header("Name filtering")]
    [field: SerializeField]
    public bool NameFilter { get; private set; } = false;
    [field: SerializeField]
    protected string[] Names { get; private set; }

    [Header("Events")]
    [SerializeField] private UnityEvent OnObjectPlacedEvent;
    [SerializeField] private UnityEvent OnObjectRemovedEvent;

    private void Awake()
    {
        iTimer.OnFinishInteraction += this.Place;
    }

    //Placement variables
    private void FixedUpdate()
    {
        if (placedObject != null)
        { //attach the object manually each frame.
            placedObject.transform.position = this.target.position;
            placedObject.transform.rotation = this.target.rotation;
        }
        if (!canPlaceLastPlacedObject)
        {
            if (lastPlacedObject == null)
                canPlaceLastPlacedObject = true;
            else
                canPlaceLastPlacedObject = (target.transform.position - lastPlacedObject.transform.position).sqrMagnitude > minimumSquaredDistanceToPlaceAgain;
        }
    }

    bool CheckNamesFilter(GameObject other)
    {
        if (!NameFilter)
            return true;

        for (int i = 0; i < Names.Length; i++)
        {
            if (other.gameObject.name == Names[i])
                return true;
        }

        return false;
    }

    public void StartPlacingObject(GameObject other)
    {
        pendingPlacedObject = other;
        iTimer.StartInteraction();
        AddOutlineMaterial(displayMesh.GetComponent<Renderer>());
    }

    public void StopPlacingObject()
    {
        pendingPlacedObject = null;
        iTimer.CancelInteraction();
        RemoveOutlineMaterial(displayMesh.GetComponent<Renderer>());
    }

    public void Place()
    {
        placedObject = pendingPlacedObject;
        pendingPlacedObject = null;
        RemoveOutlineMaterial(displayMesh.GetComponent<Renderer>());
        displayMesh.SetActive(false);
        feedback?.Play();
        OnObjectPlacedEvent?.Invoke();
    }

    public void Remove()
    {
        lastPlacedObject = placedObject;
        placedObject = null;
        displayMesh.SetActive(true);
        canPlaceLastPlacedObject = false;
        OnObjectRemovedEvent?.Invoke();
        RemoveOutlineMaterial(displayMesh.GetComponent<Renderer>());
    }

    public bool CheckIfCanPlaceObject(GameObject other)
    {
        bool canPlace = pendingPlacedObject == null && placedObject == null;
        bool isLastObject = other == lastPlacedObject;
        return (canPlace && (!isLastObject || (isLastObject && canPlaceLastPlacedObject)) && CheckNamesFilter(other));
    }

    //Wrapper to update the outline material from the Grabbable's OnTriggerStay
    public void UpdatePlacing()
    {
        UpdateOutlineFill(displayMesh.GetComponent<Renderer>());
    }
}
