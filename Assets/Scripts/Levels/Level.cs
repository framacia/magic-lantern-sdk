using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Playables;

public class Level : MonoBehaviour
{
    private LevelController levelController;
    private PlayableDirector director;

    public bool autoFinish = false;
    public int levelIndex;

    private void Awake()
    {
        //TODO check if it's better to make it a singleton
        levelController = FindObjectOfType<LevelController>();

        director = GetComponent<PlayableDirector>();

        if (!director)
            return;

        director.playOnAwake = false;
    }

    void OnEnable()
    {
        if (director)
            director.stopped += OnPlayableDirectorStopped;
    }

    void OnDisable()
    {
        if (director)
            director.stopped -= OnPlayableDirectorStopped;
    }

    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        //If it's not your director, return
        if (director != aDirector)
            return;

        if (autoFinish)
            levelController.PlayNextLevel();

        Debug.LogFormat("Level {0} timeline stopped", levelIndex);
    }

    public void Play()
    {
        if (director)
            director.Play();
    }
}
