using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FranTest.GameBuilder
{
    [CreateAssetMenu(menuName = "ARML/Create New Game", fileName = "ARML Game")]
    public class ARMLGameSO : ScriptableObject
    {
        [SerializeField] string gameName;

        /// <summary>
        /// Returns the list of tasks
        /// </summary>
        public string GetGameName()
        {
            return gameName;
        }
    }

}
