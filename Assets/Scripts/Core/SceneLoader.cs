using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private float minimumLoadingDuration = 3.5f;

    private bool _isTransitioning = false;

    public void LoadScene(string sceneName)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneWithDelay(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        float elapsed = 0f;
        while (elapsed < minimumLoadingDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            
            // Simple rotation matching logout approach
            if (loadingScreen != null)
            {
                loadingScreen.transform.Rotate(0f, 0f, -240f * Time.unscaledDeltaTime);
            }

            yield return null;
        }

        // Start loading the scene
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"Failed to start loading scene '{sceneName}'. Make sure it's added to the Build Settings.");
            _isTransitioning = false;
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
            yield break;
        }

        // Just wait until it's finished.
        while (!op.isDone)
        {
            yield return null;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
