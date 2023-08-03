using CrazyMinnow.SALSA.OneClicks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCheck : MonoBehaviour
{
    [SerializeField] private UnityEvent OnCollisionCheckMetEvent;
    [SerializeField] private int numberOfChecksToTrigger;
    [SerializeField] private string colliderNameFilter;
    [SerializeField] private bool isParentName;
    [SerializeField] private bool allowSameObjectRecollision;

    private int currentNumberOfChecks;
    private bool conditionMet;
    private List<Collider> alreadyCheckedColliders = new List<Collider>();

    void OnCollisionCheckMet()
    {
        OnCollisionCheckMetEvent?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckCollision(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckCollision(collision.collider);
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
