using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MobileKeyboardAdjuster : MonoBehaviour
{
    private RectTransform rectTransform;
    private ScrollRect scrollRect;
    private float originalBottom;
    private bool wasKeyboardVisible;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponent<ScrollRect>();
        originalBottom = rectTransform.offsetMin.y;
        
        // Disable scrolling initially
        scrollRect.vertical = false;
    }

    private void Update()
    {
        float keyboardHeight = GetKeyboardHeight();
        bool isKeyboardVisible = keyboardHeight > 0;

        if (isKeyboardVisible)
        {
            AdjustForKeyboard(keyboardHeight);
            scrollRect.vertical = true;
        }
        else if (wasKeyboardVisible && !isKeyboardVisible)
        {
            ResetUI();
            scrollRect.vertical = false;
            scrollRect.verticalNormalizedPosition = 1f;
        }

        wasKeyboardVisible = isKeyboardVisible;
    }

    private void AdjustForKeyboard(float keyboardHeight)
    {
        Vector2 offsetMin = rectTransform.offsetMin;
        offsetMin.y = keyboardHeight;
        rectTransform.offsetMin = offsetMin;
    }

    private void ResetUI()
    {
        Vector2 offsetMin = rectTransform.offsetMin;
        offsetMin.y = originalBottom;
        rectTransform.offsetMin = offsetMin;
    }

    private float GetKeyboardHeight()
    {
    #if UNITY_EDITOR
        return 0;
    #elif UNITY_ANDROID || UNITY_IOS
        float keyboardHeight = TouchScreenKeyboard.area.height;
        
        if (keyboardHeight <= 0) return 0;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return 0;
        
        float scale = canvas.scaleFactor;
        return keyboardHeight / scale;
    #else
        return 0;
    #endif
    }
}
