using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages mobile on-screen keyboard behavior for TMP Input Fields using the New Input System.
/// Handles tap-to-dismiss, persistence during dragging, and back-button dismissal.
/// </summary>
[DefaultExecutionOrder(-50)]
public class MobileKeyboardManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maximum movement distance in pixels to consider a touch as a 'tap' instead of a 'drag'")]
    public float tapThreshold = 20f;

    private TMP_InputField _activeInputField;
    private Vector2 _pressStartPosition;
    private bool _isPressing;

    private void Update()
    {
        HandleKeyboardPersistence();
        HandleBackButtonClick();
        HandlePointerInput();
    }

    /// <summary>
    /// Tracks which InputField currently has focus.
    /// </summary>
    private void HandleKeyboardPersistence()
    {
        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        
        if (selected != null)
        {
            TMP_InputField field = selected.GetComponentInParent<TMP_InputField>();
            if (field != null)
            {
                _activeInputField = field;
            }
        }
    }

    /// <summary>
    /// Detects taps outside of input fields to dismiss the keyboard using New Input System.
    /// Works for both touch and mouse (editor).
    /// </summary>
    private void HandlePointerInput()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return;

        bool pressStarted = pointer.press.wasPressedThisFrame;
        bool pressReleased = pointer.press.wasReleasedThisFrame;

        if (pressStarted)
        {
            _pressStartPosition = pointer.position.ReadValue();
            _isPressing = true;
        }
        else if (pressReleased && _isPressing)
        {
            Vector2 endPosition = pointer.position.ReadValue();
            float moveDistance = Vector2.Distance(_pressStartPosition, endPosition);

            // If the movement is less than the threshold, it's a tap
            if (moveDistance < tapThreshold)
            {
                CheckAndDismiss(endPosition);
            }
            
            _isPressing = false;
        }
    }

    /// <summary>
    /// Checks if the tap position is on an InputField. If not, deselects and hides keyboard.
    /// </summary>
    private void CheckAndDismiss(Vector2 screenPosition)
    {
        if (_activeInputField == null) return;

        if (!IsPointerOverInputField(screenPosition))
        {
            // User tapped outside of any InputField, so we deselect
            EventSystem.current.SetSelectedGameObject(null);
            _activeInputField = null;
        }
    }

    /// <summary>
    /// Uses raycasting to determine if the pointer is currently over a TMP_InputField.
    /// </summary>
    private bool IsPointerOverInputField(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Check if the hit object or any of its parents is an InputField
            if (result.gameObject.GetComponentInParent<TMP_InputField>() != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Dismisses the keyboard when the mobile Back button (Android) or Escape key is pressed.
    /// </summary>
    private void HandleBackButtonClick()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_activeInputField != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                _activeInputField = null;
            }
        }
    }

    /// <summary>
    /// Helper function for external scripts to check if an InputField is currently being touched.
    /// </summary>
    public bool IsTouchingInputField()
    {
        Pointer pointer = Pointer.current;
        if (pointer == null) return false;
        return IsPointerOverInputField(pointer.position.ReadValue());
    }
}
