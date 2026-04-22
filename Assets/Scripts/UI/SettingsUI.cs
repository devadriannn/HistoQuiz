using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Music Toggles")]
    public Toggle musicOnToggle;
    public Toggle musicOffToggle;

    [Header("SFX Toggles")]
    public Toggle sfxOnToggle;
    public Toggle sfxOffToggle;

    [Header("Language Toggles")]
    public Toggle languageEnglishToggle;
    public Toggle languageFilipinoToggle;

    [Header("Difficulty Toggles")]
    public Toggle difficultyEasyToggle;
    public Toggle difficultyMediumToggle;
    public Toggle difficultyHardToggle;

    public SceneLoader loader;

    public static bool isAdditive = false;

    private void Start()
    {
        if (SettingsManager.Instance == null) return;

        // Initialize Music
        if (musicOnToggle != null) musicOnToggle.SetIsOnWithoutNotify(SettingsManager.Instance.musicOn);
        if (musicOffToggle != null) musicOffToggle.SetIsOnWithoutNotify(!SettingsManager.Instance.musicOn);

        // Initialize SFX
        if (sfxOnToggle != null) sfxOnToggle.SetIsOnWithoutNotify(SettingsManager.Instance.sfxOn);
        if (sfxOffToggle != null) sfxOffToggle.SetIsOnWithoutNotify(!SettingsManager.Instance.sfxOn);

        // Initialize Language
        if (languageEnglishToggle != null) languageEnglishToggle.SetIsOnWithoutNotify(SettingsManager.Instance.language == "English");
        if (languageFilipinoToggle != null) languageFilipinoToggle.SetIsOnWithoutNotify(SettingsManager.Instance.language == "Filipino");

        // Initialize Difficulty
        if (difficultyEasyToggle != null) difficultyEasyToggle.SetIsOnWithoutNotify(SettingsManager.Instance.difficulty == "Easy");
        if (difficultyMediumToggle != null) difficultyMediumToggle.SetIsOnWithoutNotify(SettingsManager.Instance.difficulty == "Medium");
        if (difficultyHardToggle != null) difficultyHardToggle.SetIsOnWithoutNotify(SettingsManager.Instance.difficulty == "Hard");

        // Add Listeners
        if (musicOnToggle != null) musicOnToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetMusic(true); });
        if (musicOffToggle != null) musicOffToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetMusic(false); });
        
        if (sfxOnToggle != null) sfxOnToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetSFX(true); });
        if (sfxOffToggle != null) sfxOffToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetSFX(false); });

        if (languageEnglishToggle != null) languageEnglishToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetLanguage("English"); });
        if (languageFilipinoToggle != null) languageFilipinoToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetLanguage("Filipino"); });

        if (difficultyEasyToggle != null) difficultyEasyToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty("Easy"); });
        if (difficultyMediumToggle != null) difficultyMediumToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty("Medium"); });
        if (difficultyHardToggle != null) difficultyHardToggle.onValueChanged.AddListener((isOn) => { if (isOn) SetDifficulty("Hard"); });
    }

    private void SetMusic(bool isOn)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.musicOn = isOn;
            SettingsManager.Instance.SaveSettings();
            if (AudioManager.Instance != null) AudioManager.Instance.UpdateMusicState();
        }
    }

    private void SetSFX(bool isOn)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.sfxOn = isOn;
            SettingsManager.Instance.SaveSettings();
        }
    }

    private void SetLanguage(string lang)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.language = lang;
            SettingsManager.Instance.SaveSettings();
        }
    }

    private void SetDifficulty(string diff)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.difficulty = diff;
            SettingsManager.Instance.SaveSettings();
        }
    }

    public void Back()
    {
        if (isAdditive)
        {
            isAdditive = false; // Reset flag
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Settings");
            return;
        }

        if (loader != null)
            loader.LoadScene("StudentDashboard");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("StudentDashboard");
    }
}