using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Env : MonoBehaviour
{
    [Header("Camera and Audio")]
    public Camera cam;
    public AudioClip[] clips;
    public new AudioSource audio;
    protected int stage = 0;
    [Header("Other Environments")]
    public GameObject nextEnv;

    public Action onFinishEnvironment;

    // Start is called before the first frame update
    protected abstract void Start();

    // Update is called once per frame
    protected abstract void Update();

    protected virtual void FinishEnvironment()
    {
        if (onFinishEnvironment != null)
            onFinishEnvironment();
        nextEnv.SetActive(true);
        this.gameObject.SetActive(false);
    }

    protected IEnumerator WaitAudio(float delay)
    {
        yield return null; //Wait a frame
        yield return new WaitUntil(() => !audio.isPlaying);
        yield return new WaitForSeconds(delay);
    }

    protected void SetClipAndPlay(AudioClip clip)
    {
        audio.clip = clip;
        audio.Play();
    }
}
