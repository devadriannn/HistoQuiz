using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgotPasswordManager : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField emailField;

    [Header("UI Panels")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject modalPanel;

    [Header("Modal UI")]
    [SerializeField] private TMP_Text modalHeader;
    [SerializeField] private TMP_Text modalBody;
    [SerializeField] private Button modalCloseBtn;

    [Header("Status Feedback")]
    [SerializeField] private TMP_Text statusText;

    private bool isProcessing;

    private void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (modalPanel != null) modalPanel.SetActive(false);
        
        if (statusText != null)
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }
        
        if (modalCloseBtn != null)
        {
            modalCloseBtn.onClick.AddListener(CloseModal);
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            bool isValid = addr.Address.Equals(email.Trim(), System.StringComparison.OrdinalIgnoreCase) && email.Contains(".");
            Debug.Log($"[ForgotPassword] IsValidEmail check for '{email}': {isValid}");
            return isValid;
        }
        catch (System.Exception ex)
        {
            Debug.Log($"[ForgotPassword] IsValidEmail exception for '{email}': {ex.Message}");
            return false;
        }
    }

    public void SendResetEmail()
    {
        Debug.Log("[ForgotPassword] SendResetEmail called.");
        if (isProcessing) 
        {
            Debug.Log("[ForgotPassword] SendResetEmail ignored: already processing.");
            return;
        }

        string email = emailField != null ? emailField.text.Trim() : "";
        Debug.Log("[ForgotPassword] Retreived email: '" + email + "'");

        if (statusText != null) 
        {
            statusText.text = "";
            statusText.gameObject.SetActive(false);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            Debug.Log("[ForgotPassword] Email is empty.");
            ShowModal("Attention!", "Please enter your email address.");
            return;
        }

        if (!IsValidEmail(email))
        {
            Debug.Log("[ForgotPassword] Validation UI should show now.");
            if (statusText != null)
            {
                statusText.gameObject.SetActive(true);
                statusText.text = "Please enter a valid email address";
                statusText.color = Color.red;
                statusText.alpha = 1f;
                Debug.Log("[ForgotPassword] StatusText set to: " + statusText.text);
            }
            else
            {
                Debug.LogError("[ForgotPassword] statusText reference is NULL!");
            }
            return;
        }

        StartCoroutine(SendResetEmailRoutine(email));
    }

    private IEnumerator SendResetEmailRoutine(string email)
    {
        isProcessing = true;
        if (loadingPanel != null) 
        {
            loadingPanel.SetActive(true);
            var loadingTxt = loadingPanel.transform.Find("LoadingTxt")?.GetComponent<TMP_Text>();
            if (loadingTxt != null) loadingTxt.text = "Sending Request...";
        }

        bool finished = false;
        bool success = false;
        string errorMsg = "";

        // We skip the database check because it requires 'read' permissions 
        // that guest users don't have. Firebase Auth will handle it.
        FirebaseManager.Instance.SendPasswordReset(email, (done, error) =>
        {
            success = done;
            errorMsg = error;
            finished = true;
        });

        yield return new WaitUntil(() => finished);

        if (loadingPanel != null) loadingPanel.SetActive(false);
        isProcessing = false;

        if (success)
        {
            ShowModal("Success!", "If this email is registered, you will receive a password reset link shortly. Please check your inbox.");
            if (emailField != null) emailField.text = "";
        }
        else
        {
            ShowModal("Attention!", errorMsg ?? "An error occurred. Please try again later.");
        }
    }

    public void ShowModal(string header, string body)
    {
        if (modalPanel == null) return;

        if (modalHeader != null) modalHeader.text = header;
        if (modalBody != null) modalBody.text = body;

        modalPanel.SetActive(true);
    }

    public void CloseModal()
    {
        if (modalPanel != null) modalPanel.SetActive(false);
    }
}
