using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Placeable : MonoBehaviour
{
    [field: Header("Object Properties")]
    [field: SerializeField]
    public Transform target { get; private set; }
    public GameObject displayMesh;
    public Collider col;

    [field: Header("Placement settings")]
    [field: SerializeField]
    public bool lockObjectOnPlacement { get; protected set; } = false;
    public bool HasObject { get { return placedObject != null; } }

    public InteractionTimer iTimer;

    public GameObject pendingPlacedObject { get; protected set; }
    public GameObject placedObject { get; protected set; }
    public GameObject lastPlacedObject { get; protected set; }
    public bool canPlaceLastPlacedObject { get; protected set; } = true; //used to prevent perpetual place and grab
    [Tooltip("Minimum separation distance between the object that was picked and the placeable target that needs to happen before being able to place it again (this avoid instants placement upon pickup)")]
    public float minimumSquaredDistanceToPlaceAgain = 1f;
    [field: Header("Name filtering")]
    [field: SerializeField]
    public bool nameFilter { get; private set; } = false;
    [field: SerializeField]
    protected string[] names { get; private set; }

    [Header("Feedback")]
    public ActionFeedback feedback;

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
        if (!nameFilter)
            return true;

        for (int i = 0; i < names.Length; i++)
        {
            if (other.gameObject.name == names[i])
                return true;
        }

        return false;
    }

    public void StartPlacingObject(GameObject other)
    {
        pendingPlacedObject = other;
        iTimer.StartInteraction();
    }

    public void StopPlacingObject()
    {
        pendingPlacedObject = null;
        iTimer.CancelInteraction();
    }

    public void Place()
    {
        placedObject = pendingPlacedObject;
        pendingPlacedObject = null;
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
    }

    public bool CheckIfCanPlaceObject(GameObject other)
    {
        bool canPlace = pendingPlacedObject == null && placedObject == null;
        bool isLastObject = other == lastPlacedObject;
        return (canPlace && (!isLastObject || (isLastObject && canPlaceLastPlacedObject)) && CheckNamesFilter(other));
    }
}
