using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class QuizManager : MonoBehaviour
{
    [Serializable]
    private sealed class OptionView
    {
        public RectTransform rect;
        public Button button;
        public Image background;
        public TMP_Text label;
    }

    [Serializable]
    private sealed class ActionView
    {
        public RectTransform rect;
        public Button button;
        public Image icon;
        public TMP_Text costText;
    }

    [Serializable]
    private sealed class ResultView
    {
        public GameObject root;
        public TMP_Text scoreText;
        public TMP_Text totalCoinsEarnedText;
        public GameObject filled1Star;
        public GameObject filled2Star;
        public GameObject filled3Star;
        public Button nextBtn;
        public Button mainMenuBtn;
    }

    [Serializable]
    private sealed class MainView
    {
        public Canvas canvas;
        public RectTransform root;
        public TMP_Text titleText;
        public TMP_Text coinText;
        public TMP_Text pointsText;
        public TMP_Text promptText;
        public Slider timerSlider;
        public TMP_Text countdownText;
        public Image questionImage;
        public Button pauseButton;
        public TMP_Text playerNameText;
        public TMP_Text playerPointsText;
        public Image playerPhoto;
        public List<OptionView> options = new List<OptionView>();
        public ActionView removeAction;
        public ActionView revealAction;
        public ActionView hintAction;
    }

    [SerializeField] private QuestionsBank questionBank;
    [SerializeField] private MainView mainView = new MainView();
    [SerializeField] private ResultView resultView = new ResultView();
    [SerializeField] private List<Sprite> correctModalSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> incorrectModalSprites = new List<Sprite>();
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string settingsSceneName = "SettingsScene";
    [SerializeField] private string fallbackMenuSceneName = "StudentDashboard";
    [SerializeField] private int startingCoins = 1000;
    [SerializeField] private int correctCoinReward = 100;
    [SerializeField] private int correctStarReward = 50;
    [SerializeField] private int removeCost = 50;
    [SerializeField] private int revealCost = 100;
    [SerializeField] private int hintCost = 100;

    private readonly Dictionary<Texture2D, Sprite> spriteCache = new Dictionary<Texture2D, Sprite>();

    private TMP_Text progressText;
    private GameObject correctModal;
    private GameObject incorrectModal;
    private TMP_Text correctText;
    private TMP_Text incorrectText;
    private CanvasGroup correctCanvasGroup;
    private CanvasGroup incorrectCanvasGroup;
    
    private Color originalCorrectColor;
    private Color originalIncorrectColor;
    private Vector3 originalCorrectScale;
    private Vector3 originalIncorrectScale;

    private Coroutine modalAnimationCoroutine;
    private Coroutine textAnimationCoroutine;
    private Coroutine pointsAnimationCoroutine;
    private Image correctModalImage;
    private Image incorrectModalImage;
    private TMP_Text correctFactText;
    private TMP_Text incorrectFactText;
    private GameObject pauseModal;
    private Button correctNextButton;
    private Button incorrectNextButton;
    private Button resumeButton;
    private Button quitButton;
    private Button settingsButton;
    private GameObject fullScreenModal;
    private Image fullScreenImage;
    private CanvasGroup fullScreenCanvasGroup;
    private Coroutine fullScreenCoroutine;
    private GameObject hintModalRoot;
    private TMP_Text hintModalText;
    private Button hintModalCloseButton;
    private RectTransform revealRect;
    private Vector3 revealStartPosition;
    
    private int currentRoundIndex;
    private int currentLevel = 1;
    private List<QuestionsBank.Entry> levelQuestions = new List<QuestionsBank.Entry>();
    private List<Vector2> originalMinAnchors = new List<Vector2>();
    private List<Vector2> originalMaxAnchors = new List<Vector2>();
    
    private int coins;
    private int correctAnswersInLevel;
    private int coinsEarnedInLevel;
    
    private bool questionLocked;
    private bool removeUsed;
    private bool revealUsed;
    private bool hintUsed;

    public bool quizFinished => currentRoundIndex >= levelQuestions.Count && !HasNextLevel();
    public int score => coins;
    public int currentIndex => currentRoundIndex;
    public List<QuestionsBank.Entry> questions => levelQuestions;

    private void Awake()
    {
        ResolveReferences();
        EnsureHintModal();
        BindButtons();
        HideAllModals();
    }

    private void Start()
    {
        if (PlayerData.Role != null && PlayerData.Role.ToLower() == "teacher")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TeacherDashboard");
            return;
        }

        InitializeQuiz();
    }

    public void OnOption0Pressed() => SubmitAnswer(0);
    public void OnOption1Pressed() => SubmitAnswer(1);
    public void OnOption2Pressed() => SubmitAnswer(2);
    public void OnOption3Pressed() => SubmitAnswer(3);

    public void OnImageClicked()
    {
        if (mainView.questionImage == null || mainView.questionImage.sprite == null) return;
        if (fullScreenModal == null) CreateFullScreenModal();
        if (fullScreenModal != null && fullScreenImage != null)
        {
            fullScreenImage.sprite = mainView.questionImage.sprite;
            fullScreenImage.preserveAspect = true;
            RectTransform rt = fullScreenImage.rectTransform;
            rt.sizeDelta = new Vector2(900f, 900f);
            if (fullScreenCoroutine != null) StopCoroutine(fullScreenCoroutine);
            fullScreenCoroutine = StartCoroutine(AnimatePopup());
        }
    }

    public void CloseFullScreen()
    {
        if (fullScreenModal != null && fullScreenModal.activeSelf)
        {
            if (fullScreenCoroutine != null) StopCoroutine(fullScreenCoroutine);
            fullScreenCoroutine = StartCoroutine(AnimatePopdown());
        }
    }

    private IEnumerator AnimatePopup()
    {
        fullScreenModal.SetActive(true);
        if (fullScreenCanvasGroup != null) fullScreenCanvasGroup.alpha = 0f;
        if (fullScreenImage != null) fullScreenImage.transform.localScale = Vector3.one * 0.75f;
        float duration = 0.35f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float scaleT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            if (fullScreenCanvasGroup != null) fullScreenCanvasGroup.alpha = t;
            if (fullScreenImage != null) fullScreenImage.transform.localScale = Vector3.one * Mathf.Lerp(0.75f, 1f, scaleT);
            yield return null;
        }
        if (fullScreenCanvasGroup != null) fullScreenCanvasGroup.alpha = 1f;
        if (fullScreenImage != null) fullScreenImage.transform.localScale = Vector3.one;
        fullScreenCoroutine = null;
    }

    private IEnumerator AnimatePopdown()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        float startAlpha = fullScreenCanvasGroup != null ? fullScreenCanvasGroup.alpha : 1f;
        Vector3 startScale = fullScreenImage != null ? fullScreenImage.transform.localScale : Vector3.one;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (fullScreenCanvasGroup != null) fullScreenCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            if (fullScreenImage != null) fullScreenImage.transform.localScale = Vector3.Lerp(startScale, Vector3.one * 0.85f, t);
            yield return null;
        }
        fullScreenModal.SetActive(false);
        fullScreenCoroutine = null;
    }

    public void OnSettingsPressed()
    {
        string sceneName = string.IsNullOrWhiteSpace(settingsSceneName) ? "Settings" : settingsSceneName;
        SettingsUI.isAdditive = true;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void OnHintPressed()
    {
        if (coins < hintCost) { ShowStatusMessage("Not enough coins for a hint!"); return; }
        if (!TrySpendCoins(hintCost, ref hintUsed)) return;
        QuestionsBank.Entry entry = GetCurrentEntry();
        string message = entry == null || string.IsNullOrWhiteSpace(entry.hintText) ? "Look closely at the question." : entry.hintText.Trim();
        if (hintModalText == null || hintModalRoot == null) return;
        hintModalText.text = message;
        hintModalRoot.SetActive(true);
        UpdateUi();
    }

    public void OnRemovePressed()
    {
        QuestionsBank.Entry entry = GetCurrentEntry();
        if (entry == null || questionLocked) return;
        int incorrectActiveCount = 0;
        for (int i = 0; i < mainView.options.Count; i++)
        {
            if (i != entry.correctIndex && mainView.options[i].button != null && mainView.options[i].button.gameObject.activeSelf) incorrectActiveCount++;
        }
        if (incorrectActiveCount == 0) return;
        if (coins < removeCost) { ShowStatusMessage("Not enough coins!"); return; }
        if (GameManager.Instance != null) { if (!GameManager.Instance.SpendCoin(removeCost)) return; coins = GameManager.Instance.coins; }
        else { coins -= removeCost; }
        for (int i = 0; i < mainView.options.Count; i++)
        {
            if (i == entry.correctIndex) continue;
            Button button = mainView.options[i].button;
            if (button != null && button.gameObject.activeSelf) { button.gameObject.SetActive(false); break; }
        }
        if (incorrectActiveCount <= 1) removeUsed = true;
        UpdateUi();
    }

    public void OnRevealPressed()
    {
        if (!TrySpendCoins(revealCost, ref revealUsed)) return;
        StopCoroutine(nameof(AnimateReveal));
        StartCoroutine(nameof(AnimateReveal));
        UpdateUi();
    }

    public void OnPausePressed()
    {
        if (pauseModal != null) { pauseModal.SetActive(true); Time.timeScale = 0f; }
    }

    public void OnResumePressed()
    {
        if (pauseModal != null) pauseModal.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        string targetScene = string.IsNullOrWhiteSpace(fallbackMenuSceneName) ? "StudentDashboard" : fallbackMenuSceneName;
        if (loader != null) { loader.LoadScene(targetScene); return; }
        SceneManager.LoadScene(targetScene);
    }

    public void OnNextPressed()
    {
        HideAllModals();
        currentRoundIndex++;
        if (currentRoundIndex >= Mathf.Max(1, levelQuestions.Count))
        {
            ShowResult();
            return;
        }
        LoadQuestion(currentRoundIndex);
    }

    private void ShowResult()
    {
        if (resultView.root == null) return;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelCompletedSound();
        resultView.root.SetActive(true);
if (resultView.scoreText != null) resultView.scoreText.text = $"{correctAnswersInLevel}/{levelQuestions.Count}";
        if (resultView.totalCoinsEarnedText != null) resultView.totalCoinsEarnedText.text = coinsEarnedInLevel.ToString();
        
        float percentage = (float)correctAnswersInLevel / levelQuestions.Count;
        if (resultView.filled1Star != null) resultView.filled1Star.SetActive(percentage >= 0.33f);
        if (resultView.filled2Star != null) resultView.filled2Star.SetActive(percentage >= 0.66f);
        if (resultView.filled3Star != null) resultView.filled3Star.SetActive(percentage >= 0.90f);

        if (resultView.nextBtn != null)
        {
            TMP_Text btnText = resultView.nextBtn.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = HasNextLevel() ? "NEXT LEVEL" : "PLAY AGAIN";
        }
    }

    private void OnResultNextPressed()
    {
        if (resultView.root != null) resultView.root.SetActive(false);
        if (HasNextLevel())
        {
            currentLevel++;
            LoadLevel(currentLevel);
        }
        else
        {
            LoadLevel(currentLevel);
        }
    }

    private void InitializeQuiz()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) coins = GameManager.Instance.coins;
        else coins = Mathf.Max(0, startingCoins);
        currentLevel = 1;
        LoadPlayerProfile();
        LoadLevel(currentLevel);
    }

    private void LoadLevel(int level)
    {
        currentRoundIndex = 0;
        correctAnswersInLevel = 0;
        coinsEarnedInLevel = 0;
        levelQuestions.Clear();
        if (questionBank != null && questionBank.questions != null)
        {
            foreach (var q in questionBank.questions) { if (q.level == level) levelQuestions.Add(q); }
        }
        if (levelQuestions.Count == 0 && level > 1) { ShowResult(); return; }
        if (resultView.root != null) resultView.root.SetActive(false);
        LoadQuestion(currentRoundIndex);
    }

    private bool HasNextLevel()
    {
        if (questionBank == null || questionBank.questions == null) return false;
        foreach (var q in questionBank.questions) { if (q.level > currentLevel) return true; }
        return false;
    }

    private void LoadQuestion(int roundIndex)
    {
        QuestionsBank.Entry entry = GetEntryForRound(roundIndex);
        if (entry == null) return;
        questionLocked = false;
        removeUsed = false;
        revealUsed = false;
        hintUsed = false;
        HideAllModals();
        CloseHintModal();
        ResetOptionButtons();
        ResetRevealIcon();
        bool hasImage = entry.imageTexture != null;
        if (mainView.questionImage != null)
        {
            Sprite s = GetSprite(entry.imageTexture);
            mainView.questionImage.sprite = s;
            mainView.questionImage.gameObject.SetActive(hasImage);
            if (s != null) { mainView.questionImage.preserveAspect = true; mainView.questionImage.rectTransform.sizeDelta = new Vector2(543f, 543f); }
        }
        if (mainView.promptText != null)
        {
            mainView.promptText.text = entry.prompt ?? string.Empty;
            bool isLandmark = entry.level == 1 && entry.imageTexture != null;
            mainView.promptText.gameObject.SetActive(!isLandmark && (!hasImage || !string.IsNullOrEmpty(entry.prompt)));
        }
        int activeOptionsCount = 0;
        foreach (var opt in entry.options) if (!string.IsNullOrWhiteSpace(opt)) activeOptionsCount++;
        bool isTrueFalse = activeOptionsCount == 2;
        for (int i = 0; i < mainView.options.Count; i++)
        {
            OptionView option = mainView.options[i];
            if (option.button != null && option.rect != null)
            {
                if (i < originalMinAnchors.Count) { option.rect.anchorMin = originalMinAnchors[i]; option.rect.anchorMax = originalMaxAnchors[i]; }
                if (isTrueFalse)
                {
                    if (i == 0 || i == 2)
                    {
                        option.button.gameObject.SetActive(true);
                        if (option.label != null) option.label.text = i == 0 ? entry.options[0] : entry.options[1];
                        Vector2 min = option.rect.anchorMin; Vector2 max = option.rect.anchorMax;
                        float height = max.y - min.y; min.y = 0.5f - (height / 2f); max.y = 0.5f + (height / 2f);
                        option.rect.anchorMin = min; option.rect.anchorMax = max; option.rect.anchoredPosition = Vector2.zero;
                    }
                    else option.button.gameObject.SetActive(false);
                }
                else
                {
                    bool hasValue = i < entry.options.Count && !string.IsNullOrWhiteSpace(entry.options[i]);
                    option.button.gameObject.SetActive(hasValue);
                    if (hasValue && option.label != null) option.label.text = entry.options[i];
                }
                option.button.interactable = true;
            }
        }
        UpdateUi();
    }

    public bool SubmitAnswer(int optionIndex)
    {
        if (questionLocked) return false;
        QuestionsBank.Entry entry = GetCurrentEntry();
        if (entry == null) return false;
        questionLocked = true;
        SetOptionInteractable(false);
        CloseHintModal();
        bool isCorrect = optionIndex == entry.correctIndex;
        if (isCorrect)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrectAnswerSound();
            correctAnswersInLevel++;
            coinsEarnedInLevel += correctCoinReward;
if (GameManager.Instance != null) { GameManager.Instance.AddCoin(correctCoinReward); coins = GameManager.Instance.coins; }
            else coins += Mathf.Max(0, correctCoinReward);
            bool firstTime = !PlayerData.HasAnsweredCorrectly(entry.questionId);
            if (firstTime)
            {
                PlayerData.MarkAsCorrectlyAnswered(entry.questionId);
                if (GameManager.Instance != null)
                {
                    int oldStars = GameManager.Instance.stars; GameManager.Instance.AddStars(correctStarReward); int newStars = GameManager.Instance.stars;
                    if (FirebaseManager.Instance != null) { FirebaseManager.Instance.UpdateScore(newStars); FirebaseManager.Instance.MarkQuestionCompleted(entry.questionId); }
                    if (mainView.playerPointsText != null) { if (pointsAnimationCoroutine != null) StopCoroutine(pointsAnimationCoroutine); pointsAnimationCoroutine = StartCoroutine(AnimatePoints(oldStars, newStars)); }
                }
            }
            if (correctFactText != null) correctFactText.text = entry.factText ?? string.Empty;
            ShowModal(correctModal);
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWrongAnswerSound();
            if (incorrectFactText != null) incorrectFactText.text = entry.factText ?? string.Empty;
            ShowModal(incorrectModal);
        }
