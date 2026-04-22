using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealtimeLeaderboard : MonoBehaviour
{
    [SerializeField] private Transform entryContainer;
    [SerializeField] private TMP_FontAsset fontAsset;

    private Dictionary<string, (GameObject row, TMP_Text nameText, TMP_Text scoreText, int currentScore)> entries = new Dictionary<string, (GameObject row, TMP_Text nameText, TMP_Text scoreText, int currentScore)>();

    public void InitViaRef(Transform container, TMP_FontAsset font)
    {
        entryContainer = container;
        fontAsset = font;
    }

    public void Init(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode)) return;
        
        // Clear existing
        foreach (var entry in entries.Values)
        {
            if (entry.row != null) Destroy(entry.row);
        }
        entries.Clear();

        FirebaseRoomManager.Instance.ListenToStudents(roomCode, OnStudentsChanged);
    }

    private void OnStudentsChanged(QuerySnapshot snapshot)
    {
        if (snapshot == null) return;

        foreach (var doc in snapshot.Documents)
        {
            if (!doc.Exists) continue;

            string id = doc.Id;
            string name = doc.ContainsField("name") ? doc.GetValue<string>("name") : "Unknown";
            int score = doc.ContainsField("score") ? Convert.ToInt32(doc.GetValue<object>("score")) : 0;
            bool ready = doc.ContainsField("ready") && doc.GetValue<bool>("ready");

            if (!entries.ContainsKey(id))
            {
                CreateEntry(id, name, score, ready);
            }
            else
            {
                UpdateEntry(id, score, ready);
            }
        }

        SortEntries();
    }

    private void CreateEntry(string id, string name, int score, bool ready)
    {
        GameObject row = new GameObject("LeaderboardRow_" + id);
        row.transform.SetParent(entryContainer, false);
        
        var rect = row.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 50);
        
        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 0, 0);
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;

        var nameText = CreateText(row.transform, "Name", name, 18, Color.black, TextAlignmentOptions.Left);
        var scoreText = CreateText(row.transform, "Score", score.ToString(), 18, ready ? Color.blue : Color.gray, TextAlignmentOptions.Right);

        entries[id] = (row, nameText, scoreText, score);
    }

    private void UpdateEntry(string id, int score, bool ready)
    {
        var entry = entries[id];
        entry.scoreText.text = score.ToString();
        entry.scoreText.color = ready ? new Color(0, 0.5f, 0) : Color.gray; // Green if ready/active
        entry.currentScore = score;
        entries[id] = entry;
    }

    private TMP_Text CreateText(Transform parent, string goName, string content, float size, Color color, TextAlignmentOptions align)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableWordWrapping = false;
        return tmp;
    }

    private void SortEntries()
    {
        var sorted = entries.OrderByDescending(x => x.Value.currentScore).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Value.row.transform.SetSiblingIndex(i);
        }
    }
}
