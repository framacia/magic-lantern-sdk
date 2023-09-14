using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using DG.Tweening;

public class ActionFeedback : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] List<AudioClip> triggerSFXClips = new List<AudioClip>();
    [SerializeField] AudioClip progressSFXClip;
    [SerializeField] AudioSource audioSource;

    [Header("Volumes")]
    [SerializeField][Range(0, 1)] float triggerVolume = 1.0f;
    [SerializeField][Range(0, 1)] float progressVolume = 0.5f;
    [SerializeField] float progressFadeAmount = 0.5f;
    [SerializeField] Vector2 randomPitchRange = new Vector2(1, 1);

    [Header("Particles")]
    [SerializeField] ParticleSystem particleSystem;

    private int currentTriggerSFXIndex;

    // Start is called before the first frame update
    private void Start()
    {
        if (audioSource == null && GetComponent<AudioSource>())
            audioSource = GetComponent<AudioSource>();
        if (particleSystem == null && GetComponentInChildren<ParticleSystem>())
            particleSystem = GetComponentInChildren<ParticleSystem>();

        currentTriggerSFXIndex = Random.Range(0, triggerSFXClips.Count);
        audioSource.clip = triggerSFXClips[currentTriggerSFXIndex];
    }

    private void Update()
    {

    }

    public void PlayRandomTriggerFeedback()
    {
        if (triggerSFXClips.Count > 0)
        {
            currentTriggerSFXIndex = Random.Range(0, triggerSFXClips.Count);
            audioSource.clip = triggerSFXClips[currentTriggerSFXIndex];
            audioSource.loop = false;
            audioSource.volume = triggerVolume;
            PlaySFX();
        }

        if (particleSystem)
            PlayParticles();
    }

    public void PlayProgressFeedback()
    {
        //No matter what if there is a sfx currently playing, wait
        if (audioSource.isPlaying)
            return;

        //If there is a progress sfx and it is not currently playing, play it
        if (progressSFXClip)
        {
            audioSource.clip = progressSFXClip;
            audioSource.loop = true;
            
            //Fade volume with DOTween
            audioSource.volume = 0f;
            audioSource.DOFade(progressVolume, progressFadeAmount);

            PlaySFX();
        }
    }

    public void StopProgressFeedback()
    {
        if (!audioSource.isPlaying)
            return;

        if (progressSFXClip)
        {
            audioSource.loop = false;
            audioSource.DOFade(0f, 0.1f);
            //audioSource.Stop(); //No need to stop because now it does not loop, let it fade and then stops automatically
        }
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
