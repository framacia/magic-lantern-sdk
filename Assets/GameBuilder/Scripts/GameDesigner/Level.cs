using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int LevelIndex { get; private set; }

    public Level(int levelIndex)
    {
        LevelIndex = levelIndex;
    }

    GameObject go;
}
