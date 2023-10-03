using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitAudioFeedback : MonoBehaviour
{
    public AudioSource source;
    [Tooltip("Minimum impact value required to play the hit audio at minimum volume. Will be squared on play.")]
    public float minImpulse = 3f;
    [Tooltip("Impact value required to play the hit audio at maximum volume. Will be squared on play.")]
    public float maxImpulse = 20f;

    void Start(){
        minImpulse *= minImpulse;
        maxImpulse *= maxImpulse;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.impulse.sqrMagnitude;
        if(impact > minImpulse){
            source.volume = 0.1f + Mathf.Min(0.9f, ((impact / maxImpulse) / 0.9f));
            source.Play();
        }
    }
}
