using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LessonSceneController : MonoBehaviour
{
    [Header("UI References - Lesson List")]
    public TMP_Text lessonListTitleText;
    public Button lessonListBtn;

    [Header("UI References - Lesson Panel")]
    public GameObject lessonPanel;
    public TMP_Text lessonPanelTitleText;
    public TMP_Text lessonPanelContentText;

    private string currentPDFUrl;
    private string analyzedTitle;
    private string analyzedFullContent;
    private List<string> analyzedTopics;

    private void Start()
    {
        Debug.Log("LessonSceneController: Start called.");
        
        if (lessonListBtn == null)
        {
            var btnObj = GameObject.Find("LessonListBtn");
            if (btnObj != null) lessonListBtn = btnObj.GetComponent<Button>() ?? btnObj.GetComponentInChildren<Button>(true);
        }

        if (lessonListTitleText == null)
        {
            var titleObj = GameObject.Find("Title");
            if (titleObj != null) lessonListTitleText = titleObj.GetComponent<TMP_Text>();
        }

        if (lessonPanel != null) lessonPanel.SetActive(false);
        
        if (lessonListBtn != null)
        {
            lessonListBtn.onClick.RemoveAllListeners();
            lessonListBtn.onClick.AddListener(OnLessonListBtnClick);
        }

        FetchLessonData();
    }

    private void FetchLessonData()
    {
        Debug.Log("LessonSceneController: Fetching latest lesson data...");
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager not initialized.");
            return;
        }

        FirebaseManager.Instance.InitializeFirebase((success, error) =>
        {
            if (!success)
            {
                Debug.LogError("Firebase initialization failed: " + error);
                return;
            }

            FirebaseManager.Instance.Firestore.Collection("lessons")
                .OrderByDescending("timestamp")
                .Limit(1)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("Failed to fetch lesson data: " + task.Exception);
                        return;
                    }

                    QuerySnapshot snapshot = task.Result;
                    if (snapshot != null && snapshot.Count > 0)
                    {
                        DocumentSnapshot latestDoc = snapshot.Documents.FirstOrDefault();
                        
                        analyzedTitle = latestDoc.ContainsField("title") ? latestDoc.GetValue<string>("title") : "Untitled Lesson";
                        currentPDFUrl = latestDoc.ContainsField("PDFUrl") ? latestDoc.GetValue<string>("PDFUrl") : "";
                        analyzedFullContent = latestDoc.ContainsField("fullContent") ? latestDoc.GetValue<string>("fullContent") : "";

                        if (latestDoc.ContainsField("topics"))
                        {
                            var topicsObj = latestDoc.GetValue<List<object>>("topics");
                            analyzedTopics = topicsObj.Select(o => o.ToString()).ToList();
                        }

                        Debug.Log("LessonSceneController: Loaded latest lesson: " + analyzedTitle);
                        UpdateListUI();
                    }
                    else
                    {
                        Debug.LogWarning("No lessons found in the 'lessons' collection.");
                        if (lessonListTitleText != null) lessonListTitleText.text = "No Lessons Available";
                    }
                });
        });
    }

    private void UpdateListUI()
    {
        if (lessonListTitleText != null) lessonListTitleText.text = analyzedTitle;
    }

    public void OnCloseLessonPanel()
    {
        if (lessonPanel != null) lessonPanel.SetActive(false);
    }

    public void OnExitLessonScene()
    {
        if (UnityEngine.Object.FindFirstObjectByType<SceneLoader>() != null)
        {
            UnityEngine.Object.FindFirstObjectByType<SceneLoader>().LoadScene("StudentDashboard");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("StudentDashboard");
        }
    }

    private void OnLessonListBtnClick()
    {
        if (string.IsNullOrEmpty(analyzedTitle))
        {
            if (lessonPanel != null)
            {
                lessonPanel.SetActive(true);
                if (lessonPanelTitleText != null) lessonPanelTitleText.text = "Loading...";
            }
            return;
        }

        if (lessonPanel != null) lessonPanel.SetActive(true);
        if (lessonPanelTitleText != null) lessonPanelTitleText.text = analyzedTitle;
        
        if (lessonPanelContentText != null)
        {
            if (!string.IsNullOrEmpty(analyzedFullContent))
            {
                lessonPanelContentText.text = analyzedFullContent;
            }
            else if (analyzedTopics != null)
            {
                lessonPanelContentText.text = "TOPICS COVERED:\n\n";
                foreach (var topic in analyzedTopics)
                {
                    lessonPanelContentText.text += "• " + topic + "\n";
                }
            }
        }

        var scrollRect = lessonPanel.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }
}