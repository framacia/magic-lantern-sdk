using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyInteraction : MonoBehaviour
{
    Rigidbody rb;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    private void OnValidate()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public void AddForce(float force)
    {
        if(!cam)
            cam = Camera.main;

        //Relative to camera
        Vector3 forceVector = cam.transform.forward * force;

        rb.AddForce(forceVector);
    }
}
