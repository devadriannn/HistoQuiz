using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string mainMenuSceneName = "StudentDashboard";

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

    private void Start()
    {
        PlayerProgress progress = LeaderboardManager.Instance.GetPlayerProgress();

        if (highScoreText != null)
        {
            string sectionLine = string.IsNullOrWhiteSpace(progress.playerSection) ? string.Empty : "\nSection: " + progress.playerSection;
            highScoreText.text =
                "Player: " + progress.playerName +
                sectionLine +
                "\nBest Score: " + progress.highScore +
                "\nGames Played: " + progress.gamesPlayed +
                "\nPerfect Runs: " + progress.perfectRuns +
                "\nCoins: " + progress.coins +
                "\nStars: " + progress.stars;
        }

        if (coinsText != null)
        {
            coinsText.text = "Leaderboard moved to Firebase.";
        }
    }

    public void Back()
    {
        ResolveLoader();
        if (loader != null)
        {
            loader.LoadScene(mainMenuSceneName);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
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
    }
