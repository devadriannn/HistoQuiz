using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginValidation : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;

    [Header("UI Elements")]
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnLoginClicked);
            loginButton.onClick.AddListener(OnLoginClicked);
        }

        if (statusText != null)
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
    }

    public void OnLoginClicked()
    {
        if (ValidateInputs())
        {
            ShowStatus("Logging in...", new Color(0.35f, 0.12f, 0.08f, 1f));
            // Proceed with login logic...
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(usernameField.text))
        {
            ShowStatus("Please input your email or username.", Color.red);
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordField.text))
        {
            ShowStatus("Please input your password.", Color.red);
            return false;
        }

        if (usernameField.text.Length < 3 || passwordField.text.Length < 4)
        {
            ShowStatus("Email or password is too short.", Color.red);
            return false;
        }

        return true;
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
            statusText.gameObject.SetActive(true);
        }
    }
}