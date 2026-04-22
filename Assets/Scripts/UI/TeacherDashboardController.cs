using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeacherDashboardController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string loginSceneName = "Login";
    [SerializeField] private string quizzesSceneName = "Questions";
    [SerializeField] private string reportsSceneName = "Leaderboards";
    [SerializeField] private string settingsSceneName = "Settings";

    [Header("UI Containers (Sections)")]
    [SerializeField] public GameObject homeSection;
    [SerializeField] public GameObject quizzesSection;
    [SerializeField] public GameObject reportsSection;
    [SerializeField] public GameObject profileSection;
    [SerializeField] public GameObject multiplayerSection;

    [Header("Navigation Buttons")]
    [SerializeField] public Button homeButton;
    [SerializeField] public Button quizzesButton;
    [SerializeField] public Button reportsButton;
    [SerializeField] public Button profileButton;
    [SerializeField] public Button multiplayerButton;

    [Header("Profile Display")]
    [SerializeField] public TMP_Text profileNameText;
    [SerializeField] public TMP_Text profileEmailText;
    [SerializeField] public TMP_Text profileRoleText;
    [SerializeField] public Image profileAvatarImage;
    [SerializeField] public Sprite defaultProfileSprite;

    [Header("Multiplayer / Room Status")]
    [SerializeField] public TMP_Text roomStatusText;
    [SerializeField] public TMP_Text roomHintText;
    [SerializeField] public TMP_Text quizBankCountText;
    [SerializeField] public TMP_Text multiplayerQuizCountText;
    [SerializeField] public TMP_Text liveStudentCountText;
    [SerializeField] public Button createRoomButton;
    [SerializeField] public Button startQuizButton;
    [SerializeField] public RealtimeLeaderboard realtimeLeaderboard;

    [Header("Modals")]
    [SerializeField] public GameObject uploadLessonModal;

    [Header("Lesson Upload")]
    [SerializeField] public GameObject uploadDisplayGroup;
    [SerializeField] public TMP_Text pdfFilenameText;
    [SerializeField] public TMP_Text fileSizeText;
    [SerializeField] public Button browseFileButton;
    [SerializeField] public Button removeFileButton;
    [SerializeField] public Button uploadAndGenerateButton;
    [SerializeField] public Button cancelUploadButton;
    [SerializeField] public CloudinaryUploader cloudinaryUploader;

    private string currentSelectedFilePath;

    [Header("Countdown Settings")]
    [SerializeField] public GameObject countdownOverlay;
    [SerializeField] public TMP_Text countdownText;

    private string currentRoomCode;
    private Button activeButton;
    private Coroutine profileImageRoutine;

    private static readonly Color NavInactiveColor = new Color32(52, 29, 10, 248);
    private static readonly Color NavActiveColor = new Color32(92, 56, 22, 255);
    private static readonly Color NavLabelInactiveColor = new Color32(226, 208, 169, 255);
    private static readonly Color NavLabelActiveColor = new Color32(255, 244, 214, 255);

    private void Awake()
    {
        WireBottomNavButtons();
    }

    private void Start()
    {
        if (GetComponent<TeacherDashboardDataManager>() == null)
        {
            gameObject.AddComponent<TeacherDashboardDataManager>();
        }

        RefreshQuizMetrics();
        UpdateProfilePanel();
        
        OnHomeClick();
    }

    public void OnHomeClick() => ShowSection(homeSection, homeButton);
    public void OnQuizzesClick() { RefreshQuizMetrics(); ShowSection(quizzesSection, quizzesButton); }
    public void OnReportsClick() => ShowSection(reportsSection, reportsButton);
    public void OnProfileClick() { UpdateProfilePanel(); ShowSection(profileSection, profileButton); }
    public void OnUploadLessonClick() => uploadLessonModal?.SetActive(true);
    public void OnCloseUploadModal() => uploadLessonModal?.SetActive(false);

    public void OnBrowseFileClick()
    {
    #if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Lesson PDF", "", "pdf");
        if (!string.IsNullOrEmpty(path))
        {
            ProcessSelectedFile(path);
        }
    #else
        // For Android/Mobile, you would typically use a Native File Picker plugin here.
        // Example: NativeFilePicker.PickFile(ProcessSelectedFile, new string[] { "application/pdf" });
        Debug.Log("Native File Picker triggered (Placeholder for Mobile)");
    #endif
    }

    private void ProcessSelectedFile(string path)
    {
        currentSelectedFilePath = path;
        System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
        if (pdfFilenameText != null) pdfFilenameText.text = fileInfo.Name;
        if (fileSizeText != null) fileSizeText.text = FormatFileSize(fileInfo.Length);
        if (uploadDisplayGroup != null) uploadDisplayGroup.SetActive(true);
    }

    public void OnUploadAndGenerateClick()
    {
        if (string.IsNullOrEmpty(currentSelectedFilePath))
        {
            Debug.LogWarning("No file selected for upload.");
            return;
        }

        if (cloudinaryUploader == null)
        {
            cloudinaryUploader = UnityEngine.Object.FindFirstObjectByType<CloudinaryUploader>();
        }

        if (cloudinaryUploader == null)
        {
            Debug.LogError("CloudinaryUploader not found in scene.");
            return;
        }

        StartCoroutine(PerformUploadAndSave());
    }

    private IEnumerator PerformUploadAndSave()
    {
        byte[] fileBytes = System.IO.File.ReadAllBytes(currentSelectedFilePath);
        string fileName = System.IO.Path.GetFileName(currentSelectedFilePath);

        bool isUploading = true;
        string uploadedUrl = null;
        string uploadError = null;

        Debug.Log("TeacherDashboardController: Uploading file to Cloudinary...");
        StartCoroutine(cloudinaryUploader.UploadFile(fileBytes, fileName, "application/pdf", 
            (url) => { uploadedUrl = url; isUploading = false; },
            (err) => { uploadError = err; isUploading = false; }));

        while (isUploading) yield return null;

        if (!string.IsNullOrEmpty(uploadError))
        {
            Debug.LogError("Upload failed: " + uploadError);
            yield break;
        }

        Debug.Log("Uploaded to Cloudinary: " + uploadedUrl);

        // --- NEW: AI Analysis before saving to Firestore ---
        string cleanedFileName = System.IO.Path.GetFileNameWithoutExtension(fileName).Replace("_", " ").Replace("-", " ");
        string aiTitle = cleanedFileName;
        List<string> aiTopics = new List<string>();
        string aiFullContent = "";
        bool aiFinished = false;

        Debug.Log("TeacherDashboardController: Starting AI pre-analysis...");
        GroqAIService.Instance.AnalyzeLessonFromText("Lesson File: " + cleanedFileName, (title, topics, fullBody, error) => {
            if (string.IsNullOrEmpty(error)) {
                aiTitle = title;
                aiTopics = topics;
                aiFullContent = fullBody;
            }
            aiFinished = true;
        });

        yield return new WaitUntil(() => aiFinished);

        // --- Save to Firestore with a UNIQUE ID ---
        if (FirebaseManager.Instance != null)
        {
            var db = FirebaseManager.Instance.Firestore;
            // Use a unique ID based on time and teacher ID
            string uniqueDocId = "lesson_" + DateTime.Now.Ticks + "_" + PlayerData.UserId;
            var lessonRef = db.Collection("lessons").Document(uniqueDocId);

            Dictionary<string, object> lessonData = new Dictionary<string, object>
            {
                { "lessonId", uniqueDocId },
                { "PDFUrl", uploadedUrl },
                { "title", aiTitle },
                { "topics", aiTopics },
                { "fullContent", aiFullContent },
                { "teacherId", PlayerData.UserId },
                { "teacherName", PlayerData.Name },
                { "timestamp", Firebase.Firestore.FieldValue.ServerTimestamp }
            };

            var task = lessonRef.SetAsync(lessonData);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Firestore update failed: " + task.Exception);
            }
            else
            {
                Debug.Log("Firestore updated: New lesson saved as " + uniqueDocId);
                OnCloseUploadModal();
            }
        }
    }

    public void OnRemoveUploadedFile()
    {
        currentSelectedFilePath = null;
        if (pdfFilenameText != null) pdfFilenameText.text = "";
        if (fileSizeText != null) fileSizeText.text = "";
        if (uploadDisplayGroup != null) uploadDisplayGroup.SetActive(false);
    }

    private string FormatFileSize(long bytes)
    {
        if (bytes >= 1048576) return (bytes / 1048576f).ToString("F2") + " MB";
        return (bytes / 1024f).ToString("F1") + " KB";
    }

    public void OnMultiplayerClick() 
    { 
        RefreshQuizMetrics(); 
        if (liveStudentCountText != null) liveStudentCountText.text = "0";
        ShowSection(multiplayerSection, multiplayerButton); 
    }

    public void OnOpenQuestionsScene() { if (SceneExists(quizzesSceneName)) SceneManager.LoadScene(quizzesSceneName); }
    public void OnOpenReportsScene() { if (SceneExists(reportsSceneName)) SceneManager.LoadScene(reportsSceneName); }
    public void OnOpenSettingsScene() { if (SceneExists(settingsSceneName)) SceneManager.LoadScene(settingsSceneName); }

    public void OnLogoutClick()
    {
        if (FirebaseManager.Instance != null) FirebaseManager.Instance.SignOut();
        PlayerData.Clear();
        if (SettingsManager.Instance != null) SettingsManager.Instance.ResetToDefaults();
        PlayerPrefs.DeleteKey("FirebaseIdToken");
        PlayerPrefs.DeleteKey("FirebaseRefreshToken");
        PlayerPrefs.DeleteKey("FirebaseLocalId");
        PlayerPrefs.DeleteKey("FirebaseEmail");
        PlayerPrefs.Save();

        if (GameManager.Instance != null) { GameManager.Instance.GoToLogin(); return; }
        if (SceneExists(loginSceneName)) SceneManager.LoadScene(loginSceneName);
    }

    private void WireBottomNavButtons()
    {
        WireButton(homeButton, OnHomeClick);
        WireButton(quizzesButton, OnQuizzesClick);
        WireButton(reportsButton, OnReportsClick);
        WireButton(profileButton, OnProfileClick);
        WireButton(multiplayerButton, OnMultiplayerClick);
    }

    private void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void ShowSection(GameObject activeSection, Button selectedButton)
    {
        if (homeSection != null) homeSection.SetActive(homeSection == activeSection);
        if (quizzesSection != null) quizzesSection.SetActive(quizzesSection == activeSection);
        if (reportsSection != null) reportsSection.SetActive(reportsSection == activeSection);
        if (profileSection != null) profileSection.SetActive(profileSection == activeSection);
        if (multiplayerSection != null) multiplayerSection.SetActive(multiplayerSection == activeSection);

        SetActiveNavButton(selectedButton);
    }

    private void SetActiveNavButton(Button selectedButton)
    {
        activeButton = selectedButton;
        UpdateButtonVisuals(homeButton, selectedButton == homeButton);
        UpdateButtonVisuals(quizzesButton, selectedButton == quizzesButton);
        UpdateButtonVisuals(reportsButton, selectedButton == reportsButton);
        UpdateButtonVisuals(profileButton, selectedButton == profileButton);
        UpdateButtonVisuals(multiplayerButton, selectedButton == multiplayerButton);
    }

    private void UpdateButtonVisuals(Button button, bool isActive)
    {
        if (button == null) return;
        Image bg = button.targetGraphic as Image;
        if (bg != null) bg.color = isActive ? NavActiveColor : NavInactiveColor;

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.color = isActive ? NavLabelActiveColor : NavLabelInactiveColor;
        
        Image icon = FindButtonIcon(button);
        if (icon != null) icon.rectTransform.localScale = isActive ? new Vector3(1.06f, 1.06f, 1f) : Vector3.one;
    }

    private Image FindButtonIcon(Button button)
    {
        if (button == null) return null;
        Image[] images = button.GetComponentsInChildren<Image>(true);
        foreach (var img in images) if (img != button.targetGraphic) return img;
        return null;
    }

    public void RefreshQuizMetrics()
    {
        int questionCount = 0;
        QuestionsBank bank = UnityEngine.Object.FindFirstObjectByType<QuestionsBank>();
        if (bank != null && bank.questions != null) questionCount = bank.questions.Count;

        if (quizBankCountText != null) quizBankCountText.text = questionCount.ToString();
        if (multiplayerQuizCountText != null) multiplayerQuizCountText.text = questionCount.ToString();
    }

    public void UpdateProfilePanel()
    {
        if (profileNameText != null) profileNameText.text = string.IsNullOrWhiteSpace(PlayerData.Name) ? "Teacher" : PlayerData.Name.Trim();
        if (profileEmailText != null) profileEmailText.text = string.IsNullOrWhiteSpace(PlayerData.Email) ? "No email saved" : PlayerData.Email.Trim();
        if (profileRoleText != null)
        {
            string role = string.IsNullOrWhiteSpace(PlayerData.Role) ? "Teacher" : PlayerData.Role.Trim();
            profileRoleText.text = "Role: " + char.ToUpper(role[0]) + role.Substring(1);
        }

        if (profileAvatarImage == null) return;
        profileAvatarImage.sprite = defaultProfileSprite;
        if (profileImageRoutine != null) { StopCoroutine(profileImageRoutine); profileImageRoutine = null; }
        if (!string.IsNullOrWhiteSpace(PlayerData.PhotoUrl)) profileImageRoutine = StartCoroutine(LoadProfileImage(PlayerData.PhotoUrl.Trim()));
    }

    private IEnumerator LoadProfileImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) yield break;
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture == null || profileAvatarImage == null) yield break;
            profileAvatarImage.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    public void OnCreateRoomClicked()
    {
        if (roomStatusText != null) roomStatusText.text = "CREATING...";
        if (roomHintText != null) roomHintText.text = "Preparing room and loading quiz data...";

        QuestionsBank bank = UnityEngine.Object.FindFirstObjectByType<QuestionsBank>();
        List<QuestionsBank.Entry> quizQuestions = new List<QuestionsBank.Entry>();
        if (bank != null && bank.questions != null && bank.questions.Count > 0)
            quizQuestions = bank.questions.GetRange(0, Mathf.Min(bank.questions.Count, 5));

        if (FirebaseRoomManager.Instance == null)
        {
            if (roomStatusText != null) roomStatusText.text = "UNAVAILABLE";
            if (roomHintText != null) roomHintText.text = "Firebase room manager is missing.";
            return;
        }

        FirebaseRoomManager.Instance.CreateRoom("manual_quiz", (code, error) =>
        {
            if (!string.IsNullOrEmpty(error))
            {
                if (roomStatusText != null) roomStatusText.text = "ERROR";
                if (roomHintText != null) roomHintText.text = error;
                return;
            }

            currentRoomCode = code;
            if (roomStatusText != null) roomStatusText.text = code;
            if (roomHintText != null) roomHintText.text = "Share this code with your students.";
            if (liveStudentCountText != null) liveStudentCountText.text = "0";

            if (quizQuestions.Count > 0)
                FirebaseRoomManager.Instance.SetRoomQuiz(code, quizQuestions, (success, quizError) => { if (!success) Debug.LogError("Quiz push failed: " + quizError); });

            if (startQuizButton != null) startQuizButton.gameObject.SetActive(true);
            if (realtimeLeaderboard != null) realtimeLeaderboard.Init(code);
        });
    }

    public void OnStartQuizClicked()
    {
        if (string.IsNullOrWhiteSpace(currentRoomCode) || FirebaseRoomManager.Instance == null) return;
        
        StartCoroutine(StartQuizCountdownRoutine());
    }

    private IEnumerator StartQuizCountdownRoutine()
    {
        if (countdownOverlay != null) countdownOverlay.SetActive(true);
        
        for (int i = 5; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownOverlay != null) countdownOverlay.SetActive(false);

        FirebaseRoomManager.Instance.StartQuiz(currentRoomCode, (success, error) =>
        {
            if (success)
            {
                if (roomStatusText != null) roomStatusText.text = currentRoomCode;
                if (roomHintText != null) roomHintText.text = "Live quiz started!";
            }
            else if (roomHintText != null) roomHintText.text = error ?? "Failed to start quiz.";
        });
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName) return true;
        }
        return false;
    }
}