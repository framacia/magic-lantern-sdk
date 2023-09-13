using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public enum GrabbableState { FREE, GRABBED, PLACED, LOCKED }

public abstract class Grabbable : Interactable
{
    [field: Header("Physics")]
    [field: SerializeField] public Rigidbody rb { get; protected set; }
    protected bool usesGravity;

    // ---- States
    [HideInInspector] public GrabbableState state { get; protected set; } = GrabbableState.FREE;
    [HideInInspector] public GrabbableState previousState { get; protected set; } = GrabbableState.FREE;

    [field: Header("Grab")]
    public Placeable placeable { get; protected set; }
    protected Placeable pendingPlaceable;
    [field: SerializeField]
    public InteractionTimer iTimer { get; protected set; }

    protected Transform grabTarget; //Target that the grabbable will follow when grabbed.

    protected bool attemptingToGrab { get; private set; } = false;

    [field: SerializeField]
    protected float lerpScale { get; private set; } = 7.0f;

    //TODO Should be in parent class
    [field: Header("Event")]
    [SerializeField] protected UnityEvent OnObjectGrabbedEvent;

    // ---- Grab subscribable void actions
    public Action OnStartGrabbing;
    public Action OnStopGrabbing;
    public Action OnGrab;
    public Action OnPlace;
    public Action OnRelease;

    protected Camera cam;

    protected virtual void Start()
    {
        base.Start();
        usesGravity = rb.useGravity;
        cam = Camera.main;
    }

    public virtual bool StartGrabbingAttempt(Transform target)
    {
        if (attemptingToGrab)
            return false;

        if (state != GrabbableState.FREE && state != GrabbableState.PLACED)
            return false;

        attemptingToGrab = true;
        iTimer.StartInteraction();

        //Set outline material
        if (outlineMaterial != null)
            AddOutlineMaterial(renderer);

        this.grabTarget = target;
        if (OnStartGrabbing != null)
            OnStartGrabbing();
        return true;
    }

    public virtual void Grab()
    {
        if (state == GrabbableState.PLACED || state == GrabbableState.LOCKED)
        { //Grab can be forced that why we consider the LOCKED state
            RemoveFromPlace();
        }
        attemptingToGrab = false;
        UpdateState(state, GrabbableState.GRABBED);
        this.gameObject.layer = LayerMask.NameToLayer("Grabbed"); // Not efficient but solid when changing projects
        if (OnGrab != null)
            OnGrab();

        RemoveOutlineMaterial(renderer);
    }

    public virtual void StopGrabbingAttempt()
    {
        iTimer.CancelInteraction();
        grabTarget = null;
        ReleaseFromGrab();
        if (OnStopGrabbing != null)
            OnStopGrabbing();
    }

    public virtual void Release()
    {
        ReleaseFromGrab();
        UpdateState(state, GrabbableState.FREE);
        if (OnRelease != null)
            OnRelease();
    }

    protected virtual void ReleaseFromGrab()
    {
        grabTarget = null;
        attemptingToGrab = false;
        this.gameObject.layer = LayerMask.NameToLayer("Grabbable");

        RemoveOutlineMaterial(renderer);
    }

    public abstract void UpdateGrabbedPosition();

    public void Place()
    {
        placeable = pendingPlaceable;
        pendingPlaceable = null;
        grabTarget = null;

        UpdateState(state, placeable.LockObjectOnPlacement ? GrabbableState.LOCKED : GrabbableState.PLACED);
        this.gameObject.layer = LayerMask.NameToLayer("Grabbable");

        rb.isKinematic = true;
        rb.MovePosition(placeable.target.transform.position);
        rb.MoveRotation(placeable.target.transform.rotation);

        placeable.iTimer.OnFinishInteraction -= this.Place;
        if (OnPlace != null)
            OnPlace();

        RemoveOutlineMaterial(renderer);
    }
    public void RemoveFromPlace()
    {
        rb.isKinematic = false;
        Debug.Log("Removing from placeable");
        placeable.Remove();
        placeable = null;
        RemoveOutlineMaterial(renderer);
    }
    protected void UpdateState(GrabbableState prevState, GrabbableState targetState)
    {
        previousState = prevState;
        state = targetState;
    }

    protected Vector3 CalculateCameraToTargetVector()
    {
        return (grabTarget.transform.position - Camera.main.transform.position).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Do not check if already placed
        if (state == GrabbableState.PLACED || state == GrabbableState.LOCKED)
            return;

        Placeable p = other.GetComponent<Placeable>();
        if (p != null)
        {
            if (p.CheckIfCanPlaceObject(this.gameObject))
            {
                pendingPlaceable = p;
                p.iTimer.OnFinishInteraction += this.Place;
                p.StartPlacingObject(this.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Placeable p = other.GetComponent<Placeable>();
        if (p != null && p == pendingPlaceable)
        {
            p.iTimer.OnFinishInteraction -= this.Place;
            p.StopPlacingObject();
        }
    }

    //Used to update the fill rate of the placeable outline material
    private void OnTriggerStay(Collider other)
    {
        Placeable p = other.GetComponent<Placeable>();
        if (p != null)
        {
            p.UpdatePlacing();
        }
    }

    //TODO Probably rethink this
    public void GrabbingUpdate()
    {
        if (state == GrabbableState.GRABBED || state == GrabbableState.LOCKED)
            return;

        //Set outline material
        if (outlineMaterial != null)
            UpdateOutlineFill(renderer);
    }
}
