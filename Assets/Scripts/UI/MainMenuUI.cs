using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string playSceneName = "Questions";
    [SerializeField] private string heroesSceneName = "HeroesGallery";
    [SerializeField] private string settingsSceneName = "Settings";
    [SerializeField] private string leaderboardSceneName = "Leaderboard";
    [SerializeField] private string aboutSceneName = "About";
    [SerializeField] private string lessonSceneName = "Lesson";

    private void Reset()
    {
        ResolveLoader();
    }

    private void Awake()
    {
        ResolveLoader();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ResolveLoader();
        }
    }

    public void Play()
    {
        LoadScene(playSceneName);
    }

    public void Heroes()
    {
        LoadScene(heroesSceneName);
    }

    public void Settings()
    {
        LoadScene(settingsSceneName);
    }

    public void Back()
    {
        LoadScene("StudentDashboard");
    }

    public void Leaderboard()
    {
        LoadScene(leaderboardSceneName);
    }

    public void About()
    {
        LoadScene(aboutSceneName);
    }

    public void Lesson()
    {
        LoadScene(lessonSceneName);
    }

    private void ResolveLoader()
    {
        if (loader != null)
        {
            return;
        }

        loader = GetComponent<SceneLoader>();
        if (loader == null)
        {
            loader = GetComponentInParent<SceneLoader>();
        }
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("MainMenuUI is missing a target scene name.");
            return;
        }

        string resolvedScene = ResolveSceneReference(sceneName);
        if (string.IsNullOrWhiteSpace(resolvedScene))
        {
            Debug.LogWarning("MainMenuUI could not load scene '" + sceneName + "'. Add it to Build Profiles or fix the scene name.");
            return;
        }

        ResolveLoader();
        if (loader != null)
        {
            loader.LoadScene(resolvedScene);
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(resolvedScene);
            return;
        }

        Debug.LogWarning("MainMenuUI could not find a SceneLoader or GameManager. Loading the scene directly as a fallback.");
        SceneManager.LoadScene(resolvedScene);
    }

    private static string ResolveSceneReference(string sceneName)
    {
        string normalizedName = NormalizeSceneName(sceneName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        if (Application.CanStreamedLevelBeLoaded(normalizedName))
        {
            return normalizedName;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                continue;
            }

            string buildSceneName = Path.GetFileNameWithoutExtension(scenePath);
            if (string.Equals(buildSceneName, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scenePath, sceneName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return scenePath;
            }
        }

        return null;
    }

    private static string NormalizeSceneName(string sceneName)
    {
        string trimmed = sceneName == null ? string.Empty : sceneName.Trim();
        if (trimmed.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(trimmed);
        }

        return trimmed;
    }
}

