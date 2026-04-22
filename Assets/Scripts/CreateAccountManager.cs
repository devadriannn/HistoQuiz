using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Auth;

public class CreateAccountManager : MonoBehaviour
{
    #region UI References

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField firstNameInput;
    [SerializeField] private TMP_InputField lastNameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;

    [Header("Profile Image")]
    [SerializeField] private Image profileImagePreview;
    [SerializeField] private Sprite defaultProfileSprite;

    [Header("Error Texts")]
    [SerializeField] private TMP_Text firstNameErrorText;
    [SerializeField] private TMP_Text lastNameErrorText;
    [SerializeField] private TMP_Text emailErrorText;
    [SerializeField] private TMP_Text passwordErrorText;
    [SerializeField] private TMP_Text confirmPasswordErrorText;
    [SerializeField] private TMP_Text generalStatusText;

    [Header("Button")]
    [SerializeField] private Button createAccountButton;
    [SerializeField] private GameObject loadingUI;

    [Header("Navigation")]
    [SerializeField] private PanelSwitcher panelSwitcher;
    [SerializeField] private GameObject successModal;
    [SerializeField] private TMP_Text successModalMessage;

    #endregion

    #region Validation State

    private bool isFirstNameValid = false;
    private bool isLastNameValid = false;
    private bool isEmailValid = false;
    private bool isPasswordValid = false;
    private bool isConfirmPasswordValid = false;
    private bool isProcessing = false;
    private string lastCheckedEmail = "";

    #endregion

    private void Start()
    {
        ClearAllErrors();
        SetDefaultProfileImage();
        UpdateCreateButtonState();

        // Listeners
        firstNameInput.onValueChanged.AddListener(_ => { ValidateFirstName(); UpdateCreateButtonState(); });
        lastNameInput.onValueChanged.AddListener(_ => { ValidateLastName(); UpdateCreateButtonState(); });
        emailInput.onValueChanged.AddListener(_ => { ValidateEmail(); UpdateCreateButtonState(); });
        passwordInput.onValueChanged.AddListener(_ => { ValidatePassword(); ValidateConfirmPassword(); UpdateCreateButtonState(); });
        confirmPasswordInput.onValueChanged.AddListener(_ => { ValidateConfirmPassword(); UpdateCreateButtonState(); });

        firstNameInput.onEndEdit.AddListener(val => firstNameInput.text = Capitalize(val));
        lastNameInput.onEndEdit.AddListener(val => lastNameInput.text = Capitalize(val));

        createAccountButton.onClick.RemoveListener(CreateAccount);
        createAccountButton.onClick.AddListener(CreateAccount);
    }

    #region VALIDATIONS

    private void ValidateFirstName()
    {
        string value = firstNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            firstNameErrorText.text = "First name is required.";
            isFirstNameValid = false;
            return;
        }

        if (value.Length < 2)
        {
            firstNameErrorText.text = "At least 2 characters.";
            isFirstNameValid = false;
            return;
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z'-]+$"))
        {
            firstNameErrorText.text = "Invalid first name.";
            isFirstNameValid = false;
            return;
        }

        firstNameErrorText.text = "";
        isFirstNameValid = true;
    }

    private void ValidateLastName()
    {
        string value = lastNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            lastNameErrorText.text = "Last name is required.";
            isLastNameValid = false;
            return;
        }

        if (value.Length < 2)
        {
            lastNameErrorText.text = "At least 2 characters.";
            isLastNameValid = false;
            return;
        }

        if (!Regex.IsMatch(value, @"^[a-zA-Z'-]+$"))
        {
            lastNameErrorText.text = "Invalid last name.";
            isLastNameValid = false;
            return;
        }

