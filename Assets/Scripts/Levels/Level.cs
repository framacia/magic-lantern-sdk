using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Playables;

public class Level : MonoBehaviour
{
    [Tooltip("Unsure if having this automatically go to next Level when its timeline ends, or once all conditions are met. Think about it")]
    public bool autoFinish = false;
    [HideInInspector] public int levelIndex;

    [SerializeField] int conditionsToMeet;
    private int conditionsAlreadyMet;

    private PlayableDirector director;
    private double currentTime;
    private List<GameObject> children = new List<GameObject>();

    /// <summary>
    /// Gets director component and sets up initial values (play on awake/wrap mode)
    /// </summary>
    private void Awake()
    {
        director = GetComponent<PlayableDirector>();

        if (!director)
            return;

        director.playOnAwake = false;
        director.extrapolationMode = DirectorWrapMode.Hold;
    }

    private void Start()
    {
        //Populate children list to hold reference
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
            print(child.name);
        }

        //By default, deactivate at start to avoid seeing the level before it's played
        SetChildrenActive(false);
    }

    private void Update()
    {
        CheckIfTimelineHasReachedEnd();
    }

    /// <summary>
    /// Checks if timeline has reached last frame, if it has it gets paused and the next Level is autoplayed accordingly
    /// </summary>
    private void CheckIfTimelineHasReachedEnd()
    {
        if (!director) return;

        if (director.time >= director.duration && director.playableGraph.IsPlaying())
        {
            Debug.Log("Level " + (levelIndex + 1).ToString() + " timeline has ended");
            director.Pause();
            if (autoFinish)
                LevelController.Instance.PlayNextLevel();
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

    /// <summary>
    /// Plays the Level timeline from the beginning
    /// </summary>
    public void PlayTimeline()
    {
        if (director)
        {
            director.time = 0f;
            director.Play();
        }

        SetChildrenActive(true);
    }

    /// <summary>
    /// Resumes the timeline.
    /// </summary>
    public void ResumeTimeline()
    {
        if (director)
        {
            director.Play();
        }

        SetChildrenActive(true);
    }

    public void StopTimeline()
    {
        if (!director) return;

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


    /// <summary>
    /// Resets the timeline back to first frame and pauses it
    /// </summary>
    public void ResetTimeline()
    {
        if (!director) return;

        director.time = 0f;
        director.Play(); //Force play for one frame to get binding - workaround
        director.Evaluate();
        director.Pause();
    }

    /// <summary>
    /// Pauses the timeline and keeps it in the current frame
    /// </summary>
    public void PauseTimeline()
    {
        SetChildrenActive(false);

        if (!director) return;

        director.Pause();
    }

    //Fast forwards timeline and then pauses in the last frame to simulate going through it
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

    private void SetChildrenActive(bool state)
    {
        //Using a list preloaded with references has the risk of ignoring instantiated gameObjects, but unlikely
        foreach(var child in children)
            child.SetActive(state);
    }

    public void CompleteCondition()
    {
        conditionsAlreadyMet++;

        if(conditionsAlreadyMet >= conditionsToMeet && autoFinish)
        {
            //Finish Level
            LevelController.Instance.PlayNextLevel();
        }
    }
}