UpdateUi();
        return isCorrect;
    }

    private IEnumerator AnimatePoints(int from, int to)
    {
        float duration = 0.8f; float elapsed = 0f;
        while (elapsed < duration) { elapsed += Time.unscaledDeltaTime; float t = elapsed / duration; int current = (int)Mathf.Lerp(from, to, t); if (mainView.playerPointsText != null) mainView.playerPointsText.text = current.ToString(); yield return null; }
        if (mainView.playerPointsText != null) mainView.playerPointsText.text = to.ToString();
        pointsAnimationCoroutine = null;
    }

    private bool TrySpendCoins(int cost, ref bool alreadyUsed)
    {
        if (questionLocked || alreadyUsed) return false;
        if (coins < cost) return false;
        coins -= Mathf.Max(0, cost);
        alreadyUsed = true;
        return true;
    }

    private IEnumerator AnimateReveal()
    {
        if (revealRect == null) yield break;
        QuestionsBank.Entry entry = GetCurrentEntry();
        if (entry == null || entry.correctIndex < 0 || entry.correctIndex >= mainView.options.Count) yield break;
        Button correctButton = mainView.options[entry.correctIndex].button;
        if (correctButton == null) yield break;
        Vector3 start = revealStartPosition; Vector3 end = correctButton.transform.position;
        float duration = 0.35f; float elapsed = 0f;
        while (elapsed < duration) { elapsed += Time.deltaTime; float t = Mathf.Clamp01(elapsed / duration); revealRect.position = Vector3.Lerp(start, end, t); yield return null; }
        yield return new WaitForSeconds(0.35f); revealRect.position = revealStartPosition;
    }

    private void ResetOptionButtons()
    {
        for (int i = 0; i < mainView.options.Count; i++) { if (mainView.options[i].button != null) { mainView.options[i].button.gameObject.SetActive(true); mainView.options[i].button.interactable = true; } }
    }

    private void ResetRevealIcon() { if (revealRect != null) revealRect.position = revealStartPosition; }

    private void SetOptionInteractable(bool interactable)
    {
        for (int i = 0; i < mainView.options.Count; i++) { if (mainView.options[i].button != null && mainView.options[i].button.gameObject.activeInHierarchy) mainView.options[i].button.interactable = interactable; }
    }

    private void UpdateUi()
    {
        if (mainView.coinText != null) mainView.coinText.text = coins.ToString();
        if (mainView.playerPointsText != null && pointsAnimationCoroutine == null) { if (GameManager.Instance != null) mainView.playerPointsText.text = GameManager.Instance.stars.ToString(); }
        if (progressText != null) progressText.text = $"item {currentRoundIndex + 1}/{Mathf.Max(1, levelQuestions.Count)}";
        if (mainView.titleText != null) mainView.titleText.text = $"Level {currentLevel}";
        if (mainView.timerSlider != null) { mainView.timerSlider.minValue = 1f; mainView.timerSlider.maxValue = Mathf.Max(1, levelQuestions.Count); mainView.timerSlider.wholeNumbers = true; mainView.timerSlider.value = Mathf.Clamp(currentRoundIndex + 1, 1, Mathf.Max(1, levelQuestions.Count)); }
        UpdateActionButtons();
    }

    private void UpdateActionButtons()
    {
        int incorrectActiveCount = 0;
        QuestionsBank.Entry entry = GetCurrentEntry();
        if (entry != null) { for (int i = 0; i < mainView.options.Count; i++) { if (i != entry.correctIndex && mainView.options[i].button != null && mainView.options[i].button.gameObject.activeSelf) incorrectActiveCount++; } }
        UpdateActionButton(mainView.removeAction, !questionLocked && incorrectActiveCount > 0 && coins >= removeCost);
        UpdateActionButton(mainView.revealAction, !questionLocked && !revealUsed && coins >= revealCost);
        UpdateActionButton(mainView.hintAction, !questionLocked && !hintUsed && coins >= hintCost);
    }

    private static void UpdateActionButton(ActionView actionView, bool interactable)
    {
        if (actionView == null || actionView.button == null) return;
        actionView.button.interactable = interactable;
        if (actionView.icon != null) { Color color = actionView.icon.color; color.a = interactable ? 1f : 0.45f; actionView.icon.color = color; }
    }

    private QuestionsBank.Entry GetCurrentEntry() => GetEntryForRound(currentRoundIndex);

    private QuestionsBank.Entry GetEntryForRound(int roundIndex)
    {
        if (levelQuestions == null || levelQuestions.Count == 0) return null;
        int safeIndex = Mathf.Abs(roundIndex) % levelQuestions.Count;
        return levelQuestions[safeIndex];
    }

    private Sprite GetSprite(Texture2D texture)
    {
        if (texture == null) return null;
        if (!spriteCache.TryGetValue(texture, out Sprite sprite) || sprite == null) { sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)); spriteCache[texture] = sprite; }
        return sprite;
    }

    private TMP_Text FindProgressText()
    {
        if (mainView.root == null) return null;
        TMP_Text[] texts = mainView.root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++) { if (!string.IsNullOrWhiteSpace(texts[i].text) && texts[i].text.IndexOf("item ", StringComparison.OrdinalIgnoreCase) >= 0) return texts[i]; }
        return null;
    }

    private void EnsureHintModal()
    {
        if (mainView.canvas == null) return;
        if (hintModalRoot != null) { hintModalRoot.SetActive(false); return; }
        TMP_FontAsset fontAsset = null;
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (texts.Length > 0) fontAsset = texts[0].font;
        GameObject overlay = new GameObject("HintModal", typeof(RectTransform), typeof(Image)); overlay.transform.SetParent(mainView.canvas.transform, false);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>(); overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = Vector2.zero; overlayRect.offsetMax = Vector2.zero;
        Image overlayImage = overlay.GetComponent<Image>(); overlayImage.color = new Color(0f, 0f, 0f, 0.72f);
        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(overlay.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>(); panelRect.anchorMin = new Vector2(0.5f, 0.5f); panelRect.anchorMax = new Vector2(0.5f, 0.5f); panelRect.pivot = new Vector2(0.5f, 0.5f); panelRect.sizeDelta = new Vector2(760f, 420f);
        Image panelImage = panel.GetComponent<Image>(); panelImage.color = new Color(0.95f, 0.88f, 0.73f, 1f);
        GameObject titleObject = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI)); titleObject.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>(); titleRect.anchorMin = new Vector2(0.5f, 1f); titleRect.anchorMax = new Vector2(0.5f, 1f); titleRect.pivot = new Vector2(0.5f, 1f); titleRect.anchoredPosition = new Vector2(0f, -40f); titleRect.sizeDelta = new Vector2(620f, 72f);
        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>(); title.font = fontAsset; title.fontSize = 42f; title.fontStyle = FontStyles.Bold; title.alignment = TextAlignmentOptions.Center; title.color = new Color(0.25f, 0.16f, 0.05f, 1f); title.text = "Hint";
        GameObject bodyObject = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI)); bodyObject.transform.SetParent(panel.transform, false);
        RectTransform bodyRect = bodyObject.GetComponent<RectTransform>(); bodyRect.anchorMin = new Vector2(0.5f, 0.5f); bodyRect.anchorMax = new Vector2(0.5f, 0.5f); bodyRect.pivot = new Vector2(0.5f, 0.5f); bodyRect.anchoredPosition = new Vector2(0f, 10f); bodyRect.sizeDelta = new Vector2(620f, 200f);
        TextMeshProUGUI body = bodyObject.GetComponent<TextMeshProUGUI>(); body.font = fontAsset; body.fontSize = 30f; body.alignment = TextAlignmentOptions.Center; body.color = new Color(0.22f, 0.14f, 0.05f, 1f); body.textWrappingMode = TextWrappingModes.Normal; body.text = string.Empty; hintModalText = body;
        GameObject closeObject = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button)); closeObject.transform.SetParent(panel.transform, false);
        RectTransform closeRect = closeObject.GetComponent<RectTransform>(); closeRect.anchorMin = new Vector2(0.5f, 0f); closeRect.anchorMax = new Vector2(0.5f, 0f); closeRect.pivot = new Vector2(0.5f, 0f); closeRect.anchoredPosition = new Vector2(0f, 36f); closeRect.sizeDelta = new Vector2(260f, 90f);
        Image closeImage = closeObject.GetComponent<Image>(); closeImage.color = new Color(0.42f, 0.24f, 0.08f, 1f); hintModalCloseButton = closeObject.GetComponent<Button>();
        GameObject closeLabelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); closeLabelObject.transform.SetParent(closeObject.transform, false);
        RectTransform closeLabelRect = closeLabelObject.GetComponent<RectTransform>(); closeLabelRect.anchorMin = Vector2.zero; closeLabelRect.anchorMax = Vector2.one; closeLabelRect.offsetMin = Vector2.zero; closeLabelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI closeLabel = closeLabelObject.GetComponent<TextMeshProUGUI>(); closeLabel.font = fontAsset; closeLabel.fontSize = 34f; closeLabel.fontStyle = FontStyles.Bold; closeLabel.alignment = TextAlignmentOptions.Center; closeLabel.color = Color.white; closeLabel.text = "Close";
        hintModalRoot = overlay; hintModalRoot.SetActive(false);
    }

    private void CreateFullScreenModal()
    {
        if (mainView.canvas == null) return;
        GameObject overlay = new GameObject("FullScreenImageModal", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup)); overlay.transform.SetParent(mainView.canvas.transform, false);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>(); overlayRect.anchorMin = Vector2.zero; overlayRect.anchorMax = Vector2.one; overlayRect.offsetMin = Vector2.zero; overlayRect.offsetMax = Vector2.zero;
        Image overlayImage = overlay.GetComponent<Image>(); overlayImage.color = new Color(0f, 0f, 0f, 0.85f);
        Button overlayButton = overlay.GetComponent<Button>(); overlayButton.onClick.AddListener(CloseFullScreen);
        fullScreenCanvasGroup = overlay.GetComponent<CanvasGroup>();
        GameObject imageObject = new GameObject("FullScreenImage", typeof(RectTransform), typeof(Image)); imageObject.transform.SetParent(overlay.transform, false);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>(); imageRect.anchorMin = new Vector2(0.5f, 0.5f); imageRect.anchorMax = new Vector2(0.5f, 0.5f); imageRect.pivot = new Vector2(0.5f, 0.5f); imageRect.anchoredPosition = Vector2.zero;
        fullScreenImage = imageObject.GetComponent<Image>(); fullScreenImage.preserveAspect = true; fullScreenImage.raycastTarget = false;
        fullScreenModal = overlay; fullScreenModal.SetActive(false);
    }

    private void CloseHintModal() { if (hintModalRoot != null) hintModalRoot.SetActive(false); }

    private void HideAllModals()
    {
        CloseHintModal(); CloseFullScreen();
        if (modalAnimationCoroutine != null) StopCoroutine(modalAnimationCoroutine);
        if (textAnimationCoroutine != null) StopCoroutine(textAnimationCoroutine);
        modalAnimationCoroutine = null; textAnimationCoroutine = null;
        if (correctText != null) { correctText.color = originalCorrectColor; correctText.transform.localScale = originalCorrectScale; correctText.transform.localRotation = Quaternion.identity; }
        if (incorrectText != null) { incorrectText.color = originalIncorrectColor; incorrectText.transform.localScale = originalIncorrectScale; incorrectText.transform.localRotation = Quaternion.identity; }
        if (correctCanvasGroup != null) correctCanvasGroup.alpha = 1f;
        if (incorrectCanvasGroup != null) incorrectCanvasGroup.alpha = 1f;
        if (correctModal != null) { Transform panel = FindChild(correctModal.transform, "Panel"); if (panel != null) panel.localScale = Vector3.one; correctModal.SetActive(false); }
        if (incorrectModal != null) { Transform panel = FindChild(incorrectModal.transform, "Panel"); if (panel != null) panel.localScale = Vector3.one; incorrectModal.SetActive(false); }
        if (pauseModal != null) pauseModal.SetActive(false);
    }

    private void ShowModal(GameObject modal)
    {
        HideAllModals(); if (modal == null) return;
        bool isCorrect = modal == correctModal;
        if (isCorrect) { if (correctModalImage != null && correctModalSprites != null && correctModalSprites.Count > 0) correctModalImage.sprite = correctModalSprites[UnityEngine.Random.Range(0, correctModalSprites.Count)]; }
        else { if (incorrectModalImage != null && incorrectModalSprites != null && incorrectModalSprites.Count > 0) incorrectModalImage.sprite = incorrectModalSprites[UnityEngine.Random.Range(0, incorrectModalSprites.Count)]; }
        modal.SetActive(true);
        CanvasGroup cg = isCorrect ? correctCanvasGroup : incorrectCanvasGroup; TMP_Text text = isCorrect ? correctText : incorrectText;
        if (cg != null) modalAnimationCoroutine = StartCoroutine(AnimateModalIn(modal, cg));
        if (text != null) textAnimationCoroutine = StartCoroutine(AnimateTextModern(text));
    }

    private IEnumerator AnimateModalIn(GameObject modal, CanvasGroup cg)
    {
        Transform panel = FindChild(modal.transform, "Panel"); Transform target = panel != null ? panel : modal.transform;
        cg.alpha = 0f; target.localScale = Vector3.one * 0.8f;
        float duration = 0.4f; float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; float t = Mathf.Clamp01(elapsed / duration);
            float c1 = 1.70158f; float c3 = c1 + 1f; float scaleT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            cg.alpha = t; target.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, scaleT); yield return null;
        }
        cg.alpha = 1f; target.localScale = Vector3.one;
    }

    private IEnumerator AnimateTextModern(TMP_Text text)
    {
        Vector3 baseScale = text == correctText ? originalCorrectScale : originalIncorrectScale;
        Color baseColor = text == correctText ? originalCorrectColor : originalIncorrectColor;
        if (baseScale.sqrMagnitude < 0.01f) baseScale = Vector3.one;
        if (baseColor.a < 0.01f && baseColor.r < 0.01f && baseColor.g < 0.01f) baseColor = text == correctText ? Color.green : Color.red;
        float h, s, v; Color.RGBToHSV(baseColor, out h, out s, out v); Color accentColor = Color.HSVToRGB(h, s * 0.6f, Mathf.Min(v + 0.3f, 1f));
        float elapsed = 0f;
        while (true)
        {
            elapsed += Time.unscaledDeltaTime; float sin = Mathf.Sin(elapsed * 8f); float t = (sin + 1f) / 2f;
            text.transform.localScale = baseScale * (1f + (sin * 0.08f));
            Color lerpedColor = Color.Lerp(baseColor, accentColor, t); lerpedColor.a = Mathf.Lerp(0.6f, 1f, t); text.color = lerpedColor;
            text.transform.localRotation = Quaternion.Euler(0, 0, sin * 3f);
            yield return null;
        }
    }

    private static Transform FindChild(Transform root, string name)
    {
        if (root == null) return null;
        string bracketedName = $"[{name}]";
        foreach (Transform child in root) { if (string.Equals(child.name, name, StringComparison.Ordinal) || string.Equals(child.name, bracketedName, StringComparison.Ordinal)) return child; Transform nested = FindChild(child, name); if (nested != null) return nested; }
        return null;
    }

    private static GameObject FindChildGameObject(Transform root, string name) { Transform found = FindChild(root, name); return found != null ? found.gameObject : null; }
    private static Button FindChildButton(Transform root, string name) { Transform found = FindChild(root, name); return found != null ? found.GetComponent<Button>() : null; }

    private void ShowStatusMessage(string message) { if (mainView.promptText != null) { StopCoroutine("TemporaryPromptChange"); StartCoroutine("TemporaryPromptChange", message); } else Debug.Log(message); }

    private IEnumerator TemporaryPromptChange(string message)
    {
        string original = mainView.promptText.text; Color originalColor = mainView.promptText.color;
        mainView.promptText.text = message; mainView.promptText.color = Color.yellow;
        yield return new WaitForSeconds(2f);
        mainView.promptText.text = original; mainView.promptText.color = originalColor;
    }

    private void ResolveReferences()
    {
        if (questionBank == null) { QuestionsBank[] banks = FindObjectsByType<QuestionsBank>(FindObjectsInactive.Include, FindObjectsSortMode.None); if (banks.Length > 0) questionBank = banks[0]; }
        if (mainView.canvas == null) { Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None); if (canvases.Length > 0) mainView.canvas = canvases[0]; }
        if (mainView.root == null && mainView.canvas != null) { Transform q = FindChild(mainView.canvas.transform, "Questions"); mainView.root = q != null ? q.GetComponent<RectTransform>() : mainView.canvas.GetComponent<RectTransform>(); }
        if (mainView.options == null) mainView.options = new List<OptionView>();
        progressText ??= FindProgressText();
        correctModal ??= FindChildGameObject(mainView.canvas.transform, "CorrectModal");
        incorrectModal ??= FindChildGameObject(mainView.canvas.transform, "IncorrectModal");
        if (correctModal != null)
        {
            correctCanvasGroup = correctModal.GetComponent<CanvasGroup>(); if (correctCanvasGroup == null) correctCanvasGroup = correctModal.AddComponent<CanvasGroup>();
            Transform textObj = FindChild(correctModal.transform, "Correct"); if (textObj != null) { correctText = textObj.GetComponent<TMP_Text>(); if (correctText != null) { Color c = correctText.color; c.a = 1f; originalCorrectColor = c; originalCorrectScale = Vector3.one; } }
        }
        if (incorrectModal != null)
        {
            incorrectCanvasGroup = incorrectModal.GetComponent<CanvasGroup>(); if (incorrectCanvasGroup == null) incorrectCanvasGroup = incorrectModal.AddComponent<CanvasGroup>();
            Transform textObj = FindChild(incorrectModal.transform, "Incorrect"); if (textObj != null) { incorrectText = textObj.GetComponent<TMP_Text>(); if (incorrectText != null) { Color c = incorrectText.color; c.a = 1f; originalIncorrectColor = c; originalIncorrectScale = Vector3.one; } }
        }
        correctNextButton ??= FindChildButton(correctModal != null ? correctModal.transform : null, "NextBtn");
        incorrectNextButton ??= FindChildButton(incorrectModal != null ? incorrectModal.transform : null, "NextBtn");
        if (correctModal != null) { Transform panel = FindChild(correctModal.transform, "Panel"); if (panel != null) { Transform imgObj = panel.Find("Image"); if (imgObj != null) correctModalImage = imgObj.GetComponent<Image>(); Transform factObj = panel.Find("Fact"); if (factObj == null) factObj = panel.Find("Description"); if (factObj != null) correctFactText = factObj.GetComponent<TMP_Text>(); } }
        if (incorrectModal != null) { Transform panel = FindChild(incorrectModal.transform, "Panel"); if (panel != null) { Transform imgObj = panel.Find("Image"); if (imgObj != null) incorrectModalImage = imgObj.GetComponent<Image>(); Transform factObj = panel.Find("Fact"); if (factObj == null) factObj = panel.Find("Description"); if (factObj != null) incorrectFactText = factObj.GetComponent<TMP_Text>(); } }
        pauseModal ??= FindChildGameObject(mainView.canvas.transform, "PauseModal"); if (pauseModal != null) { resumeButton ??= FindChildButton(pauseModal.transform, "ResumeBtn"); quitButton ??= FindChildButton(pauseModal.transform, "QuitBtn"); settingsButton ??= FindChildButton(pauseModal.transform, "SettingsBtn"); }
        if ((mainView.options == null || mainView.options.Count == 0) && mainView.root != null) AutoResolveOptionButtons();
        if (originalMinAnchors.Count == 0 && mainView.options != null) { foreach (var opt in mainView.options) { if (opt.rect != null) { originalMinAnchors.Add(opt.rect.anchorMin); originalMaxAnchors.Add(opt.rect.anchorMax); } } }
        if (mainView.root != null) { Transform panel = FindChild(mainView.root, "Image&TextPanel"); if (panel != null) { Transform img = panel.Find("GuessLandmarkImage"); if (img != null) mainView.questionImage = img.GetComponent<Image>(); Transform txt = panel.Find("TextQuestion"); if (txt != null) mainView.promptText = txt.GetComponent<TMP_Text>(); } }
        if (mainView.pauseButton == null && mainView.root != null) mainView.pauseButton = FindChildButton(mainView.root, "PauseBtn");
        if (mainView.playerNameText == null && mainView.root != null) { Transform profile = FindChild(mainView.root, "Profile"); if (profile != null) { Transform nameObj = FindChild(profile, "Name"); if (nameObj != null) mainView.playerNameText = nameObj.GetComponent<TMP_Text>(); } }
        if (mainView.playerPointsText == null && mainView.root != null) { Transform profile = FindChild(mainView.root, "Profile"); if (profile != null) { Transform star = FindChild(profile, "Star"); if (star != null) { Transform pointsObj = FindChild(star, "Points"); if (pointsObj != null) mainView.playerPointsText = pointsObj.GetComponent<TMP_Text>(); } } }
        if (mainView.playerPhoto == null && mainView.root != null) { Transform profile = FindChild(mainView.root, "Profile"); if (profile != null) { Transform photoObj = profile.Find("Photo"); if (photoObj != null) mainView.playerPhoto = photoObj.GetComponent<Image>(); } }
        if (mainView.promptText == null && mainView.root != null) { foreach (Transform child in mainView.root) { TMP_Text t = child.GetComponent<TMP_Text>(); if (t != null) { string content = t.text.ToLower(); if (!content.Contains("level") && !content.Contains("item")) { mainView.promptText = t; break; } } } }
        if (mainView.titleText == null && mainView.root != null) { TMP_Text[] texts = mainView.root.GetComponentsInChildren<TMP_Text>(true); foreach (var t in texts) { if (t.text.StartsWith("Level", StringComparison.OrdinalIgnoreCase)) { mainView.titleText = t; break; } } }
        if (mainView.coinText == null && mainView.root != null) { Transform coinTransform = FindChild(mainView.root, "Coin"); if (coinTransform != null) mainView.coinText = coinTransform.GetComponentInChildren<TMP_Text>(true); }
        if (mainView.timerSlider == null && mainView.root != null) { Transform progressTransform = FindChild(mainView.root, "ProgressBar"); if (progressTransform != null) mainView.timerSlider = progressTransform.GetComponent<Slider>(); }
        if (mainView.removeAction == null) mainView.removeAction = new ActionView(); if (mainView.revealAction == null) mainView.revealAction = new ActionView(); if (mainView.hintAction == null) mainView.hintAction = new ActionView();
        ResolveActionView(mainView.removeAction, "Remove"); ResolveActionView(mainView.revealAction, "Answer"); ResolveActionView(mainView.hintAction, "Hint");
        if (revealRect == null && mainView.revealAction != null && mainView.revealAction.icon != null) revealRect = mainView.revealAction.icon.rectTransform;
        if (revealRect == null && mainView.revealAction != null) revealRect = mainView.revealAction.rect;
        if (revealRect != null) revealStartPosition = revealRect.position;
        if (resultView.root == null && mainView.canvas != null)
        {
            Transform res = mainView.canvas.transform.Find("Result");
            if (res != null) { resultView.root = res.gameObject; resultView.scoreText = FindChild(res, "ScoreTxt")?.GetComponent<TMP_Text>(); resultView.totalCoinsEarnedText = FindChild(res, "TotalCoinsEarned")?.GetComponent<TMP_Text>(); Transform panel = res.Find("Panel"); if (panel != null) { resultView.filled1Star = panel.Find("Filled1Star")?.gameObject; resultView.filled2Star = panel.Find("Filled2Star")?.gameObject; resultView.filled3Star = panel.Find("Filled3Star")?.gameObject; } resultView.nextBtn = res.Find("NextBtn")?.GetComponent<Button>(); resultView.mainMenuBtn = res.Find("NextBtn (1)")?.GetComponent<Button>(); }
        }
    }

    private void BindButtons()
    {
        if (mainView.pauseButton != null && mainView.pauseButton.onClick.GetPersistentEventCount() == 0) mainView.pauseButton.onClick.AddListener(OnPausePressed);
        if (mainView.questionImage != null) { Button imgBtn = mainView.questionImage.GetComponent<Button>(); if (imgBtn != null && imgBtn.onClick.GetPersistentEventCount() == 0) imgBtn.onClick.AddListener(OnImageClicked); }
        if (correctNextButton != null && correctNextButton.onClick.GetPersistentEventCount() == 0) correctNextButton.onClick.AddListener(OnNextPressed);
        if (incorrectNextButton != null && incorrectNextButton.onClick.GetPersistentEventCount() == 0) incorrectNextButton.onClick.AddListener(OnNextPressed);
        if (resumeButton != null && resumeButton.onClick.GetPersistentEventCount() == 0) resumeButton.onClick.AddListener(OnResumePressed);
        if (quitButton != null && quitButton.onClick.GetPersistentEventCount() == 0) quitButton.onClick.AddListener(OnQuitPressed);
        if (settingsButton != null && settingsButton.onClick.GetPersistentEventCount() == 0) settingsButton.onClick.AddListener(OnSettingsPressed);
        if (hintModalCloseButton != null && hintModalCloseButton.onClick.GetPersistentEventCount() == 0) hintModalCloseButton.onClick.AddListener(CloseHintModal);
        if (resultView.nextBtn != null) resultView.nextBtn.onClick.AddListener(OnResultNextPressed);
        if (resultView.mainMenuBtn != null) resultView.mainMenuBtn.onClick.AddListener(OnQuitPressed);
    }

    private void AutoResolveOptionButtons()
    {
        Transform buttonPanel = FindChild(mainView.root, "ChoicesPanel"); if (buttonPanel == null) buttonPanel = FindChild(mainView.root, "ButtonPanel"); if (buttonPanel == null) return;
        Button[] buttons = buttonPanel.GetComponentsInChildren<Button>(true); Array.Sort(buttons, (left, right) => string.Compare(left.name, right.name, StringComparison.Ordinal));
        mainView.options.Clear();
        for (int i = 0; i < buttons.Length; i++) { if (mainView.options.Count >= 4) break; Button button = buttons[i]; TMP_Text label = button.GetComponentInChildren<TMP_Text>(true); Image background = button.GetComponent<Image>(); OptionView option = new OptionView { rect = button.GetComponent<RectTransform>(), button = button, background = background, label = label }; mainView.options.Add(option); }
    }

    private void ResolveActionView(ActionView actionView, string objectName)
    {
        if (actionView == null || mainView.root == null) return;
        if (actionView.rect == null) { Transform actionTransform = FindChild(mainView.root, objectName); if (actionTransform != null) { actionView.rect = actionTransform as RectTransform; actionView.button = actionTransform.GetComponent<Button>(); Image[] images = actionTransform.GetComponentsInChildren<Image>(true); if (images.Length > 0) actionView.icon = images[0]; } }
    }

    private void LoadPlayerProfile()
    {
        if (mainView.playerNameText != null) mainView.playerNameText.text = !string.IsNullOrWhiteSpace(PlayerData.Name) ? PlayerData.Name : "Guest";
        var firebase = FirebaseManager.Instance;
        if (firebase != null && firebase.Auth != null && firebase.Auth.CurrentUser != null)
        {
            firebase.LoadUserDocument(firebase.Auth.CurrentUser.UserId, (snapshot, error) =>
            {
                if (snapshot != null && snapshot.Exists)
                {
                    string name = snapshot.ContainsField("name") ? snapshot.GetValue<string>("name") : "Guest";
                    string username = snapshot.ContainsField("username") ? snapshot.GetValue<string>("username") : string.Empty;
                    string email = snapshot.ContainsField("email") ? snapshot.GetValue<string>("email") : firebase.Auth.CurrentUser.Email;
                    string role = snapshot.ContainsField("role") ? snapshot.GetValue<string>("role") : "student";
                    string photoUrl = snapshot.ContainsField("photoUrl") ? snapshot.GetValue<string>("photoUrl") : string.Empty;
                    List<string> completed = new List<string>();
                    if (snapshot.ContainsField("completedQuestions")) completed = snapshot.GetValue<List<string>>("completedQuestions") ?? new List<string>();
                    PlayerData.SetProfile(firebase.Auth.CurrentUser.UserId, name, username, email, role, photoUrl);
PlayerData.SetCorrectQuestions(completed);
                    if (mainView.playerNameText != null) mainView.playerNameText.text = name;
                    if (mainView.playerPhoto != null && !string.IsNullOrEmpty(photoUrl))
                    {
                        ProfileImageLoader imgLoader = mainView.playerPhoto.GetComponent<ProfileImageLoader>();
                        if (imgLoader == null) imgLoader = mainView.playerPhoto.gameObject.AddComponent<ProfileImageLoader>();
                        imgLoader.LoadProfileImage(photoUrl);
                    }
                }
            });
        }
        if (mainView.playerPointsText != null) mainView.playerPointsText.text = GameManager.Instance != null ? GameManager.Instance.stars.ToString() : "0";
    }
}
