using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyInteraction : MonoBehaviour
{
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnValidate()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public void AddForce(float force)
    {
        Vector3 forceVector = new Vector3(0, 0, force);
        rb.AddForce(forceVector);
    }
}
