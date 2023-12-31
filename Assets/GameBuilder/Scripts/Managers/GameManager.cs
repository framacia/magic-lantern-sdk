using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FranTest.GameBuilder
{
    //[ExecuteInEditMode]
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }
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
        [SerializeField] bool loadOnStart;
        private int currentScore;
        private float currentTime;

        private void Start()
        {
            if(loadOnStart)
            {
                StartCoroutine(SceneController.Instance.LoadSceneByReference(loadedGameSO.gameScene));
                loadOnStart = false;
            }

            // Check if the loaded game scriptable object uses scores.
            if (!loadedGameSO.usesScores)
                return;

            // Load scores from a JSON file based on the game's name.
            //loadedGameSO.LoadScores($"/{loadedGameSO.GetGameName()}.json");
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.L))
            {
                StartCoroutine(SceneController.Instance.LoadSceneByReference(loadedGameSO.gameScene));
            }
        }

        /// <summary>
        /// Load a game by setting the active ARMLGameSO.
        /// </summary>
        /// <param name="game">The ARMLGameSO to load.</param>
        public void LoadGame(ARMLGameSO game)
        {
            loadedGameSO = game;
        }

        /// <summary>
        /// Save the player's score along with their name.
        /// </summary>
        /// <param name="playerName">The name of the player.</param>
        public void SaveScore(string playerName)
        {
            // Check if the loaded game scriptable object uses scores.
            if (!loadedGameSO.usesScores)
                return;

            // Add a new high score entry to the loaded ARMLGameSO.
            loadedGameSO.AddHighScore(new ScoreEntry(currentScore, Time.realtimeSinceStartup, playerName));
        }
    }
}
