using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class HeroStoryData
{
    public string heroKey; // e.g. "Aguinaldo", "Rizal", "Bonifacio", etc.
    public GameObject loadingPanel;
    public GameObject bg;
    public GameObject title;
    public List<GameObject> pages;
    [HideInInspector] public Button triggerButton; // The button in the gallery that triggers this story
}

public class HeroesStoryController : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private GameObject heroesLoadingPanel;
    [SerializeField] private GameObject loadingPrefab;

    [Header("Hero Story Collection")]
    [SerializeField] private List<HeroStoryData> heroStories = new List<HeroStoryData>();

    [Header("Navigation Buttons")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button prevBtn;
    [SerializeField] private Button exitBtn;

    [Header("Loading Duration")]
    [SerializeField] private float loadingDuration = 2.0f;

    private int currentPageIndex = 0;
    private GameObject currentLoadingInstance;
    private HeroStoryData currentHeroData;

    private void Start()
    {
        Debug.Log("[HeroesStory] Controller started on " + gameObject.name);
        
        // Initial setup for the story content (ensure they are hidden)
        if (heroesLoadingPanel != null) heroesLoadingPanel.SetActive(false);
        foreach (var story in heroStories)
        {
            if (story.loadingPanel != null) story.loadingPanel.SetActive(false);
        }
        
        SetNavButtonsActive(false);

        // Setup button listeners
        if (nextBtn != null) nextBtn.onClick.AddListener(OnNextClicked);
        if (prevBtn != null) prevBtn.onClick.AddListener(OnPrevClicked);
        if (exitBtn != null) exitBtn.onClick.AddListener(OnExitClicked);

        // Periodically find and assign the button listener for all heroes
        StartCoroutine(FindAndAssignButtonsRoutine());
    }

    private IEnumerator FindAndAssignButtonsRoutine()
    {
        while (true)
        {
            foreach (var story in heroStories)
            {
                // Check if we need to re-find the button or re-assign the listener
                GameObject btnObj = GameObject.Find("Hero_" + story.heroKey);
                
                if (btnObj == null)
                {
                    GameObject canvas = GameObject.Find("Canvas");
                    if (canvas != null)
                    {
                        Transform content = canvas.transform.Find("Heroes/Viewport/Content");
                        if (content != null)
                        {
                            Transform t = content.Find("Hero_" + story.heroKey);
                            if (t != null) btnObj = t.gameObject;
                        }
                    }
                }

                if (btnObj != null)
                {
                    Button btn = btnObj.GetComponent<Button>();
                    if (btn != null && story.triggerButton != btn)
                    {
                        story.triggerButton = btn;
                        string key = story.heroKey;
                        
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => StartStory(key));
                        Debug.Log("[HeroesStory] Assigned click listener to " + btnObj.name);
                    }
                }
            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    public void StartStory(string heroKey)
    {
        currentHeroData = heroStories.Find(s => s.heroKey == heroKey);
        if (currentHeroData == null)
        {
            Debug.LogError("[HeroesStory] No story found for hero: " + heroKey);
            return;
        }

        Debug.Log("[HeroesStory] Starting " + heroKey + " Story sequence...");
        
        StopAllCoroutines();
        StartCoroutine(FindAndAssignButtonsRoutine()); // Resume finding buttons
        StartCoroutine(LoadingSequence());
    }

    private IEnumerator LoadingSequence()
    {
        if (currentHeroData == null) yield break;

        // 1. Show Main Panel
        heroesLoadingPanel.SetActive(true);

        // 2. Hide all other hero panels, show the current one
        foreach (var story in heroStories)
        {
            if (story.loadingPanel != null) story.loadingPanel.SetActive(story == currentHeroData);
        }

        // 3. Hide hero content initially
        if (currentHeroData.bg != null) currentHeroData.bg.SetActive(false);
        if (currentHeroData.title != null) currentHeroData.title.SetActive(false);
        foreach (var page in currentHeroData.pages) if (page != null) page.SetActive(false);
        SetNavButtonsActive(false);

        // 4. Add loading spinner
        if (loadingPrefab != null && currentHeroData.loadingPanel != null)
        {
            if (currentLoadingInstance != null) Destroy(currentLoadingInstance);
            currentLoadingInstance = Instantiate(loadingPrefab, currentHeroData.loadingPanel.transform);
            
            RectTransform rt = currentLoadingInstance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.SetAsLastSibling();
            }
        }

        // 5. Wait
        yield return new WaitForSeconds(loadingDuration);

        // 6. Hide loading indicator but keep the panel background
        if (currentLoadingInstance != null)
        {
            Destroy(currentLoadingInstance);
        }

        // 7. Show actual story content
        if (currentHeroData.bg != null) currentHeroData.bg.SetActive(true);
        if (currentHeroData.title != null) currentHeroData.title.SetActive(true);
        if (exitBtn != null) exitBtn.gameObject.SetActive(true);
        
        // Reset to first page
        currentPageIndex = 0;
        UpdatePageVisibility();
    }

    private void OnNextClicked()
    {
        if (currentHeroData == null) return;
        if (currentPageIndex < currentHeroData.pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
        }
    }

    private void OnPrevClicked()
    {
        if (currentHeroData == null) return;
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageVisibility();
        }
    }

    private void UpdatePageVisibility()
    {
        if (currentHeroData == null) return;
        for (int i = 0; i < currentHeroData.pages.Count; i++)
        {
            if (currentHeroData.pages[i] != null)
            {
                currentHeroData.pages[i].SetActive(i == currentPageIndex);
            }
        }

        // Navigation visibility
        if (nextBtn != null) nextBtn.gameObject.SetActive(currentPageIndex < currentHeroData.pages.Count - 1);
        if (prevBtn != null) prevBtn.gameObject.SetActive(currentPageIndex > 0);
    }

    private void SetNavButtonsActive(bool active)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(active);
        if (prevBtn != null) prevBtn.gameObject.SetActive(active);
        if (exitBtn != null) exitBtn.gameObject.SetActive(active);
    }

    private void OnExitClicked()
    {
        Debug.Log("[HeroesStory] Exit clicked. Hiding loading panel.");
        if (heroesLoadingPanel != null) heroesLoadingPanel.SetActive(false);
        SetNavButtonsActive(false);
        
        if (currentHeroData != null && currentHeroData.loadingPanel != null)
            currentHeroData.loadingPanel.SetActive(false);
        
        currentHeroData = null;
    }
}