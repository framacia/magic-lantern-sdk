using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Playables;

public class Level : MonoBehaviour
{
    public bool autoFinish = false;
    [HideInInspector] public int levelIndex;

    private LevelController levelController;
    private PlayableDirector director;
    private double currentTime;

    private void Awake()
    {
        //TODO check if it's better to make it a singleton
        levelController = FindObjectOfType<LevelController>();

        director = GetComponent<PlayableDirector>();

        if (!director)
            return;

        director.playOnAwake = false;
        director.extrapolationMode = DirectorWrapMode.Hold;
    }

    private void Update()
    {
        CheckIfTimelineHasReachedEnd();
    }

    private void CheckIfTimelineHasReachedEnd()
    {
        if(!director) return;

        if(director.time >= director.duration && director.playableGraph.IsPlaying())
        {
            Debug.Log("Level " + levelIndex + 1 + " timeline has ended");
            director.Pause();
            if (autoFinish)
                levelController.PlayNextLevel();
        }
    }

    //With Wrap Mode Hold this is useless as director never really "ends"
    #region "Stop implementation"
    /*void OnEnable()
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
    }*/
    #endregion

    public void PlayTimeline()
    {
        if (director)
        {
            director.time = 0f;
            director.Play();
        }
    }

    public void ResumeTimeline()
    {
        if (director)
        {
            director.Play();
        }
    }

    public void PauseTimeline(bool setEndTime)
    {
        if (!director) return;

        //Reset state to first timeline frame
        if (!setEndTime)
        {
            director.time = 0f;
            //director.Play(); //Force play for one frame to get binding - workaround
            director.Evaluate();
            director.Pause();
        }
        //Reset to last timeline frame
        else
        {
            //Original method
            //director.time = director.duration;
            ////director.Play(); //Force play for one frame to get binding - workaround
            //director.Evaluate();
            //director.Pause();

            //Fast-forward method
            director.Play();
            director.playableGraph.GetRootPlayable(0).SetSpeed(100f);
            StartCoroutine(FastForwardLevel());
        }
    }

    //Plays the timeline very fast and then pauses in the last frame to simulate going through it (fast-forward)
    IEnumerator FastForwardLevel()
    {
        //Loop until director reaches end (minus error range for safety)
        if (director.time >= director.duration - 0.1f)
        {
            director.playableGraph.GetRootPlayable(0).SetSpeed(1f);
            director.Pause();
            StopCoroutine(FastForwardLevel());
        }
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(FastForwardLevel());       
    }
}
