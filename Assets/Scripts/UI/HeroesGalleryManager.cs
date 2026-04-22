using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class HeroesGalleryManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject heroPrefab; // This will be the HeroesBtn template

    [Header("Hero Data")]
    [SerializeField] private List<Sprite> heroSprites = new List<Sprite>();

    [Header("Snap Settings")]
    [SerializeField] private float snapSpeed = 10f;
    
    private List<RectTransform> heroItems = new List<RectTransform>();
    private int currentIndex = 0;
    private bool isSnapping = false;
    private float targetHorizontalPos;

    private void Start()
    {
        if (heroSprites.Count > 0)
        {
            PopulateGallery();
        }
    }

    [ContextMenu("Populate Gallery")]
    public void PopulateGallery()
    {
        if (content == null || heroPrefab == null || scrollRect == null) return;

        // Clear existing children except the template
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Transform child = content.GetChild(i);
            if (child.gameObject != heroPrefab)
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }

        heroItems.Clear();

        if (heroSprites == null || heroSprites.Count == 0)
        {
            Debug.LogWarning("No hero sprites to populate.");
            return;
        }

        RectTransform templateRT = heroPrefab.GetComponent<RectTransform>();
        float itemWidth = templateRT.rect.width;

        foreach (var sprite in heroSprites)
        {
            GameObject item = Instantiate(heroPrefab, content);
            item.SetActive(true);
            item.name = "Hero_" + sprite.name;

            // Ensure correct size from template
            RectTransform rt = item.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = templateRT.sizeDelta;
                rt.localScale = Vector3.one;
                rt.anchoredPosition3D = Vector3.zero;
            }

            // Set Image
            Image img = item.GetComponent<Image>();
            if (img == null) img = item.GetComponentInChildren<Image>();
            if (img != null) 
            {
                img.sprite = sprite;
                img.color = Color.white;
            }

            // Set Text
            Transform txtObj = item.transform.Find("HeroesTxt");
            TMP_Text txt = txtObj != null ? txtObj.GetComponent<TMP_Text>() : item.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = sprite.name;
            }

            heroItems.Add(rt);
        }

        // Hide template
        heroPrefab.SetActive(false);

        // Adjust Padding to Center first and last items
        HorizontalLayoutGroup hlg = content.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            float viewportWidth = scrollRect.viewport.rect.width;
            if (viewportWidth <= 0) viewportWidth = 1080f; // Fallback
            
            int padding = Mathf.RoundToInt((viewportWidth - itemWidth) / 2f);
            hlg.padding.left = padding;
            hlg.padding.right = padding;
        }

        // Reset scroll position
        Canvas.ForceUpdateCanvases();
        scrollRect.horizontalNormalizedPosition = 0;
    }

    private void Update()
    {
        if (isSnapping && heroItems.Count > 1)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetHorizontalPos, Time.deltaTime * snapSpeed);
            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetHorizontalPos) < 0.0001f)
            {
                scrollRect.horizontalNormalizedPosition = targetHorizontalPos;
                isSnapping = false;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isSnapping = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SnapToNearest();
    }

    private void SnapToNearest()
    {
        if (heroItems.Count <= 1) return;

        float contentWidth = content.rect.width - scrollRect.viewport.rect.width;
        if (contentWidth <= 0) return;

        // Current scroll position in 0-1 range
        float currentPos = scrollRect.horizontalNormalizedPosition;
        
        // Find index
        float step = 1f / (heroItems.Count - 1);
        currentIndex = Mathf.RoundToInt(currentPos / step);
        currentIndex = Mathf.Clamp(currentIndex, 0, heroItems.Count - 1);

        targetHorizontalPos = currentIndex * step;
        isSnapping = true;
    }

    // Setters for the list (to be called from editor or automated script)
    public void SetHeroSprites(List<Sprite> sprites)
    {
        heroSprites = sprites;
    }
}
