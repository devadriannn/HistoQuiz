using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UIPasswordToggle : MonoBehaviour
{
    [SerializeField] private List<TMP_InputField> passwordFields = new List<TMP_InputField>();
    [SerializeField] private Button hidePasswordButton;
    [SerializeField] private Button showPasswordButton;

    private void Awake()
    {
        if (hidePasswordButton != null)
        {
            hidePasswordButton.onClick.RemoveListener(ToggleVisible);
            hidePasswordButton.onClick.AddListener(ToggleVisible);
        }

        if (showPasswordButton != null)
        {
            showPasswordButton.onClick.RemoveListener(ToggleVisible);
            showPasswordButton.onClick.AddListener(ToggleVisible);
        }

        SetPasswordVisible(false);
    }

    public void ToggleVisible()
    {
        if (passwordFields.Count > 0 && passwordFields[0] != null)
        {
            SetPasswordVisible(passwordFields[0].contentType == TMP_InputField.ContentType.Password);
        }
    }

    private void SetPasswordVisible(bool visible)
    {
        foreach (var field in passwordFields)
        {
            if (field != null)
            {
                field.contentType = visible
                    ? TMP_InputField.ContentType.Standard
                    : TMP_InputField.ContentType.Password;
                field.ForceLabelUpdate();
            }
        }

        SetButtonVisible(showPasswordButton, visible);
        SetButtonVisible(hidePasswordButton, !visible);
    }

    private void SetButtonVisible(Button button, bool visible)
    {
        if (button == null) return;
        Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.enabled = visible;
            graphic.raycastTarget = visible;
        }
        button.interactable = visible;
    }
}