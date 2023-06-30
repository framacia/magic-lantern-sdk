using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionFeedback : MonoBehaviour
{
    public AudioSource[] audio;
    public ParticleSystem[] particles;
    // Start is called before the first frame update
    public void Play(){
        for(int i = 0; i < audio.Length; i++){
            audio[i].Play();
        }
        for(int i = 0; i < particles.Length; i++){
            particles[i].Play();
        }
    }
}
