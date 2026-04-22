using TMPro;
using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject createAccountPanel;
    [SerializeField] private GameObject forgotPasswordPanel;
    [SerializeField] private GameObject[] statusTexts;
    [SerializeField] private TMP_InputField[] allInputFields;

    public void ShowCreateAccount()
    {
        ResetPanels();
            
        if (loginPanel != null) loginPanel.SetActive(false);
        if (createAccountPanel != null) createAccountPanel.SetActive(true);
        if (forgotPasswordPanel != null) forgotPasswordPanel.SetActive(false);
    }

    public void ShowLogin()
    {
        ResetPanels();
            
        if (loginPanel != null) loginPanel.SetActive(true);
        if (createAccountPanel != null) createAccountPanel.SetActive(false);
        if (forgotPasswordPanel != null) forgotPasswordPanel.SetActive(false);
    }

    public void ShowForgotPassword()
    {
        ResetPanels();

        if (loginPanel != null) loginPanel.SetActive(false);
        if (createAccountPanel != null) createAccountPanel.SetActive(false);
        if (forgotPasswordPanel != null) forgotPasswordPanel.SetActive(true);
    }

    private void ResetPanels()
    {
        HideAllStatusTexts();
        ClearAllInputFields();
    }

    private void HideAllStatusTexts()
    {
        if (statusTexts == null) return;
        foreach (var status in statusTexts)
        {
            if (status != null)
                status.SetActive(false);
        }
    }

    private void ClearAllInputFields()
    {
        if (allInputFields == null) return;
        foreach (var input in allInputFields)
        {
            if (input != null)
            {
                input.text = string.Empty;
            }
        }
    }
}