        lastNameErrorText.text = "";
        isLastNameValid = true;
    }

    private void ValidateEmail()
    {
        string email = emailInput.text.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            emailErrorText.text = "Email is required.";
            isEmailValid = false;
            lastCheckedEmail = "";
            return;
        }

        if (email.Contains(" "))
        {
            emailErrorText.text = "No spaces allowed.";
            isEmailValid = false;
            lastCheckedEmail = "";
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,7}$"))
        {
            emailErrorText.text = "Invalid email address.";
            isEmailValid = false;
            lastCheckedEmail = "";
            return;
        }

        CheckEmailAvailability(email.ToLower());
    }

    private void CheckEmailAvailability(string email)
    {
        if (email == lastCheckedEmail) return;
        lastCheckedEmail = email;

        isEmailValid = false;
        UpdateCreateButtonState();

        FirebaseManager.Instance.CheckEmailExists(email, (exists, error) =>
        {
            if (emailInput.text.Trim().ToLower() != email) return;

            if (exists)
            {
                emailErrorText.text = "This email is already in use.";
                isEmailValid = false;
            }
            else if (!string.IsNullOrEmpty(error))
            {
                emailErrorText.text = "Error: " + error; // Show the real error instead of generic message
                isEmailValid = false;
            }
            else
            {
                emailErrorText.text = "";
                isEmailValid = true;
            }
            UpdateCreateButtonState();
        });
    }

    private void ValidatePassword()
    {
        string pass = passwordInput.text;

        if (string.IsNullOrWhiteSpace(pass))
        {
            passwordErrorText.text = "Password is required.";
            isPasswordValid = false;
            return;
        }

        if (pass.Length < 8)
        {
            passwordErrorText.text = "Minimum 8 characters.";
            isPasswordValid = false;
            return;
        }

        if (!Regex.IsMatch(pass, @"[A-Z]"))
        {
            passwordErrorText.text = "Need 1 uppercase.";
            isPasswordValid = false;
            return;
        }

        if (!Regex.IsMatch(pass, @"[a-z]"))
        {
            passwordErrorText.text = "Need 1 lowercase.";
            isPasswordValid = false;
            return;
        }

        if (!Regex.IsMatch(pass, @"[0-9]"))
        {
            passwordErrorText.text = "Need 1 number.";
            isPasswordValid = false;
            return;
        }

        if (!Regex.IsMatch(pass, @"[\W_]"))
        {
            passwordErrorText.text = "Need 1 special char.";
            isPasswordValid = false;
            return;
        }

        passwordErrorText.text = "";
        isPasswordValid = true;
    }

    private void ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(confirmPasswordInput.text))
        {
            confirmPasswordErrorText.text = "Confirm your password.";
            isConfirmPasswordValid = false;
            return;
        }

        if (confirmPasswordInput.text != passwordInput.text)
        {
            confirmPasswordErrorText.text = "Passwords do not match.";
            isConfirmPasswordValid = false;
            return;
        }

        confirmPasswordErrorText.text = "";
        isConfirmPasswordValid = true;
    }

    #endregion

    #region HELPERS

    private string Capitalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        input = input.Trim().ToLower();
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    private void ClearAllErrors()
    {
        firstNameErrorText.text = "";
        lastNameErrorText.text = "";
        emailErrorText.text = "";
        passwordErrorText.text = "";
        confirmPasswordErrorText.text = "";
        generalStatusText.text = "";
    }

    private void UpdateCreateButtonState()
    {
        createAccountButton.interactable =
            !isProcessing &&
            isFirstNameValid &&
            isLastNameValid &&
            isEmailValid &&
            isPasswordValid &&
            isConfirmPasswordValid;
    }

    private void SetDefaultProfileImage()
    {
        if (profileImagePreview != null && profileImagePreview.sprite == null && defaultProfileSprite != null)
        {
            profileImagePreview.sprite = defaultProfileSprite;
        }
    }

    #endregion

    #region BUTTON

    public void CreateAccount()
    {
        OnClickCreateAccount();
    }

    private void OnClickCreateAccount()
    {
        ValidateFirstName();
        ValidateLastName();
        ValidateEmail();
        ValidatePassword();
        ValidateConfirmPassword();

        if (!createAccountButton.interactable)
        {
            generalStatusText.text = "Fix all errors first.";
            generalStatusText.color = Color.red;
            return;
        }

        string firstName = firstNameInput.text.Trim();
        string lastName = lastNameInput.text.Trim();
        string email = emailInput.text.Trim().ToLower();
        string password = passwordInput.text;
        
        // Use email prefix as default username
        string username = email.Split('@')[0];
        string role = "student"; 
        string photoUrl = ""; // No image upload implemented yet

        isProcessing = true;
        UpdateCreateButtonState();
        
        if (loadingUI != null)
        {
            loadingUI.SetActive(true);
            var loadingTxt = loadingUI.transform.Find("LoadingTxt")?.GetComponent<TMP_Text>();
            if (loadingTxt != null) loadingTxt.text = "Creating account...";
        }

        generalStatusText.text = "Creating account...";
        generalStatusText.color = Color.white;

        // Register student in Firebase
        FirebaseManager.Instance.RegisterStudent(firstName, lastName, role, username, email, password, photoUrl, (user, error) =>
        {
            isProcessing = false;
            UpdateCreateButtonState();

            if (loadingUI != null) loadingUI.SetActive(false);

            if (user != null)
            {
                StartCoroutine(HandleSuccessSignup());
            }
            else
            {
                generalStatusText.text = error ?? "Registration failed.";
                generalStatusText.color = Color.red;
            }
        });
    }

    private System.Collections.IEnumerator HandleSuccessSignup()
    {
        if (successModal != null)
        {
            successModal.SetActive(true);
            if (successModalMessage != null)
            {
                successModalMessage.text = "Please check your email to activate your account.";
            }
            if (generalStatusText != null) generalStatusText.text = "";
        }
        else
        {
            generalStatusText.text = "Please check your email to activate your account.";
            generalStatusText.color = Color.green;
        }

        yield return new WaitForSeconds(3.5f);

        if (successModal != null)
        {
            successModal.SetActive(false);
        }

        if (panelSwitcher != null)
        {
            panelSwitcher.ShowLogin();
        }
        else
        {
            Debug.LogWarning("PanelSwitcher is not assigned. Switching panels manually.");
            GameObject loginPanel = GameObject.Find("Canvas/Login");
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }

                #endregion
                }
