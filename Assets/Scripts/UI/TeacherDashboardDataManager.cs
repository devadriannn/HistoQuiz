using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeacherDashboardDataManager : MonoBehaviour
{
    [Header("Metric UI (Manual Wiring)")]
    [SerializeField] public TMP_Text studentsValueText;
    [SerializeField] public TMP_Text quizzesValueText;
    [SerializeField] public TMP_Text avgScoreValueText;
    [SerializeField] public TMP_Text completionValueText;
    [SerializeField] public TMP_Text classAvgText;

    private void Start()
    {
        LoadDashboardData();
    }

    public void LoadDashboardData()
    {
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("TeacherDashboardDataManager: FirebaseManager instance is null.");
            return;
        }

        FirebaseManager.Instance.InitializeFirebase((success, error) =>
        {
            if (!success)
            {
                Debug.LogError("TeacherDashboardDataManager: Firebase init failed: " + error);
                return;
            }

            FirebaseManager.Instance.Firestore.Collection("users")
                .WhereEqualTo("role", "student")
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.LogError("TeacherDashboardDataManager: Failed to fetch student data: " + task.Exception);
                        return;
                    }

                    QuerySnapshot snapshot = task.Result;
                    List<Dictionary<string, object>> students = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot doc in snapshot.Documents)
                    {
                        if (!doc.Exists) continue;
                        Dictionary<string, object> data = doc.ToDictionary();
                        data["userId"] = doc.Id;
                        students.Add(data);
                    }

                    UpdateDashboardUI(students);
                });
        });
    }

    private void UpdateDashboardUI(List<Dictionary<string, object>> students)
    {
        int totalStudents = students.Count;
        int totalPoints = 0;
        int totalCompletedQuestions = 0;

        int totalQuizCount = 0;
        QuestionsBank bank = UnityEngine.Object.FindFirstObjectByType<QuestionsBank>();
        if (bank != null && bank.questions != null)
        {
            totalQuizCount = bank.questions.Count;
        }

        foreach (Dictionary<string, object> student in students)
        {
            totalPoints += ReadInt(student, "points");

            if (student.TryGetValue("completedQuestions", out object completedObject) && completedObject is List<object> completedList)
            {
                totalCompletedQuestions += completedList.Count;
            }
        }

        float averageScore = totalStudents > 0 ? (float)totalPoints / totalStudents : 0f;
        float completionRate = totalStudents > 0 && totalQuizCount > 0
            ? (float)totalCompletedQuestions / (totalStudents * totalQuizCount) * 100f
            : 0f;

        // Update existing UI elements only
        if (studentsValueText != null) studentsValueText.text = totalStudents.ToString();
        if (quizzesValueText != null) quizzesValueText.text = totalQuizCount.ToString();
        if (avgScoreValueText != null) avgScoreValueText.text = "0"; // Placeholder for active classes if needed
        if (completionValueText != null) completionValueText.text = completionRate.ToString("F0") + "%";
        if (classAvgText != null) classAvgText.text = averageScore.ToString("F0");
    }

    private static int ReadInt(Dictionary<string, object> values, string key)
    {
        if (!values.TryGetValue(key, out object raw) || raw == null) return 0;
        try { return Convert.ToInt32(raw); } catch { return 0; }
    }
}