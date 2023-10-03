using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FranTest.GameBuilder
{
    [ExecuteInEditMode]
    public class GameManager : MonoBehaviour
    {
        //Singleton
        public static GameManager Instance { get; private set; }

        [SerializeField] ARMLGameSO loadedGameSO;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public void LoadGame(ARMLGameSO game)
        {
            loadedGameSO = game;
        }

        private void OnValidate()
        {
            
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}