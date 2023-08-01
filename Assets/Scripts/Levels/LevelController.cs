using RosMessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class LevelController : MonoBehaviour
{
    [SerializeField] List<Level> levels;
    private int currentLevel;

    private void Start()
    {
        //Set level index to levels
        for(int i = 0; i < levels.Count; i++)
        {
            levels[i].levelIndex = i;
        }
    }

    public void PlayNextLevel()
    {
        //If it exists
        if (levels[currentLevel++])
            currentLevel++;
        PlayLevel();
    }

    public void PlayLevelIndex(Level levelIndex)
    {
        //If it exists
        if (levels.Contains(levelIndex))
        {
            currentLevel = levels.IndexOf(levelIndex);
            PlayLevel();
        }
        else
            Debug.LogError("Tried to load a Level that is not in LevelController list");
    }

    private void PlayLevel()
    {
        levels[currentLevel].Play();
        Debug.LogFormat("Playing level {0}", currentLevel);
    }
}
