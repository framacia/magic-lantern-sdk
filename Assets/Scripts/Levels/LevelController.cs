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

    public void PlayLevelByReference(Level levelIndex)
    {
        //If it exists
        if (levels.Contains(levelIndex))
        {
            PlayLevel(levels.IndexOf(levelIndex));
        }
    }

    private void PlayLevel(int index)
    {
        if (levels.Count > index)
        {
            currentLevel = index;
            levels[currentLevel].PlayTimeline();
            Debug.LogFormat("<color=yellow>LevelController: Playing Level {0}</color>", currentLevel + 1);

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
            Debug.LogErrorFormat("Tried to load Level with index {0} that is not in LevelController list", index);
        }
    }

    private void Update()
    {
        DebugForceChangeLevel();

        //I think there will be logic issues if we let timelines actually "stop/finish" instead of pausing them at the last frame. Consider.
    }

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
    }
}
