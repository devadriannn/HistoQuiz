using System.Collections.Generic;
using UnityEngine;

public static class PlayerData
{
    private const string UserIdKey = "PlayerData.UserId";
    private const string NameKey = "PlayerData.Name";
    private const string UsernameKey = "PlayerData.Username";
    private const string EmailKey = "PlayerData.Email";
    private const string RoleKey = "PlayerData.Role";
    private const string PhotoUrlKey = "PlayerData.PhotoUrl";
    private const string CorrectQuestionsKey = "PlayerData.CorrectQuestions";

    public static string UserId { get; private set; }
    public static string Name { get; private set; }
    public static string Username { get; private set; }
    public static string Email { get; private set; }
    public static string Role { get; private set; }
    public static string PhotoUrl { get; private set; }
    public static int Score { get; set; }

    private static List<string> correctlyAnsweredQuestions = new List<string>();

    public static bool IsLoggedIn => !string.IsNullOrWhiteSpace(UserId);

    static PlayerData()
    {
        Load();
    }

    public static bool HasAnsweredCorrectly(string questionId)
    {
        return correctlyAnsweredQuestions.Contains(questionId);
    }

    public static void MarkAsCorrectlyAnswered(string questionId)
    {
        if (!correctlyAnsweredQuestions.Contains(questionId))
        {
            correctlyAnsweredQuestions.Add(questionId);
            Save();
        }
    }

    public static void SetProfile(string userId, string name, string username, string email, string role, string photoUrl)
    {
        UserId = (userId ?? string.Empty).Trim();
        Name = (name ?? string.Empty).Trim();
        Username = (username ?? string.Empty).Trim();
        Email = (email ?? string.Empty).Trim();
        Role = (role ?? string.Empty).Trim();
        PhotoUrl = (photoUrl ?? string.Empty).Trim();
        Save();
    }

    public static void SetCorrectQuestions(List<string> ids)
    {
        correctlyAnsweredQuestions = ids ?? new List<string>();
        Save();
    }

    public static void Save()
    {
        PlayerPrefs.SetString(UserIdKey, UserId ?? string.Empty);
        PlayerPrefs.SetString(NameKey, Name ?? string.Empty);
        PlayerPrefs.SetString(UsernameKey, Username ?? string.Empty);
        PlayerPrefs.SetString(EmailKey, Email ?? string.Empty);
        PlayerPrefs.SetString(RoleKey, Role ?? string.Empty);
        PlayerPrefs.SetString(PhotoUrlKey, PhotoUrl ?? string.Empty);
        
        string joined = string.Join(",", correctlyAnsweredQuestions);
        PlayerPrefs.SetString(CorrectQuestionsKey, joined);

        if (!string.IsNullOrWhiteSpace(Name))
        {
            SaveSystem.SavePlayerProfile(Name, 0, string.Empty);
        }

        PlayerPrefs.Save();
    }

    public static void Load()
    {
        UserId = PlayerPrefs.GetString(UserIdKey, string.Empty);
        Name = PlayerPrefs.GetString(NameKey, string.Empty);
        Username = PlayerPrefs.GetString(UsernameKey, string.Empty);
        Email = PlayerPrefs.GetString(EmailKey, string.Empty);
        Role = PlayerPrefs.GetString(RoleKey, string.Empty);
        PhotoUrl = PlayerPrefs.GetString(PhotoUrlKey, string.Empty);
        
        string joined = PlayerPrefs.GetString(CorrectQuestionsKey, string.Empty);
        correctlyAnsweredQuestions = string.IsNullOrWhiteSpace(joined) 
            ? new List<string>() 
            : new List<string>(joined.Split(','));
    }

    public static void Clear()
    {
        UserId = string.Empty;
        Name = string.Empty;
        Username = string.Empty;
        Email = string.Empty;
        Role = string.Empty;
        PhotoUrl = string.Empty;
        Score = 0;
        correctlyAnsweredQuestions.Clear();

        PlayerPrefs.DeleteKey(UserIdKey);
        PlayerPrefs.DeleteKey(NameKey);
        PlayerPrefs.DeleteKey(UsernameKey);
        PlayerPrefs.DeleteKey(EmailKey);
        PlayerPrefs.DeleteKey(RoleKey);
        PlayerPrefs.DeleteKey(PhotoUrlKey);
        PlayerPrefs.DeleteKey(CorrectQuestionsKey);
        
        SaveSystem.ClearAll();

        PlayerPrefs.Save();
    }
}
