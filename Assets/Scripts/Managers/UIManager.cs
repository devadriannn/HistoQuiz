using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UIManager : MonoBehaviour
{
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UIManager>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("UIManager");
                    instance = managerObject.AddComponent<UIManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(TMP_Text target, string message, bool show = true)
    {
        if (target == null)
        {
            return;
        }

        target.text = message ?? string.Empty;
        if (show)
        {
            target.gameObject.SetActive(true);
            target.alpha = 1f;
        }
    }

    public void SetText(TMP_Text target, string message, Color color, bool show = true)
    {
        if (target == null)
        {
            return;
        }

        target.color = color;
        SetText(target, message, show);
    }

    public void SetInteractable(Selectable target, bool interactable)
    {
        if (target != null)
        {
            target.interactable = interactable;
        }
    }

    public void SetActive(GameObject target, bool visible)
    {
        if (target != null)
        {
            target.SetActive(visible);
        }
    }

    public void Show(CanvasGroup target, bool visible)
    {
        if (target == null)
        {
            return;
        }

        target.alpha = visible ? 1f : 0f;
        target.interactable = visible;
        target.blocksRaycasts = visible;
    }
}
