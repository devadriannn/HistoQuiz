using UnityEngine;
using UnityEngine.UI;

public class SettingsTabManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject audioPanel;
    public GameObject accountPanel;

    [Header("Audio Tab Buttons")]
    public Button[] audioTabButtons; // Buttons that switch to Audio tab

    [Header("Account Tab Buttons")]
    public Button[] accountTabButtons; // Buttons that switch to Account tab

    [Header("Colors")]
    private Color selectedColor = new Color(0.6156f, 0.6156f, 0.6156f); // #9D9D9D
    private Color defaultColor = Color.white;

    private void Start()
    {
        // Force initial state
        ShowAudioSettings();
    }

    public void ShowAudioSettings()
    {
        Debug.Log("[SettingsTabManager] Showing Audio Settings");
        if (audioPanel != null) audioPanel.SetActive(true);
        if (accountPanel != null) accountPanel.SetActive(false);

        UpdateVisuals(true);
    }

    public void ShowAccountSettings()
    {
        Debug.Log("[SettingsTabManager] Showing Account Settings");
        if (audioPanel != null) audioPanel.SetActive(false);
        if (accountPanel != null) accountPanel.SetActive(true);

        // Sync data when opening account tab
        AccountSettingsManager accountManager = GetComponentInChildren<AccountSettingsManager>(true);
        if (accountManager != null) accountManager.LoadUserData();

        UpdateVisuals(false);
    }

    private void UpdateVisuals(bool isAudioActive)
    {
        // Color all "Audio" tab buttons
        foreach (var btn in audioTabButtons)
        {
            SetButtonColor(btn, isAudioActive ? selectedColor : defaultColor);
        }

        // Color all "Account" tab buttons
        foreach (var btn in accountTabButtons)
        {
            SetButtonColor(btn, isAudioActive ? defaultColor : selectedColor);
        }
    }

    private void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = color;
        }
    }
}
