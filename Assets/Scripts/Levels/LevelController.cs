using RosMessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class LevelController : MonoBehaviour
{
    [SerializeField, ReadOnly] private List<Level> levels = new List<Level>();
    private int currentLevel;

    private void Start()
    {
        //Current child-based automatic level assign mode
        foreach(Transform child in transform)
        {
            Debug.Log(child.name);
            levels.Add(child.GetComponent<Level>());
        }

        // Previous manual list mode
        ////Set level index to levels
        //for (int i = 0; i < levels.Count; i++)
        //{
        //    levels[i].levelIndex = i;
        //}
    }

    public void PlayNextLevel()
    {
        PlayLevel(currentLevel++);
    }

    public void PlayLevelByReference(Level levelIndex)
    {
        //If it exists
        if (levels.Contains(levelIndex))
        {
            PlayLevel(levels.IndexOf(levelIndex));
        }
    }

    public void PlayLevel(int index)
    {
        if (levels[index])
        {
            currentLevel = index;
            levels[currentLevel].PlayTimeline();
            Debug.LogFormat("Playing level {0}", currentLevel);

            //Stop all other level timelines
            foreach (var level in levels)
            {
                if (level.levelIndex == index)
                    continue;

                if (level.levelIndex > index)
                    level.StopTimeline(false);
                else
                    level.StopTimeline(true);
            }
        }
        else
        {
            Debug.LogErrorFormat("Tried to load Level with index {0} that is not in LevelController list", index);
        }
    }

    private void Update()
    {
        DebugForceChangeLevel();
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
