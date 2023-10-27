using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField] private Image fadeToBlackTexture;
    [SerializeField] bool fadeToBlackBetweenLoads;
    [SerializeField] private float fadeDuration;

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

    private void Start()
    {
        Color tempColor = fadeToBlackTexture.color;
        tempColor.a = 0f;
        fadeToBlackTexture.color = tempColor;
    }

    public IEnumerator ResetCurrentSceneAdditive()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        //Are the first 2 lines necessary?
        yield return SceneManager.UnloadSceneAsync(currentScene);
        Resources.UnloadUnusedAssets();
        StartCoroutine(LoadSceneByReference(currentScene));
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
        // If Game Scene is already loaded, return
        if (SceneManager.GetActiveScene().name == scene)
        {
            Debug.Log($"Scene {scene} already loaded");
            yield break;
        }

        //Fade to Black
        if (fadeToBlackBetweenLoads)
        {
            float timeElapsed = 0f;
            Color fadeColor = fadeToBlackTexture.color;
            while (timeElapsed < fadeDuration)
            {
                fadeColor.a = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration);
                fadeToBlackTexture.color = fadeColor;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            fadeColor.a = 1f;
            fadeToBlackTexture.color = fadeColor;
        }

        //Load Level
        Debug.Log($"Started loading scene {scene}");
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        //SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
        Debug.Log($"Finished loading scene {scene} and set as active");

        //Set Camera transform to Scene Origin 
        FindObjectOfType<CameraParentController>()?.MoveToSceneOrigin();

        //Fade back to game
        if (fadeToBlackBetweenLoads)
        {
            float timeElapsed = 0f;
            Color fadeColor = fadeToBlackTexture.color;
            while (timeElapsed < fadeDuration)
            {
                fadeColor.a = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
                fadeToBlackTexture.color = fadeColor;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            fadeColor.a = 0f;
            fadeToBlackTexture.color = fadeColor;
        }
    }
}
