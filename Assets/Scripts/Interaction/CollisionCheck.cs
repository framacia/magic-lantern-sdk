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

    [SerializeField] private CheckType checkType;
    [SerializeField] private int numberOfChecksToTrigger;
    [SerializeField] private string colliderNameFilter;
    [SerializeField] private bool isParentName;
    [SerializeField] private bool allowSameObjectRecollision;
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
        if(!ownCollider.isTrigger && checkType == CheckType.OnEnter)
            CheckCollision(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ownCollider.isTrigger && checkType == CheckType.OnEnter)
            CheckCollision(other);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!ownCollider.isTrigger && checkType == CheckType.OnExit)
            CheckCollision(collision.collider);
    }

    private void OnTriggerExit(Collider other)
    {
        if (ownCollider.isTrigger && checkType == CheckType.OnExit)
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

            currentNumberOfChecks++;

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
}
