using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerQuizManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private List<Button> optionButtons;
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_Text readyCountText;

    [Header("Countdown Settings")]
    [SerializeField] private GameObject countdownOverlay;
    [SerializeField] private TMP_Text countdownText;

    private string currentRoomCode;
    private List<QuestionsBank.Entry> questions = new List<QuestionsBank.Entry>();
    private int currentQuestionIndex = -1;
    private int score = 0;
    private bool isCountingDown = false;

    private void Awake()
    {
        if (roomCodeText == null) roomCodeText = transform.Find("JoinPanel/Title")?.GetComponent<TMP_Text>();
        if (questionText == null) questionText = transform.Find("QuizPanel/QuestionText")?.GetComponent<TMP_Text>();
        if (statusText == null) statusText = transform.Find("JoinPanel/StatusText")?.GetComponent<TMP_Text>();
        if (quizPanel == null) quizPanel = transform.Find("QuizPanel")?.gameObject;
        if (lobbyPanel == null) lobbyPanel = transform.Find("JoinPanel")?.gameObject;
        if (readyCountText == null) readyCountText = transform.Find("QuizPanel/ReadyCountText")?.GetComponent<TMP_Text>();
        
        if (optionButtons == null || optionButtons.Count == 0)
        {
            optionButtons = new List<Button>();
            Transform optionsContainer = transform.Find("QuizPanel/Options");
            if (optionsContainer != null)
            {
                foreach (Transform child in optionsContainer)
                {
                    Button btn = child.GetComponent<Button>();
                    if (btn != null) optionButtons.Add(btn);
                }
            }
        }
    }

    public void Initialize(string roomCode)
    {
        currentRoomCode = roomCode;
        if (roomCodeText != null) roomCodeText.text = "ROOM: " + roomCode;
        
        lobbyPanel?.SetActive(true);
        quizPanel?.SetActive(false);
        if (countdownOverlay != null) countdownOverlay.SetActive(false);
        
        FirebaseRoomManager.Instance.ListenToRoom(roomCode, OnRoomUpdate);
        FirebaseRoomManager.Instance.ListenToStudents(roomCode, OnStudentsUpdate);
    }

    private void OnDestroy()
    {
        if (FirebaseRoomManager.Instance != null)
        {
            FirebaseRoomManager.Instance.StopListeningToRoom();
            FirebaseRoomManager.Instance.StopListeningToStudents();
        }
    }

    private void OnRoomUpdate(DocumentSnapshot snapshot)
    {
        if (!snapshot.Exists) return;

        bool started = snapshot.ContainsField("quizStarted") && snapshot.GetValue<bool>("quizStarted");
        
        if (started && currentQuestionIndex == -1 && !isCountingDown)
        {
            LoadQuizData(snapshot);
            StartCoroutine(StartCountdownRoutine());
        }
    }

    private IEnumerator StartCountdownRoutine()
    {
        isCountingDown = true;
        lobbyPanel?.SetActive(false);
        if (countdownOverlay != null) countdownOverlay.SetActive(true);

        for (int i = 5; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownOverlay != null) countdownOverlay.SetActive(false);
        StartQuiz();
        isCountingDown = false;
    }

    private void OnStudentsUpdate(QuerySnapshot snapshot)
    {
        int readyCount = 0;
        int totalCount = snapshot.Count;
        
        foreach (var doc in snapshot.Documents)
        {
            if (doc.ContainsField("ready") && doc.GetValue<bool>("ready")) readyCount++;
        }

        if (readyCountText != null) readyCountText.text = $"Students Ready: {readyCount} / {totalCount}";
    }

    private void LoadQuizData(DocumentSnapshot snapshot)
    {
        if (!snapshot.ContainsField("questions")) return;

        var questionsData = snapshot.GetValue<List<object>>("questions");
        questions = new List<QuestionsBank.Entry>();

        foreach (var qObj in questionsData)
        {
            var qDict = qObj as Dictionary<string, object>;
            var entry = new QuestionsBank.Entry
            {
                questionId = qDict.ContainsKey("questionId") ? qDict["questionId"].ToString() : Guid.NewGuid().ToString(),
                prompt = qDict.ContainsKey("prompt") ? qDict["prompt"].ToString() : "",
                hintText = qDict.ContainsKey("hintText") ? qDict["hintText"].ToString() : "",
                factText = qDict.ContainsKey("factText") ? qDict["factText"].ToString() : "",
                correctIndex = qDict.ContainsKey("correctIndex") ? Convert.ToInt32(qDict["correctIndex"]) : 0,
                options = new List<string>()
            };

            if (qDict.ContainsKey("options") && qDict["options"] is List<object> opts)
            {
                foreach (var opt in opts) entry.options.Add(opt.ToString());
            }

            questions.Add(entry);
        }
    }

    private void StartQuiz()
    {
        quizPanel?.SetActive(true);
        currentQuestionIndex = 0;
        score = 0;
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (currentQuestionIndex < 0 || currentQuestionIndex >= questions.Count)
        {
            FinishQuiz();
            return;
        }

        var q = questions[currentQuestionIndex];
        if (questionText != null) questionText.text = q.prompt;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < q.options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                var label = optionButtons[i].GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = q.options[i];
                
                int index = i;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        if (statusText != null) statusText.text = $"Question {currentQuestionIndex + 1} of {questions.Count}";
    }

    private void OnOptionSelected(int index)
    {
        var q = questions[currentQuestionIndex];
        bool isCorrect = (index == q.correctIndex);

        if (isCorrect)
        {
            score += 100;
            FirebaseRoomManager.Instance.UpdateStudentScore(currentRoomCode, score);
        }

        currentQuestionIndex++;
        ShowQuestion();
    }

    private void FinishQuiz()
    {
        if (questionText != null) questionText.text = "Quiz Finished!";
        if (statusText != null) statusText.text = $"Final Score: {score}";
        foreach (var btn in optionButtons) btn.gameObject.SetActive(false);
    }
}