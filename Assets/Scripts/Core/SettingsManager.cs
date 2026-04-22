using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public bool musicOn = true;
    public bool sfxOn = true;
    public string language = "English";
    public string difficulty = "Easy";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("musicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("sfxOn", sfxOn ? 1 : 0);
        PlayerPrefs.SetString("language", language);
        PlayerPrefs.SetString("difficulty", difficulty);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        musicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;
        sfxOn = PlayerPrefs.GetInt("sfxOn", 1) == 1;
        language = PlayerPrefs.GetString("language", "English");
        difficulty = PlayerPrefs.GetString("difficulty", "Easy");
    }

    public void ResetToDefaults()
    {
        musicOn = true;
        sfxOn = true;
        SaveSettings();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateMusicState(true);
        }
    }
    }