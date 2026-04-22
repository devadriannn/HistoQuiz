using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LoginPasswordToggle : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Button hidePasswordButton;
    [SerializeField] private Button showPasswordButton;

    private void Reset()
    {
        ResolveReferences();
        HidePassword();
    }

    private void Awake()
    {
        ResolveReferences();
        EnsureButtonObjectsAreActive();
        RegisterListeners();
        HidePassword();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        ResolveReferences();
    }

    public void ShowPassword()
    {
        SetPasswordVisible(true);
    }

    public void HidePassword()
    {
        SetPasswordVisible(false);
    }

    private void ResolveReferences()
    {
        if (passwordField == null)
        {
            GameObject passwordObject = GameObject.Find("PasswordField");
            if (passwordObject != null)
            {
                passwordField = passwordObject.GetComponent<TMP_InputField>();
            }
        }

        if (hidePasswordButton == null)
        {
            GameObject hideButtonObject = GameObject.Find("HidePasswordBtn");
            if (hideButtonObject != null)
            {
                hidePasswordButton = hideButtonObject.GetComponent<Button>();
            }
        }

        if (showPasswordButton == null)
        {
            GameObject showButtonObject = GameObject.Find("ShowPasswordBtn");
            if (showButtonObject != null)
            {
                showPasswordButton = showButtonObject.GetComponent<Button>();
            }
        }
    }

    private void RegisterListeners()
    {
        if (hidePasswordButton != null)
        {
            hidePasswordButton.onClick.RemoveListener(ShowPassword);
            hidePasswordButton.onClick.AddListener(ShowPassword);
        }

        if (showPasswordButton != null)
        {
            showPasswordButton.onClick.RemoveListener(HidePassword);
            showPasswordButton.onClick.AddListener(HidePassword);
        }
    }

    private void SetPasswordVisible(bool visible)
    {
        if (passwordField != null)
        {
            passwordField.contentType = visible
                ? TMP_InputField.ContentType.Standard
                : TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();
        }

        SetButtonVisible(showPasswordButton, visible);
        SetButtonVisible(hidePasswordButton, !visible);
    }

    private void EnsureButtonObjectsAreActive()
    {
        if (showPasswordButton != null && !showPasswordButton.gameObject.activeSelf)
        {
            showPasswordButton.gameObject.SetActive(true);
        }

        if (hidePasswordButton != null && !hidePasswordButton.gameObject.activeSelf)
        {
            hidePasswordButton.gameObject.SetActive(true);
        }
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button == null)
        {
            return;
        }

        Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.enabled = visible;
            graphic.raycastTarget = visible;
        }

        button.interactable = visible;
    }
}