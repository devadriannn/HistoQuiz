using UnityEngine;
using TMPro;
using System.Reflection;

[DefaultExecutionOrder(-100)]
public class MobileInputFieldFixer : MonoBehaviour
{
    [Header("Caret Settings")]
    public int caretWidth = 3;
    public Color caretColor = Color.black;
public bool forceCaretColor = false;

    [Header("Behavior Settings")]
    public bool disableSelectAllOnFocus = true;
    public bool showMobileInput = false; // shouldHideMobileInput = !showMobileInput

    private void Start()
    {
        FixAllInputFields();
    }

    public void FixAllInputFields()
    {
        TMP_InputField[] allFields = Resources.FindObjectsOfTypeAll<TMP_InputField>();
        foreach (var field in allFields)
        {
            // Only fix fields in the scene
            if (field.gameObject.scene.name == null) continue;

            FixField(field);
        }
    }

    private void FixField(TMP_InputField field)
    {
        // 1. Fix Caret
        field.caretWidth = caretWidth;
        if (forceCaretColor)
        {
            field.caretColor = caretColor;
        }
        else if (field.caretColor.a == 0)
        {
            field.caretColor = new Color(field.caretColor.r, field.caretColor.g, field.caretColor.b, 1f);
        }

        // 2. Fix Mobile Input Visibility
        // Setting this to false shows the native input field on top of the keyboard.
        // Setting this to true hides it and uses Unity's own caret.
        field.shouldHideMobileInput = !showMobileInput;

        // 3. Disable Select All on Focus
        if (disableSelectAllOnFocus)
        {
            // Try via reflection first
            FieldInfo fieldInfo = typeof(TMP_InputField).GetField("m_OnFocusSelectAll", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(field, false);
            }

            // Also add a listener to ensure it doesn't select all
            field.onFocusSelectAll = false; // Newer TMP versions have this
            
            field.onSelect.RemoveListener(OnFieldSelect);
            field.onSelect.AddListener(OnFieldSelect);
        }
    }

    private void OnFieldSelect(string value)
    {
        // Find which field was selected
        TMP_InputField field = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.GetComponent<TMP_InputField>();
        if (field != null)
        {
            // Delay the selection reset to the end of the frame to override default behavior
            StartCoroutine(ResetSelection(field));
        }
    }

    private System.Collections.IEnumerator ResetSelection(TMP_InputField field)
    {
        yield return null;
        field.selectionAnchorPosition = field.text.Length;
        field.selectionFocusPosition = field.text.Length;
    }
}
