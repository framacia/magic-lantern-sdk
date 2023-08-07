using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyInteraction : MonoBehaviour
{
    Rigidbody rb;
    Camera cam;
    [SerializeField] AudioClip soundFXClip;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        audioSource = this.GetComponent<AudioSource>();
        audioSource.clip = soundFXClip;
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
        PlaySFX();
    }

    public void PlaySFX()
    {
        audioSource.pitch = (Random.Range(0.8f, 1.2f));
        audioSource.Play();
    }
}
