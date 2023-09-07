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
    public ParticleSystem[] particles;

    // Start is called before the first frame update
    private void Start()
    {
        if(audioSource == null && GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();
        audioSource.clip = soundFXClip;
    }

    public void Play()
    {
        if(soundFXClip)
            PlaySFX();

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
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Play();
        }
    }
}
