using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LoginManager : MonoBehaviour
{
    [Header("Login UI")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text emailErrorText;
    [SerializeField] private TMP_Text passwordErrorText;

    [Header("Scenes")]
    [SerializeField] private string studentSceneName = "StudentDashboard";
    [SerializeField] private string teacherSceneName = "TeacherDashboard";

    [Header("Loading UI")]
[SerializeField] private GameObject loadingUI;
    [SerializeField] private float successDelay = 1.0f;

    private bool isLoggingIn;

    private void Awake()
    {
        ResolveReferences();
        if (loadingUI != null) loadingUI.SetActive(false);
        ClearErrorTexts();
        SetStatus(string.Empty);
    }

    private void Start()
    {
        ResolveReferences();
        FirebaseManager.Instance.InitializeFirebase();
        
        // Add listeners to clear errors when user types
        if (usernameField != null) 
        {
            usernameField.onValueChanged.AddListener((_) => ClearEmailError());
            usernameField.onSubmit.AddListener((_) => LoginUser());
        }

        if (passwordField != null) 
        {
            passwordField.onValueChanged.AddListener((_) => ClearPasswordError());
            passwordField.onSubmit.AddListener((_) => LoginUser());
        }
    }

    private void ClearErrorTexts()
    {
        ClearEmailError();
        ClearPasswordError();
    }

    private void ClearEmailError()
    {
        if (emailErrorText != null) 
        {
            emailErrorText.text = "";
            emailErrorText.gameObject.SetActive(false);
            // Also hide parent if it's the specific error handler object
            if (emailErrorText.transform.parent != null && emailErrorText.transform.parent.name.Contains("Error"))
                emailErrorText.transform.parent.gameObject.SetActive(false);
        }
    }

    private void ClearPasswordError()
    {
        if (passwordErrorText != null) 
        {
            passwordErrorText.text = "";
            passwordErrorText.gameObject.SetActive(false);
            // Also hide parent if it's the specific error handler object
            if (passwordErrorText.transform.parent != null && passwordErrorText.transform.parent.name.Contains("Error"))
                passwordErrorText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void Login()
    {
        LoginUser();
    }

    public void LoginUser()
    {
        ResolveReferences();

        if (isLoggingIn)
        {
            Debug.Log("Login already in progress...");
            return;
        }

        ClearErrorTexts();
        SetStatus(string.Empty);

        string identifier = usernameField != null ? usernameField.text.Trim() : string.Empty;
        string password = passwordField != null ? passwordField.text : string.Empty;

        Debug.Log($"Login attempt with Identifier: '{identifier}', Password length: {password.Length}");

        bool hasError = false;

        // 1. Email validation
        if (string.IsNullOrWhiteSpace(identifier))
        {
            ShowEmailError("Please input your email.");
            hasError = true;
        }
        else if (!IsValidEmail(identifier))
        {
            if (!identifier.Contains("@"))
            {
                ShowEmailError("Email must include '@' and a valid domain.");
            }
            else if (!identifier.Contains("."))
            {
                ShowEmailError("Email must include a domain like '.com'.");
            }
            else
            {
                ShowEmailError("Please enter a valid email address.");
            }
            hasError = true;
        }

        // Additional email validations
        if (!hasError)
        {
            if (identifier.Contains(" "))
            {
                ShowEmailError("Email should not contain spaces.");
                hasError = true;
            }
            else if (identifier != identifier.ToLower())
            {
                ShowEmailError("Email should be in lowercase.");
                hasError = true;
            }
            else if (identifier.StartsWith("@") || identifier.EndsWith("@"))
            {
                ShowEmailError("Please enter a valid email address.");
                hasError = true;
            }
            else if (identifier.Contains(".."))
            {
                ShowEmailError("Please enter a valid email address.");
                hasError = true;
            }
        }

        // 2. Password validation
        if (string.IsNullOrWhiteSpace(password))
        {
            Debug.Log("Password is empty, showing error.");
            ShowPasswordError("Please input your password.");
            hasError = true;
        }
        else if (password.Length < 4)
        {
            ShowPasswordError("Password must be at least 4 characters.");
            hasError = true;
        }

        // Additional sign-in validation for password
        if (!hasError)
        {
            if (password.Contains(" "))
            {
                ShowPasswordError("Password should not contain spaces.");
                hasError = true;
            }
            else if (password.Length > 128)
            {
                ShowPasswordError("Password is too long.");
                hasError = true;
            }
        }

        if (hasError) 
        {
            Debug.Log("Validation failed, stopping login.");
            return;
        }

        Debug.Log("Validation passed, calling Firebase...");
        SetStatus("");
        SetInputsInteractable(false);
        if (loadingUI != null) 
        {
            loadingUI.SetActive(true);
            var loadingTxt = loadingUI.transform.Find("LoadingTxt")?.GetComponent<TMP_Text>();
            if (loadingTxt != null) loadingTxt.text = "Logging In...";
        }
        isLoggingIn = true;

        try 
        {
            FirebaseManager.Instance.SignInWithIdentifier(identifier, password, HandleSignInResult);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Sign-in initiation failed: {ex.Message}");
            HandleLoginFailure("System error. Please try again.");
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) return false;
        
        return System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,7}$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private void ShowEmailError(string message)
    {
        if (emailErrorText != null)
        {
            emailErrorText.text = message;
            emailErrorText.gameObject.SetActive(true);
            // Ensure parent is active too if it's the container
            if (emailErrorText.transform.parent != null)
                emailErrorText.transform.parent.gameObject.SetActive(true);
        }
    }

    private void ShowPasswordError(string message)
    {
        if (passwordErrorText != null)
        {
            passwordErrorText.text = message;
            passwordErrorText.gameObject.SetActive(true);
            // Ensure parent is active too if it's the container
            if (passwordErrorText.transform.parent != null)
                passwordErrorText.transform.parent.gameObject.SetActive(true);
        }
    }

    public void ClearFields()
    {
        ResolveReferences();

        if (usernameField != null)
        {
            usernameField.text = string.Empty;
        }

        if (passwordField != null)
        {
            passwordField.text = string.Empty;
        }

        ClearErrorTexts();
        SetStatus(string.Empty);
    }

    private void HandleSignInResult(FirebaseUser user, string error)
    {
        if (user == null)
        {
            Debug.LogWarning($"Sign-in failed: {error}");
            
            string lowerError = error?.ToLower() ?? "";
            
            // Check for specific error strings from FirebaseManager or the SDK
            if (lowerError.Contains("password") || lowerError.Contains("match"))
            {
                ShowPasswordError("Email and password doesn't match.");
            }
            else if (lowerError.Contains("not registered") || lowerError.Contains("user not found") || lowerError.Contains("no record") || lowerError.Contains("database"))
            {
                ShowEmailError("Email is not registered yet.");
            }
    else if (lowerError.Contains("network"))
            {
                SetStatus("Network error. Please check your connection.");
            }
            else if (lowerError.Contains("too many") || lowerError.Contains("attempts"))
            {
                SetStatus("Too many login attempts. Please try again later.");
            }
            else if (lowerError.Contains("disabled"))
            {
                ShowEmailError("This account has been disabled.");
            }
            else if (lowerError.Contains("badly formatted") || lowerError.Contains("invalid email"))
            {
                ShowEmailError("Invalid email address format.");
            }
            else
            {
                // General error status if no specific field error matches
                SetStatus(string.IsNullOrWhiteSpace(error) ? "Sign-in failed." : error);
            }

            HandleLoginFailure(null); 
            return;
        }

        // Check if email is verified
        if (!user.IsEmailVerified)
        {
            Debug.LogWarning($"Login failed: Email {user.Email} is not verified.");
            SetStatus("This account is not verified. Please check your email.");
            FirebaseManager.Instance.SignOut();
            HandleLoginFailure(null);
            return;
        }

        Debug.Log($"Sign-in successful. User ID: {user.UserId}. Loading Firestore document...");

        FirebaseManager.Instance.LoadUserDocument(user.UserId, (snapshot, loadError) =>
        {
            if (snapshot == null)
            {
                Debug.LogWarning($"Failed to load user document: {loadError}");
                HandleLoginFailure(string.IsNullOrWhiteSpace(loadError) ? "Failed to read user data." : loadError);
                return;
            }

            Debug.Log("Firestore document loaded successfully. Applying profile...");
            
            // Check Account Status
            string status = snapshot.ContainsField("status") ? snapshot.GetValue<string>("status") : "pending";
            if (status.ToLower() != "approved" && status.ToLower() != "approve")
            {
                string statusMsg = status.ToLower() == "pending" ? 
                    "Your account is still pending approval." : 
                    "Your account has been rejected or disabled.";
                
                HandleLoginFailure(statusMsg);
                FirebaseManager.Instance.SignOut();
                return;
            }

            ApplyPlayerProfile(user, snapshot);
            
            string normalizedRole = (PlayerData.Role ?? string.Empty).Trim().ToLowerInvariant();
            string targetScene = studentSceneName;
            if (normalizedRole == "teacher")
            {
                targetScene = teacherSceneName;
            }

            string resolvedTargetScene = ResolveSceneReference(targetScene);
            if (string.IsNullOrWhiteSpace(resolvedTargetScene))
            {
                // Fallback to known dashboard names in case inspector scene names were changed.
                resolvedTargetScene = ResolveSceneReference(normalizedRole == "teacher" ? "TeacherDashboard" : "StudentDashboard");
            }

            if (string.IsNullOrWhiteSpace(resolvedTargetScene))
            {
                HandleLoginFailure("Dashboard scene is missing from Build Settings.");
                return;
            }

            Debug.Log($"Navigating to dashboard: {resolvedTargetScene} (Role: {PlayerData.Role})");
            StartCoroutine(ShowSuccessAndLoad(resolvedTargetScene));
});
    }

    private void ApplyPlayerProfile(FirebaseUser user, DocumentSnapshot snapshot)
    {
        try
        {
            string name = snapshot.ContainsField("name") ? snapshot.GetValue<string>("name") : string.Empty;
            string username = snapshot.ContainsField("username") ? snapshot.GetValue<string>("username") : string.Empty;
            string role = snapshot.ContainsField("role") ? snapshot.GetValue<string>("role") : string.Empty;
            string email = !string.IsNullOrWhiteSpace(user.Email)
                ? user.Email
                : snapshot.ContainsField("email") ? snapshot.GetValue<string>("email") : string.Empty;
            
            string photoUrl = snapshot.ContainsField("photoUrl") ? snapshot.GetValue<string>("photoUrl") : string.Empty;

            List<string> completed = new List<string>();
            if (snapshot.ContainsField("completedQuestions"))
            {
                try
                {
                    completed = snapshot.GetValue<List<string>>("completedQuestions") ?? new List<string>();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Could not parse completedQuestions: {ex.Message}. Resetting to empty list.");
                }
            }

            PlayerData.SetProfile(user.UserId, name, username, email, role, photoUrl);
PlayerData.SetCorrectQuestions(completed);

            if (GameManager.Instance != null)
            {
                int stars = snapshot.ContainsField("points") ? System.Convert.ToInt32(snapshot.GetValue<object>("points")) : 0;
                int coins = snapshot.ContainsField("coins") ? System.Convert.ToInt32(snapshot.GetValue<object>("coins")) : 0;

                GameManager.Instance.stars = stars;
                GameManager.Instance.coins = coins;
                GameManager.Instance.SaveMeta();
            }
            }
            catch (System.Exception ex)
    {
            Debug.LogError($"Critical error in ApplyPlayerProfile: {ex.Message}");
        }
    }

    private void HandleLoginFailure(string message)
    {
        isLoggingIn = false;
        SetInputsInteractable(true);
        ToggleModal(false);
        
        if (usernameField != null) usernameField.interactable = true;
        if (passwordField != null) passwordField.interactable = true;
        
        if (!string.IsNullOrEmpty(message))
        {
            SetStatus(message);
        }
        Debug.Log($"Login failed UI reset.");
    }

    private void ResolveReferences()
    {
        if (usernameField == null)
        {
            GameObject usernameObject = GameObject.Find("UsernameField");
            if (usernameObject != null)
            {
                usernameField = usernameObject.GetComponent<TMP_InputField>();
            }
        }

        if (passwordField == null)
        {
            GameObject passwordObject = GameObject.Find("PasswordField");
            if (passwordObject != null)
            {
                passwordField = passwordObject.GetComponent<TMP_InputField>();
            }
        }

        if (statusText == null)
        {
            GameObject statusObject = GameObject.Find("LoginStatusText");
            if (statusObject != null)
            {
                statusText = statusObject.GetComponent<TMP_Text>();
            }
        }

        if (emailErrorText == null)
        {
            GameObject emailErrorGo = GameObject.Find("ErrorEmailHandling");
            if (emailErrorGo != null)
            {
                emailErrorText = emailErrorGo.GetComponentInChildren<TMP_Text>(true);
            }
        }

        if (passwordErrorText == null)
        {
            GameObject passErrorGo = GameObject.Find("ErrorPasswordHandling");
            if (passErrorGo != null)
            {
                passwordErrorText = passErrorGo.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private void SetStatus(string message)
    {
        ResolveReferences();
        UIManager.Instance.SetText(statusText, message, true);
    }

    private void SetInputsInteractable(bool interactable)
    {
        UIManager.Instance.SetInteractable(usernameField, interactable);
        UIManager.Instance.SetInteractable(passwordField, interactable);
    }

    private IEnumerator ShowSuccessAndLoad(string sceneName)
    {
        if (loadingUI != null)
        {
            loadingUI.SetActive(true);
            // Set the specific loading label if found
            var loadingTxt = loadingUI.transform.Find("LoadingTxt")?.GetComponent<TMP_Text>();
            if (loadingTxt != null) loadingTxt.text = "";
        }

        yield return new WaitForSeconds(successDelay);

        isLoggingIn = false;
        SceneManager.LoadScene(sceneName);
    }

    private static string ResolveSceneReference(string sceneName)
    {
        string normalizedName = NormalizeSceneName(sceneName);
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        if (Application.CanStreamedLevelBeLoaded(normalizedName))
        {
            return normalizedName;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                continue;
            }

            string buildSceneName = Path.GetFileNameWithoutExtension(scenePath);
            if (string.Equals(buildSceneName, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scenePath, sceneName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return scenePath;
            }
        }

        return null;
    }

    private static string NormalizeSceneName(string sceneName)
    {
        string trimmed = sceneName == null ? string.Empty : sceneName.Trim();
        if (trimmed.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(trimmed);
        }

        return trimmed;
    }

    private void ToggleModal(bool visible)
    {
        if (loadingUI != null)
        {
            loadingUI.SetActive(visible);
        }
    }
    }
