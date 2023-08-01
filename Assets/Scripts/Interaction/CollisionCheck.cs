using CrazyMinnow.SALSA.OneClicks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCheck : MonoBehaviour
{

    [SerializeField] private UnityEvent OnCollisionCheckMetEvent;
    [SerializeField] private int numberOfChecks;
    private int currentNumberOfChecks;
    [SerializeField] private string colliderName;
    [SerializeField] private bool isParentName;
    private bool conditionMet;

    void OnCollisionCheckMet()
    {
        if (OnCollisionCheckMetEvent != null)
            OnCollisionCheckMetEvent.Invoke();
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
        //Check already met?
        if (currentNumberOfChecks + 1 >= numberOfChecks && !conditionMet)
        {
            OnCollisionCheckMet();
            conditionMet = true;
            return;
        }

        if (isParentName)
        {
            if (other.transform.parent.name == colliderName)
                currentNumberOfChecks++;
        }
        else
        {
            if (other.transform.name == colliderName)
                currentNumberOfChecks++;
        }
    }
}
