using CrazyMinnow.SALSA.OneClicks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCheck : MonoBehaviour
{
    enum CheckType
    {
        OnEnter,
        OnExit
    }

    [Header("Collision")]
    [SerializeField] private CheckType checkType;
    [SerializeField] private int numberOfChecksToTrigger;
    [SerializeField] private bool allowSameObjectRecollision;
    [SerializeField] private bool deactivateCollidedInteractables;
    [SerializeField] private bool triggerActionFeedback;
    [Tooltip("The collider GameObject will need to exceed this velocity for a succesful check")]
    [SerializeField] private Vector3 requiredVelocityVector;
    [Tooltip("Do we need to check the velocity of the parent Rigidbody? (Useful for Grabbed objects)")]
    [SerializeField] private bool isParentVelocity;

    [Header("Name Filter")]
    [SerializeField] private string colliderNameFilter;
    [SerializeField] private bool isParentName;

    [Header("Event")]
    [SerializeField] private UnityEvent OnCollisionCheckMetEvent;

    private Collider ownCollider;
    private int currentNumberOfChecks;
    private bool conditionMet;
    private List<Collider> alreadyCheckedColliders = new List<Collider>();

    private void Start()
    {
        ownCollider = GetComponent<Collider>();
    }

    void OnCollisionCheckMet()
    {
        OnCollisionCheckMetEvent?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (checkType == CheckType.OnEnter)
            CheckCollision(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (checkType == CheckType.OnEnter)
            CheckCollision(other);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (checkType == CheckType.OnExit)
            CheckCollision(collision.collider);
    }

    private void OnTriggerExit(Collider other)
    {
        if (checkType == CheckType.OnExit)
            CheckCollision(other);
    }

    private void CheckCollision(Collider other)
    {
        //Name filter check
        if ((isParentName && other.transform.parent.name == colliderNameFilter) || (!isParentName && other.transform.name == colliderNameFilter))
        {
            //If collider already checked, return
            if (!allowSameObjectRecollision && alreadyCheckedColliders.Contains(other))
                return;

            //If required velocity not met, return
            if(requiredVelocityVector == Vector3.zero)
            {
                //Nothing to check
            }
            else
            {
                GameObject velocityGO;

                if (isParentVelocity)
                    velocityGO = other.transform.parent.gameObject;
                else
                    velocityGO = other.transform.gameObject;

                if (!VelocityCheck(velocityGO.GetComponent<Rigidbody>().velocity))
                    return;
            }

            //Succesful check!
            currentNumberOfChecks++;

            //Deactivate Collided Interactable
            if (other.GetComponent<Interactable>() && deactivateCollidedInteractables)
            {
                other.GetComponent<Interactable>().RemoveOutlineMaterial();
                other.GetComponent<Interactable>().enabled = false;
            }

            //Trigger Action Feedback
            if(other.GetComponent<ActionFeedback>() && triggerActionFeedback)
            {
                other.GetComponent<ActionFeedback>().PlayRandomTriggerFeedback();
            }

            //Check number goal already met?
            if (currentNumberOfChecks >= numberOfChecksToTrigger && !conditionMet)
            {
                OnCollisionCheckMet();
                conditionMet = true;
                alreadyCheckedColliders.Clear();
                return;
            }

            if (!allowSameObjectRecollision)
                alreadyCheckedColliders.Add(other);
        }
    }

    bool VelocityCheck(Vector3 colliderAcceleration)
    {
        return Vector3.Dot(colliderAcceleration, requiredVelocityVector) > 0;
    }

}
