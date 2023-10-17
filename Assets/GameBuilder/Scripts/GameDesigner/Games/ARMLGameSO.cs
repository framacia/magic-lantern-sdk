using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FranTest.GameBuilder
{
    [CreateAssetMenu(menuName = "ARML/Create New Game", fileName = "ARML Game")]
    public class ARMLGameSO : ScriptableObject
    {
        [SerializeField] string gameName;
        [SerializeField] List<Level> levels;
        [SerializeField] List<ScoreEntry> highScores;
        public bool usesScores;

        private IDataService DataService = new JsonDataService();

        /// <summary>
        /// Returns name of ARML Game
        /// </summary>
        public string GetGameName()
        {
            return gameName;
        }

        public void AddHighScore(ScoreEntry sc)
        {
            highScores.Add(sc);

            if (DataService.SaveData(string.Format("/{0}.json", gameName), highScores, false))
            {
                Debug.Log("Succesfully saved score data");
            }
        }

        public void LoadScores(string path)
        {
            List<ScoreEntry> loadedScores = DataService.LoadData<List<ScoreEntry>>(path, false);

            highScores = loadedScores;
        }
    }

    [Serializable]
    public class ScoreEntry
    {
        public int score;
        public float timeToComplete;
        public string name;
        public string dateTime;

        public ScoreEntry(int _score, float _timeToComplete, string _name)
        {
            score = _score;
            timeToComplete = _timeToComplete;
            name = _name;
            dateTime = DateTime.Now.ToString();
        }
    }

}
