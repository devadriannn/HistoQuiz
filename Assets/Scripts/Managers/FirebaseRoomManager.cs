using System;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseRoomManager : MonoBehaviour
{
    private static FirebaseRoomManager instance;
    public static FirebaseRoomManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<FirebaseRoomManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FirebaseRoomManager");
                    instance = go.AddComponent<FirebaseRoomManager>();
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
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private ListenerRegistration roomListener;
    private ListenerRegistration studentsListener;

    private void OnDestroy()
    {
        StopListeningToRoom();
        StopListeningToStudents();
    }

    public void ListenToRoom(string roomCode, Action<DocumentSnapshot> onRoomChanged)
    {
        StopListeningToRoom();
        roomListener = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode).Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                onRoomChanged?.Invoke(snapshot);
            }
        });
    }

    public void StopListeningToRoom()
    {
        roomListener?.Stop();
        roomListener = null;
    }

    public void ListenToStudents(string roomCode, Action<QuerySnapshot> onStudentsChanged)
    {
        StopListeningToStudents();
        studentsListener = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode).Collection("students").Listen(snapshot =>
        {
            onStudentsChanged?.Invoke(snapshot);
        });
    }

    public void StopListeningToStudents()
    {
        studentsListener?.Stop();
        studentsListener = null;
    }

    public void UpdateStudentScore(string roomCode, int score, Action<bool, string> onCompleted = null)
    {
        string studentId = FirebaseManager.Instance.Auth.CurrentUser.UserId;
        DocumentReference studentRef = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode).Collection("students").Document(studentId);

        studentRef.UpdateAsync("score", score).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onCompleted?.Invoke(false, "Failed to update score.");
                return;
            }
            onCompleted?.Invoke(true, null);
        });
    }

    public void SetRoomQuiz(string roomCode, List<QuestionsBank.Entry> questions, Action<bool, string> onCompleted)
    {
        DocumentReference roomRef = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode);
        
        List<Dictionary<string, object>> questionList = new List<Dictionary<string, object>>();
        foreach (var q in questions)
        {
            questionList.Add(new Dictionary<string, object>
            {
                { "questionId", q.questionId },
                { "prompt", q.prompt },
                { "hintText", q.hintText },
                { "factText", q.factText },
                { "correctIndex", q.correctIndex },
                { "options", q.options }
            });
        }

        roomRef.UpdateAsync("questions", questionList).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onCompleted?.Invoke(false, "Failed to store quiz in room.");
                return;
            }
            onCompleted?.Invoke(true, null);
        });
    }

    public void CreateRoom(string quizId, Action<string, string> onCompleted)
    {
        FirebaseManager.Instance.InitializeFirebase((success, error) =>
        {
            if (!success)
            {
                onCompleted?.Invoke(null, error);
                return;
            }

            string teacherId = FirebaseManager.Instance.Auth.CurrentUser.UserId;
            string roomCode = UnityEngine.Random.Range(100000, 999999).ToString();

            DocumentReference roomRef = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode);
            
            Dictionary<string, object> roomData = new Dictionary<string, object>
            {
                { "teacherId", teacherId },
                { "quizId", quizId },
                { "status", "waiting" },
                { "quizStarted", false },
                { "createdAt", FieldValue.ServerTimestamp }
            };

            roomRef.SetAsync(roomData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    string innerError = task.Exception?.InnerException?.Message ?? "Check Firestore rules or internet connection.";
                    Debug.LogError($"Room creation failed: {innerError}");
                    onCompleted?.Invoke(null, "Failed to create room. Please check your account permissions or internet.");
                    return;
                }
                onCompleted?.Invoke(roomCode, null);
            });
        });
    }

    public void JoinRoom(string roomCode, string studentName, Action<bool, string> onCompleted)
    {
        FirebaseManager.Instance.InitializeFirebase((success, error) =>
        {
            if (!success)
            {
                onCompleted?.Invoke(false, error);
                return;
            }

            string studentId = FirebaseManager.Instance.Auth.CurrentUser.UserId;
            DocumentReference roomRef = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode);

            roomRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted || !task.Result.Exists)
                {
                    onCompleted?.Invoke(false, "Room not found.");
                    return;
                }

                Dictionary<string, object> studentData = new Dictionary<string, object>
                {
                    { "name", studentName },
                    { "ready", false },
                    { "score", 0 },
                    { "finished", false },
                    { "lastUpdated", FieldValue.ServerTimestamp }
                };

                roomRef.Collection("students").Document(studentId).SetAsync(studentData).ContinueWithOnMainThread(joinTask =>
                {
                    if (joinTask.IsCanceled || joinTask.IsFaulted)
                    {
                        onCompleted?.Invoke(false, "Failed to join room.");
                        return;
                    }
                    onCompleted?.Invoke(true, null);
                });
            });
        });
    }

    public void SetReady(string roomCode, bool isReady, Action<bool, string> onCompleted)
    {
        string studentId = FirebaseManager.Instance.Auth.CurrentUser.UserId;
        DocumentReference studentRef = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode).Collection("students").Document(studentId);

        studentRef.UpdateAsync("ready", isReady).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onCompleted?.Invoke(false, "Failed to update ready status.");
                return;
            }
            onCompleted?.Invoke(true, null);
        });
    }

    public void StartQuiz(string roomCode, Action<bool, string> onCompleted)
    {
        DocumentReference roomRef = FirebaseManager.Instance.Firestore.Collection("rooms").Document(roomCode);
        roomRef.UpdateAsync("quizStarted", true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                onCompleted?.Invoke(false, "Failed to start quiz.");
                return;
            }
            onCompleted?.Invoke(true, null);
        });
    }
}
