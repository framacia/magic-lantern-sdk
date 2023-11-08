using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField] private Image fadeToBlackTexture;

    #region Singleton
    public static SceneController Instance { get; private set; }

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
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }
    #endregion

    public IEnumerator ResetCurrentSceneAdditive()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        //Are the first 2 lines necessary?
        yield return SceneManager.UnloadSceneAsync(currentScene);
        Resources.UnloadUnusedAssets();
        StartCoroutine(LoadSceneByReference(currentScene));
        //SceneManager.LoadScene(currentScene, LoadSceneMode.Additive);
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentScene));
    }

    public void ResetCurrentSceneSingle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //StartCoroutine(ResetCurrentSceneAdditive());
            ResetCurrentSceneSingle();
        }
    }

    public IEnumerator LoadSceneByReference(string scene)
    {
        Debug.Log($"Started loading scene {scene}");
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
        Debug.Log($"Finished loading scene {scene} and set as active");
    }
}
