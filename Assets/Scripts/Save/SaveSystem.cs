using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerProgress
{
    public string playerName;
    public string playerSection;
    public int playerAge;
    public int highScore;
    public int coins;
    public int stars;
    public int gamesPlayed;
    public int perfectRuns;
}

public static class SaveSystem
{
    private const string HighScoreKey = "HighScore";
    private const string CoinsKey = "Coins";
    private const string StarsKey = "Stars";
    private const string SelectedModeKey = "SelectedMode";
    private const string PlayerNameKey = "PlayerName";
    private const string PlayerAgeKey = "PlayerAge";
    private const string PlayerSectionKey = "PlayerSection";
    private const string GamesPlayedKey = "GamesPlayed";
    private const string PerfectRunsKey = "PerfectRuns";

    public static void SaveHighScore(int score)
    {
        int best = LoadHighScore();
        if (score <= best)
        {
            return;
        }

        PlayerPrefs.SetInt(HighScoreKey, Mathf.Max(0, score));
        PlayerPrefs.Save();
    }

    public static int LoadHighScore()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public static void SaveCoins(int coins)
    {
        PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, coins));
        PlayerPrefs.Save();
    }

    public static int LoadCoins()
    {
        return PlayerPrefs.GetInt(CoinsKey, 0);
    }

    public static void SaveStars(int stars)
    {
        PlayerPrefs.SetInt(StarsKey, Mathf.Max(0, stars));
        PlayerPrefs.Save();
    }

    public static int LoadStars()
    {
        return PlayerPrefs.GetInt(StarsKey, 0);
    }

    public static void SaveSelectedMode(string selectedMode)
    {
        PlayerPrefs.SetString(SelectedModeKey, string.IsNullOrWhiteSpace(selectedMode) ? "FourPics" : selectedMode);
        PlayerPrefs.Save();
    }

    public static string LoadSelectedMode()
    {
        return PlayerPrefs.GetString(SelectedModeKey, "FourPics");
    }

    public static void SavePlayerProfile(string playerName, int playerAge, string playerSection)
    {
        PlayerPrefs.SetString(PlayerNameKey, NormalizeName(playerName, "Ken"));
        PlayerPrefs.SetInt(PlayerAgeKey, Mathf.Clamp(playerAge, 0, 130));
        PlayerPrefs.SetString(PlayerSectionKey, NormalizeName(playerSection, string.Empty));
        PlayerPrefs.Save();
    }

    public static string LoadPlayerName(string fallback = "Ken")
    {
        return NormalizeName(PlayerPrefs.GetString(PlayerNameKey, fallback), fallback);
    }

    public static int LoadPlayerAge()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(PlayerAgeKey, 0));
    }

    public static string LoadPlayerSection()
    {
        return NormalizeName(PlayerPrefs.GetString(PlayerSectionKey, string.Empty), string.Empty);
    }

    public static PlayerProgress LoadProgress()
    {
        return new PlayerProgress
        {
            playerName = LoadPlayerName(),
            playerSection = LoadPlayerSection(),
            playerAge = LoadPlayerAge(),
            highScore = LoadHighScore(),
            coins = LoadCoins(),
            stars = LoadStars(),
            gamesPlayed = PlayerPrefs.GetInt(GamesPlayedKey, 0),
            perfectRuns = PlayerPrefs.GetInt(PerfectRunsKey, 0)
        };
    }

    private static void IncrementInt(string key)
    {
        PlayerPrefs.SetInt(key, Mathf.Max(0, PlayerPrefs.GetInt(key, 0)) + 1);
    }

    public static void ClearAll()
    {
        // Game Progress
        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey(StarsKey);
        PlayerPrefs.DeleteKey(SelectedModeKey);
        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.DeleteKey(PlayerAgeKey);
        PlayerPrefs.DeleteKey(PlayerSectionKey);
        PlayerPrefs.DeleteKey(GamesPlayedKey);
        PlayerPrefs.DeleteKey(PerfectRunsKey);
        
        // Map Progress
        PlayerPrefs.DeleteKey("MapManager.HighestUnlockedStage");

        // Firebase Auth Cache
        PlayerPrefs.DeleteKey("FirebaseIdToken");
        PlayerPrefs.DeleteKey("FirebaseRefreshToken");
        PlayerPrefs.DeleteKey("FirebaseLocalId");
        PlayerPrefs.DeleteKey("FirebaseEmail");

        PlayerPrefs.Save();
    }

    private static string NormalizeName(string value, string fallback)
    {
        string trimmed = (value ?? string.Empty).Trim();
        return string.IsNullOrEmpty(trimmed) ? fallback : trimmed;
    }
}
