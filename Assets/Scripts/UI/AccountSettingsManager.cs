using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountSettingsManager : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private TMP_InputField oldPasswordField;
    [SerializeField] private TMP_InputField passwordField;

    [Header("Profile Image")]
    [SerializeField] private Image profilePreview;
    [SerializeField] private ProfileImageLoader profileLoader;
    [SerializeField] private PickImage imagePicker;
    [SerializeField] private CloudinaryUploader uploader;

    [Header("Feedback")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button saveButton;
    [SerializeField] private GameObject saveLoadingPanel;

    [Header("Password Visibility")]
    [SerializeField] private Button showPasswordBtn;
    [SerializeField] private Button hidePasswordBtn;

    private bool isProcessing;

    private void Start()
    {
        LoadUserData();
        SetupFieldResetListeners();
        if (saveLoadingPanel != null) saveLoadingPanel.SetActive(false);
        
        // Initialize password visibility
        SetPasswordVisibility(false);
        
        if (showPasswordBtn != null) showPasswordBtn.onClick.AddListener(() => SetPasswordVisibility(true));
        if (hidePasswordBtn != null) hidePasswordBtn.onClick.AddListener(() => SetPasswordVisibility(false));
    }

    public void SetPasswordVisibility(bool visible)
    {
        // Toggle Main Password Field
        if (passwordField != null)
        {
            passwordField.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            passwordField.inputType = visible ? TMP_InputField.InputType.Standard : TMP_InputField.InputType.Password;
            passwordField.ForceLabelUpdate();
        }

        // Toggle Old Password Field
        if (oldPasswordField != null)
        {
            oldPasswordField.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            oldPasswordField.inputType = visible ? TMP_InputField.InputType.Standard : TMP_InputField.InputType.Password;
            oldPasswordField.ForceLabelUpdate();
        }
        
        // Toggle buttons
        if (showPasswordBtn != null) showPasswordBtn.gameObject.SetActive(!visible);
        if (hidePasswordBtn != null) hidePasswordBtn.gameObject.SetActive(visible);
        
        Debug.Log($"[AccountSettings] Password visibility set to: {visible}");
    }

    private void SetupFieldResetListeners()
    {
        // When focus is lost, if the field is empty, revert to the current user data
        if (nameField != null) nameField.onEndEdit.AddListener((val) => { if (string.IsNullOrWhiteSpace(val)) nameField.text = PlayerData.Name; });
        
        // For passwords, if focus is lost and they are empty, just keep them empty.
    }

    public void LoadUserData()
    {
        // Load from PlayerData which is populated after login
        if (nameField != null) nameField.text = PlayerData.Name;
        
        // Clear password fields
        if (passwordField != null) passwordField.text = ""; 
        if (oldPasswordField != null) oldPasswordField.text = "";
        
        if (profileLoader != null && !string.IsNullOrEmpty(PlayerData.PhotoUrl))
        {
            profileLoader.LoadProfileImage(PlayerData.PhotoUrl);
        }

        if (statusText != null) statusText.text = "";
        
        // Ensure buttons match initial hidden state
        SetPasswordVisibility(false);
    }

    private bool HasChanges()
    {
        string currentName = nameField != null ? nameField.text.Trim() : "";
        string currentPassword = passwordField != null ? passwordField.text : "";

        bool nameChanged = currentName != (PlayerData.Name ?? "");
        bool passwordChanged = !string.IsNullOrEmpty(currentPassword);
        bool imageChanged = imagePicker != null && imagePicker.selectedTexture != null;

        Debug.Log($"[AccountSettings] Change Detection - Name: {nameChanged}, Image: {imageChanged}, Password: {passwordChanged}");
        
        return nameChanged || passwordChanged || imageChanged;
    }

    public void UpdateProfile()
    {
        Debug.Log("[AccountSettings] Save Button Clicked!");
        
        if (isProcessing) return;

        string newName = nameField != null ? nameField.text.Trim() : "";
        string newPassword = passwordField != null ? passwordField.text : "";
        string oldPassword = oldPasswordField != null ? oldPasswordField.text : "";

        if (string.IsNullOrWhiteSpace(newName))
        {
            ShowStatus("Error: Name is required.", Color.red);
            return;
        }

        // Check if password change is attempted
        if (!string.IsNullOrEmpty(newPassword))
        {
            if (string.IsNullOrEmpty(oldPassword))
            {
                ShowStatus("Error: Enter old password to change password.", Color.red);
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowStatus("Error: New password must be at least 6 characters.", Color.red);
                return;
            }
        }

        if (!HasChanges())
        {
            ShowStatus("No changes detected.", Color.yellow);
            return;
        }

        StartCoroutine(UpdateProfileRoutine(newName, oldPassword, newPassword));
    }

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

    private IEnumerator UpdateProfileRoutine(string newName, string oldPassword, string newPassword)
    {
        isProcessing = true;
        if (saveButton != null) saveButton.interactable = false;
        if (saveLoadingPanel != null) saveLoadingPanel.SetActive(true);
        
        ShowStatus("Saving changes...", Color.white);

        string photoUrl = PlayerData.PhotoUrl;
        string currentEmail = PlayerData.Email;

        // 1. Upload new image if selected
        if (imagePicker != null && imagePicker.selectedTexture != null)
        {
            ShowStatus("Uploading new photo...", Color.white);
            
            bool uploadDone = false;
            byte[] imageBytes = imagePicker.selectedTexture.EncodeToPNG();

            yield return uploader.UploadImage(imageBytes, 
                (url) => { 
                    photoUrl = url; 
                    uploadDone = true; 
                }, 
                (error) => {
                    Debug.LogError("Cloudinary Upload Error: " + error);
                    uploadDone = true;
                });
            
            while (!uploadDone) yield return null;
        }

        // 2. Update Password if requested
        if (!string.IsNullOrEmpty(newPassword))
        {
            ShowStatus("Updating password...", Color.white);
            bool passUpdateDone = false;
            bool passUpdateSuccess = false;
            string passUpdateError = "";

            FirebaseManager.Instance.UpdatePassword(oldPassword, newPassword, (success, error) =>
            {
                passUpdateSuccess = success;
                passUpdateError = error;
                passUpdateDone = true;
            });

            yield return new WaitUntil(() => passUpdateDone);

            if (!passUpdateSuccess)
            {
                FinalizeUpdate(false, passUpdateError);
                yield break;
            }
        }

        // 3. Update Firestore Profile (Name, Email, Photo)
        ShowStatus("Updating profile info...", Color.white);
        FirebaseManager.Instance.UpdateStudentProfile(newName, currentEmail, photoUrl, (success, error) =>
        {
            if (success)
            {
                // Update local PlayerData
                PlayerData.SetProfile(PlayerData.UserId, newName, PlayerData.Username, currentEmail, PlayerData.Role, photoUrl);
                FinalizeUpdate(true, "Profile updated successfully!");
            }
            else
            {
                FinalizeUpdate(false, error);
            }
        });
    }

    private void FinalizeUpdate(bool success, string message)
    {
        isProcessing = false;
        if (saveButton != null) saveButton.interactable = true;
        if (saveLoadingPanel != null) saveLoadingPanel.SetActive(false);

        ShowStatus(message, success ? Color.green : Color.red);
        
        if (success)
        {
            // Clear password fields on success
            if (passwordField != null) passwordField.text = "";
            if (oldPasswordField != null) oldPasswordField.text = "";
        }
    }
    }
