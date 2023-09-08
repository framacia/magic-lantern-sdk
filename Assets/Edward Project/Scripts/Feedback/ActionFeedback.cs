using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ActionFeedback : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] AudioClip soundFXClip;
    [SerializeField] Vector2 randomPitchRange = new Vector2(1,1);
    [SerializeField] AudioSource audioSource;

    [Header("Particles")]
    [SerializeField] ParticleSystem particleSystem;

    // Start is called before the first frame update
    private void Start()
    {
        if(audioSource == null && GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();
        if (particleSystem == null && GetComponentInChildren<ParticleSystem>())
            particleSystem = GetComponentInChildren<ParticleSystem>();
        audioSource.clip = soundFXClip;
    }

    public void Play()
    {
        if(soundFXClip)
            PlaySFX();

        if(particleSystem)
            PlayParticles();
    }

    //Have to use AudioSource because PlayOneShot does not support pitch changes
    private void PlaySFX()
    {
        audioSource.pitch = (Random.Range(randomPitchRange.x, randomPitchRange.y));
        audioSource.Play();
    }

    private void PlayParticles()
    {
        particleSystem.Play();
    }
}
