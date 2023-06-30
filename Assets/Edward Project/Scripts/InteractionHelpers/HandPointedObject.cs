using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPointedObject : MonoBehaviour
{
    public float radius = 0.25f;
    [field: SerializeField]
    public InteractionTimer iTimer {get; private set;}
    // Start is called before the first frame update
    public LayerMask handLayer;
    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(this.transform.position, radius);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Collider[] overlappedHands = Physics.OverlapSphere(this.transform.position, radius, handLayer, QueryTriggerInteraction.Ignore);
        if(overlappedHands.Length > 0){
            if(!iTimer.IsInteracting)
                iTimer.StartInteraction();
        }
        else{
            if(iTimer.IsInteracting)
                iTimer.CancelInteraction();
        }
    }
}
