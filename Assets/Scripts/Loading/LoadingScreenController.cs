using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    public Slider progressBar;
    public TMP_Text loadingText;
    public TMP_Text tipText;
    public string nextScene = "Login";
    public float minimumLoadingDuration = 1.0f;
    public float completionHoldDuration = 0.2f;

    [SerializeField] private Image trackBackground;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image handleImage;
    [SerializeField] private float tipMinWidth = 560f;
    [SerializeField] private float tipWidthPadding = 180f;
    [SerializeField] private float tipHeight = 128f;
    [SerializeField] private float tipOffsetBelowProgress = 108f;
    [SerializeField] private float tipFontSizeMin = 36f;
    [SerializeField] private float tipFontSizeMax = 44f;
    [SerializeField] private Vector4 tipMargins = new Vector4(28f, 12f, 28f, 12f);

    private Sprite roundedSliderSprite;

    private readonly string[] tips =
    {
        "Did you know? Jose Rizal wrote Noli Me Tangere.",
        "Bonifacio founded the Katipunan.",
        "Apolinario Mabini was called the Brains of the Revolution.",
        "Lapu-Lapu defended Mactan against Magellan."
    };

    private void Awake()
    {
        ResolveReferences();
        ApplySliderStyle();
        SetProgress(0f);
    }

    private void Start()
    {
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        string targetScene = ResolveTargetSceneName();
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("LoadingScreenController could not determine the next scene.");
            yield break;
        }

        if (loadingText != null)
        {
            loadingText.text = "Loading...";
        }

        if (tipText != null)
        {
            tipText.text = "\"" + tips[UnityEngine.Random.Range(0, tips.Length)] + "\"";
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetScene);
        if (loadOperation == null)
        {
            Debug.LogError($"LoadingScreenController failed to start loading scene '{targetScene}'.");
            yield break;
        }

        loadOperation.allowSceneActivation = false;
        float elapsed = 0f;

        while (loadOperation.progress < 0.9f || elapsed < minimumLoadingDuration)
        {
            elapsed += Time.deltaTime;

            float sceneProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            float timeProgress = minimumLoadingDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / minimumLoadingDuration);
            SetProgress(Mathf.Min(sceneProgress, timeProgress));
            yield return null;
        }

        float displayedProgress = progressBar != null ? progressBar.value : 0f;
        while (displayedProgress < 1f)
        {
            displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime * 1.5f);
            SetProgress(displayedProgress);
            yield return null;
        }

        if (completionHoldDuration > 0f)
        {
            yield return new WaitForSeconds(completionHoldDuration);
        }

        loadOperation.allowSceneActivation = true;
    }

    private void ResolveReferences()
    {
        if (progressBar == null)
        {
            progressBar = GetComponent<Slider>();
        }

        if (progressBar == null)
        {
            Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (sliders.Length > 0)
            {
                progressBar = sliders[0];
            }
        }

        ResolveSliderGraphics();

        if (loadingText == null)
        {
            if (tipText != null && IsLoadingLabel(tipText))
            {
                loadingText = tipText;
            }
            else
            {
                TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (TMP_Text text in texts)
                {
                    if (IsLoadingLabel(text))
                    {
                        loadingText = text;
                        break;
                    }
                }
            }
        }

        if (tipText == null)
        {
            TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TMP_Text text in texts)
            {
                if (text != null && text != loadingText && string.Equals(text.gameObject.name, "QuoteText", StringComparison.Ordinal))
                {
                    tipText = text;
                    break;
                }
            }

            if (tipText == null)
            {
                tipText = CreateTipText();
            }
        }
        else if (tipText == loadingText)
        {
            tipText = CreateTipText();
        }

        if (tipText != null)
        {
            ConfigureTipText(tipText);
        }
    }

    private void ResolveSliderGraphics()
    {
        if (progressBar == null)
        {
            return;
        }

        if (trackBackground == null)
        {
            Transform background = progressBar.transform.Find("Background");
            if (background != null)
            {
                trackBackground = background.GetComponent<Image>();
            }
        }

        if (fillImage == null && progressBar.fillRect != null)
        {
            fillImage = progressBar.fillRect.GetComponent<Image>();
        }

        if (handleImage == null && progressBar.handleRect != null)
        {
            handleImage = progressBar.handleRect.GetComponent<Image>();
        }
    }

    private void ApplySliderStyle()
    {
        ResolveSliderGraphics();
        if (progressBar == null)
        {
            return;
        }

        Sprite sliderSprite = GetRoundedSliderSprite();

        if (trackBackground != null)
        {
            trackBackground.sprite = sliderSprite;
            trackBackground.type = Image.Type.Sliced;
            trackBackground.color = new Color(0.16f, 0.09f, 0.07f, 0.9f);
            trackBackground.raycastTarget = false;

            RectTransform trackRect = trackBackground.rectTransform;
            trackRect.anchorMin = new Vector2(0f, 0.2f);
            trackRect.anchorMax = new Vector2(1f, 0.8f);
            trackRect.anchoredPosition = Vector2.zero;
            trackRect.sizeDelta = Vector2.zero;
        }

        if (fillImage != null)
        {
            fillImage.sprite = sliderSprite;
            fillImage.type = Image.Type.Sliced;
            fillImage.color = new Color(0.78f, 0.2f, 0.14f, 1f);
            fillImage.raycastTarget = false;
        }

        if (handleImage != null)
        {
            handleImage.sprite = sliderSprite;
            handleImage.type = Image.Type.Sliced;
            handleImage.color = new Color(0.97f, 0.91f, 0.79f, 1f);
            handleImage.raycastTarget = false;
        }
    }

    private string ResolveTargetSceneName()
    {
        if (PlayerData.IsLoggedIn)
        {
            if (PlayerData.Role != null && PlayerData.Role.ToLower() == "teacher")
            {
                return "TeacherDashboard";
            }
            return "StudentDashboard";
        }

        string targetScene = string.IsNullOrWhiteSpace(nextScene) ? "Login" : nextScene.Trim();
        if (targetScene.EndsWith(".unity"))
        {
            targetScene = Path.GetFileNameWithoutExtension(targetScene);
        }

        return targetScene;
    }

    private void SetProgress(float value)
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = Mathf.Clamp01(value);
        }
    }

    private bool IsLoadingLabel(TMP_Text text)
    {
        if (text == null)
        {
            return false;
        }

        string content = (text.text ?? string.Empty).Trim();
        return string.Equals(text.gameObject.name, "Text (TMP)", StringComparison.Ordinal) ||
               string.Equals(content, "Loading...", StringComparison.OrdinalIgnoreCase);
    }

    private TMP_Text CreateTipText()
    {
        Canvas canvas = ResolveCanvas();
        if (canvas == null)
        {
            return null;
        }

        Transform existing = canvas.transform.Find("QuoteText");
        if (existing != null)
        {
            TMP_Text existingText = existing.GetComponent<TMP_Text>();
            if (existingText != null)
            {
                ConfigureTipText(existingText);
                return existingText;
            }
        }

        GameObject quoteObject = new GameObject("QuoteText", typeof(RectTransform));
        quoteObject.layer = canvas.gameObject.layer;
        RectTransform quoteRect = quoteObject.GetComponent<RectTransform>();
        quoteRect.SetParent(canvas.transform, false);

        TMP_Text quoteText = quoteObject.AddComponent<TextMeshProUGUI>();
        ConfigureTipText(quoteText);
        return quoteText;
    }

    private void ConfigureTipText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Min(tipFontSizeMin, tipFontSizeMax);
        text.fontSizeMax = Mathf.Max(tipFontSizeMin, tipFontSizeMax);
        text.fontSize = text.fontSizeMax;
        text.margin = tipMargins;

        if (loadingText != null)
        {
            text.font = loadingText.font;
            text.fontSharedMaterial = loadingText.fontSharedMaterial;
            text.color = loadingText.color;
        }
        else
        {
            text.color = new Color(0.97f, 0.9f, 0.76f, 1f);
        }

        RectTransform tipRect = text.rectTransform;
        tipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tipRect.pivot = new Vector2(0.5f, 0.5f);
        float tipWidth = tipMinWidth;
        Vector2 tipPosition = new Vector2(0f, -312f);
        if (progressBar != null)
        {
            RectTransform progressRect = progressBar.GetComponent<RectTransform>();
            float progressWidth = progressRect.rect.width > 0f ? progressRect.rect.width : progressRect.sizeDelta.x;
            tipWidth = Mathf.Max(tipWidth, progressWidth + tipWidthPadding);
            tipPosition = new Vector2(progressRect.anchoredPosition.x, progressRect.anchoredPosition.y - tipOffsetBelowProgress);
        }

        tipRect.anchoredPosition = tipPosition;
        tipRect.sizeDelta = new Vector2(tipWidth, tipHeight);
    }

    private Canvas ResolveCanvas()
    {
        if (progressBar != null)
        {
            Canvas sliderCanvas = progressBar.GetComponentInParent<Canvas>(true);
            if (sliderCanvas != null)
            {
                return sliderCanvas;
            }
        }

        if (loadingText != null && loadingText.canvas != null)
        {
            return loadingText.canvas;
        }

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return canvases.Length > 0 ? canvases[0] : null;
    }

    private Sprite GetRoundedSliderSprite()
    {
        if (roundedSliderSprite == null)
        {
            roundedSliderSprite = CreateRoundedSprite(128, 128, 40);
        }

        return roundedSliderSprite;
    }

    private Sprite CreateRoundedSprite(int width, int height, int radius)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "LoadingRoundedSliderTexture";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;

        Color32[] pixels = new Color32[width * height];
        Color32 solid = new Color32(255, 255, 255, 255);
        Color32 transparent = new Color32(255, 255, 255, 0);

        int maxX = width - 1;
        int maxY = height - 1;
        float cornerRadius = radius;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool insideHorizontal = x >= radius && x <= maxX - radius;
                bool insideVertical = y >= radius && y <= maxY - radius;
                bool inside = insideHorizontal || insideVertical;

                if (!inside)
                {
                    float centerX = x < radius ? radius : maxX - radius;
                    float centerY = y < radius ? radius : maxY - radius;
                    float deltaX = x - centerX;
                    float deltaY = y - centerY;
                    inside = (deltaX * deltaX) + (deltaY * deltaY) <= cornerRadius * cornerRadius;
                }

                pixels[(y * width) + x] = inside ? solid : transparent;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        sprite.name = "LoadingRoundedSliderSprite";
        return sprite;
    }
}

