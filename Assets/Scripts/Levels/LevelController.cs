using RosMessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class LevelController : MonoBehaviour
{
    [SerializeField, ReadOnly] private List<Level> levels = new List<Level>();
    private int currentLevel;

    #region "Singleton"
    public static LevelController Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    private void Start()
    {
        //Current child-based automatic level assign mode
        foreach (Transform child in transform)
        {
            levels.Add(child.GetComponent<Level>());
        }

        //Assign level index to levels
        foreach (var level in levels)
        {
            level.levelIndex = levels.IndexOf(level);
        }

        PlayLevel(0);
    }

    public void PlayNextLevel()
    {
        int nextLevel = currentLevel + 1;
        PlayLevel(nextLevel);
    }

    /// <summary>
    /// Plays level attached in parameter, usually done through a UnityEvent in the inspector
    /// </summary>
    /// <param name="level">The level.</param>
    public void PlayLevelByReference(Level level)
    {
        //If it exists
        if (levels.Contains(level))
        {
            PlayLevel(levels.IndexOf(level));
        }
    }

    /// <summary>
    /// Plays the level, if it has a timeline, it will play it. Other levels are stopped
    /// </summary>
    /// <param name="index">The level index.</param>
    private void PlayLevel(int index)
    {
        if (levels.Count > index)
        {
            currentLevel = index;
            levels[currentLevel].PlayTimeline();
            Debug.LogFormat("<color=yellow>LevelController: Playing Level {0}</color>", currentLevel + 1);

            //Temporary solution, pause all other level timelines
            foreach (var level in levels)
            {
                if (level.levelIndex == index)
                {
                    //Debug.LogFormat("{0}: Level index already loaded.", gameObject.name);
                    continue;
                }

                level.PauseTimeline();
            }

            //This is force level change debug stuff, rethink it but meanwhile commenting it
            //Stop all other level timelines
            //foreach (var level in levels)
            //{
            //    if (level.levelIndex == index)
            //    {
            //        Debug.LogFormat("{0}: Level index already loaded.", gameObject.name);
            //        continue;
            //    }

            //    //If it's a future level, stop it and set the end time to the start
            //    if (level.levelIndex > index)
            //        level.PauseTimeline(false);
            //    //If it's a previous level, stop it and set the end time to the end of the timeline
            //    else
            //        //level.PauseTimeline(true);
            //        level.PauseTimeline(true);
            //}
        }
        else
        {
            Debug.LogErrorFormat("Tried to load Level number {0} that is not in LevelController list", index + 1);
        }
    }

    private void Update()
    {
        DebugForceChangeLevel();
    }

    /// <summary>
    /// Forces the selected level to Play
    /// </summary>
    void DebugForceChangeLevel()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlayLevel(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            PlayLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            PlayLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            PlayLevel(3);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            PlayLevel(4);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            PlayLevel(5);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            PlayLevel(6);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            PlayLevel(7);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            PlayLevel(8);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            PlayLevel(9);
    }
}
