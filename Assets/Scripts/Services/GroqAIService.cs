using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GroqAIService : MonoBehaviour
{
    private static GroqAIService instance;
    public static GroqAIService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = UnityEngine.Object.FindFirstObjectByType<GroqAIService>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GroqAIService");
                    instance = go.AddComponent<GroqAIService>();
                }
            }
            return instance;
        }
    }

    [Header("Groq API Settings")]
    [SerializeField] private string groqApiKey = "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"; 
    private const string ApiUrl = "https://api.groq.com/openai/v1/chat/completions";
    private const string DefaultModel = "llama-3.3-70b-versatile";

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
            return;
        }

        // Hard override to ensure the key provided in the prompt is used if Inspector is stale
        groqApiKey = "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";
    }

    [Serializable]
    private class GroqRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public float temperature = 0.5f;
    }

    [Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class GroqResponse
    {
        public List<Choice> choices;
    }

    [Serializable]
    private class Choice
    {
        public Message message;
    }

    [Serializable]
    private class Message
    {
        public string content;
    }

    [Serializable]
    private class LessonWrapper
    {
        public string title;
        public List<string> topics;
        public string fullContent;
    }

    [Serializable]
    private class QuizWrapper
    {
        public List<QuestionsBank.Entry> questions;
    }

    public void AnalyzeLessonFromText(string text, Action<string, List<string>, string, string> onCompleted)
    {
        StartCoroutine(AnalyzeLessonCoroutine(text, onCompleted));
    }

    private IEnumerator AnalyzeLessonCoroutine(string text, Action<string, List<string>, string, string> onCompleted)
    {
        string currentKey = groqApiKey.Trim();
        if (string.IsNullOrEmpty(currentKey))
        {
            onCompleted?.Invoke(null, null, null, "Groq API Key is missing.");
            yield break;
        }

        string promptText = $"Act as an expert educator. I have a lesson file named: \"{text}\". " +
                           $"Based on this title, identify the most likely subject and generate: " +
                           $"1. A professional lesson title. " +
                           $"2. A list of 5-8 key topics. " +
                           $"3. A COMPREHENSIVE and DETAILED educational reading material (at least 600 words) that a student can read to learn this subject fully. " +
                           $"Format your response EXACTLY as a JSON object with 'title' (string), 'topics' (array of strings), and 'fullContent' (string) keys. " +
                           $"Include ONLY the raw JSON, no markdown formatting, no explanation.";

        GroqRequest requestBody = new GroqRequest
        {
            model = DefaultModel,
            messages = new List<ChatMessage> {
                new ChatMessage { role = "user", content = promptText }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + currentKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onCompleted?.Invoke(null, null, null, "Groq API Error: " + request.error + "\n" + request.downloadHandler.text);
            }
            else
            {
                try
                {
                    GroqResponse response = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                    string rawContent = response.choices[0].message.content;
                    
                    if (rawContent.Contains("```json")) rawContent = rawContent.Replace("```json", "").Replace("```", "").Trim();
                    else if (rawContent.Contains("```")) rawContent = rawContent.Replace("```", "").Trim();

                    LessonWrapper wrapper = JsonUtility.FromJson<LessonWrapper>(rawContent);
                    onCompleted?.Invoke(wrapper.title, wrapper.topics, wrapper.fullContent, null);
                }
                catch (Exception ex)
                {
                    onCompleted?.Invoke(null, null, null, "Failed to parse Groq response: " + ex.Message + "\nRaw: " + request.downloadHandler.text);
                }
            }
        }
    }

    public void GenerateQuizFromText(string text, int questionCount, Action<List<QuestionsBank.Entry>, string> onCompleted)
    {
        StartCoroutine(GenerateQuizCoroutine(text, questionCount, onCompleted));
    }

    private IEnumerator GenerateQuizCoroutine(string text, int questionCount, Action<List<QuestionsBank.Entry>, string> onCompleted)
    {
        string currentKey = groqApiKey.Trim();
        if (string.IsNullOrEmpty(currentKey))
        {
            onCompleted?.Invoke(null, "Groq API Key is missing.");
            yield break;
        }

        string promptText = $"Generate {questionCount} multiple choice questions about Philippine history based on this text: \"{text}\". " +
                           "Format your response EXACTLY as a JSON object with a 'questions' key containing an array. " +
                           "Each question MUST follow this structure: { \"questionId\": \"unique_string\", \"prompt\": \"Question?\", \"hintText\": \"hint\", \"factText\": \"fact\", \"correctIndex\": 0, \"options\": [\"Correct\", \"Wrong1\", \"Wrong2\", \"Wrong3\"] }. " +
                           "Include ONLY the raw JSON, no markdown formatting, no explanation.";

        GroqRequest requestBody = new GroqRequest
        {
            model = DefaultModel,
            messages = new List<ChatMessage> {
                new ChatMessage { role = "user", content = promptText }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + currentKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onCompleted?.Invoke(null, "Groq API Error: " + request.error);
            }
            else
            {
                try
                {
                    GroqResponse response = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                    string rawContent = response.choices[0].message.content;
                    
                    if (rawContent.Contains("```json")) rawContent = rawContent.Replace("```json", "").Replace("```", "").Trim();
                    else if (rawContent.Contains("```")) rawContent = rawContent.Replace("```", "").Trim();

                    QuizWrapper wrapper = JsonUtility.FromJson<QuizWrapper>(rawContent);
                    onCompleted?.Invoke(wrapper.questions, null);
                }
                catch (Exception ex)
                {
                    onCompleted?.Invoke(null, "Failed to parse Groq response: " + ex.Message);
                }
            }
        }
    }
}