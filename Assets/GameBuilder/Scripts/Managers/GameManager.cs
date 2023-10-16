using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FranTest.GameBuilder
{
    [ExecuteInEditMode]
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (!Application.isPlaying) { return; }
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion

        [SerializeField] ARMLGameSO loadedGameSO;
        private int currentScore;
        private float currentTime;

        private void Start()
        {
            loadedGameSO.LoadScores($"/{loadedGameSO.GetGameName()}.json");
        }

        public void LoadGame(ARMLGameSO game)
        {
            loadedGameSO = game;
        }

        public void SaveScore(string playerName)
        {
            loadedGameSO.AddHighScore(new ScoreEntry(currentScore, Time.realtimeSinceStartup, playerName));
        }

        public void OnApplicationQuit()
        {
            //AssetDatabase.SaveAssets();    
        }

    }
}