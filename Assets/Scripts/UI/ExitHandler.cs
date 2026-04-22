using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles the mobile back button (Escape key) to toggle an exit confirmation modal.
/// </summary>
public class ExitHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject exitModal;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private void Start()
    {
        // Ensure modal is hidden at start
        if (exitModal != null)
            exitModal.SetActive(false);

        // Wire up buttons if they aren't already
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(QuitGame);
            yesButton.onClick.AddListener(QuitGame);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(CloseModal);
            noButton.onClick.AddListener(CloseModal);
        }
    }

    private void Update()
    {
        // Check for back button (Escape key in Unity handles Android back button)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleModal();
        }
    }

    public void ToggleModal()
    {
        if (exitModal != null)
        {
            exitModal.SetActive(!exitModal.activeSelf);
        }
    }

    public void CloseModal()
    {
        if (exitModal != null)
        {
            exitModal.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Application Quit triggered.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
