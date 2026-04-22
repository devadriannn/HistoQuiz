// Re-saved to trigger recompile and resolve transient editor error.
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FirestoreLeaderboardUI : MonoBehaviour
{
    [Header("Top 3 Panels")]
    [SerializeField] private GameObject top1Panel;
    [SerializeField] private GameObject top2Panel;
    [SerializeField] private GameObject top3Panel;

    [Header("Scroll View Settings")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject entryPrefab; // This will be the template for Top 4+
    [SerializeField] private GameObject loadingOverlay;

    [Header("Current Player Panel")]
    [SerializeField] private GameObject selfPanel;

    private void Start()
    {
        if (PlayerData.Role != null && PlayerData.Role.ToLower() == "teacher")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TeacherDashboard");
            return;
        }

        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        if (loadingOverlay != null) loadingOverlay.SetActive(true);

        // Initially hide all top panels and player panel to prevent showing old data
        if (top1Panel != null) top1Panel.SetActive(false);
        if (top2Panel != null) top2Panel.SetActive(false);
        if (top3Panel != null) top3Panel.SetActive(false);
        if (selfPanel != null) selfPanel.SetActive(false);

        // Clear existing entries in Scroll View (except the template)
        if (contentParent != null)
        {
            foreach (Transform child in contentParent)
            {
                if (child.gameObject != entryPrefab)
                {
                    // Use DestroyImmediate in edit mode if needed, but this script runs in play mode.
                    Destroy(child.gameObject);
                }
            }
            if (entryPrefab != null) entryPrefab.SetActive(false);
        }

        FirebaseManager.Instance.GetLeaderboard(100, (leaderboard, error) =>
        {
            if (loadingOverlay != null) loadingOverlay.SetActive(false);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning("Leaderboard sync error: " + error);
                
                if (error.Contains("permissions") || error.Contains("insufficient") || error.Contains("No user signed in"))
                {
                    Debug.LogError("FIRESTORE AUTH/RULE ERROR: " + error);
                }
                return;
            }

            if (leaderboard == null || leaderboard.Count == 0)
            {
                Debug.Log("No leaderboard entries found.");
                return;
            }

            // Manually sort the list in C# to ensure correct numeric ranking
            leaderboard.Sort((a, b) =>
            {
                int scoreA = a.ContainsKey("points") ? System.Convert.ToInt32(a["points"]) : 0;
                int scoreB = b.ContainsKey("points") ? System.Convert.ToInt32(b["points"]) : 0;
                return scoreB.CompareTo(scoreA); // Descending order
            });

            string currentUserId = FirebaseManager.Instance.Auth?.CurrentUser?.UserId;
            bool foundSelf = false;

            for (int i = 0; i < leaderboard.Count; i++)
            {
                var entry = leaderboard[i];
                string name = entry.ContainsKey("name") ? entry["name"].ToString() : (entry.ContainsKey("username") ? entry["username"].ToString() : "Unknown");
                int score = entry.ContainsKey("points") ? System.Convert.ToInt32(entry["points"]) : 0;
                string photoUrl = entry.ContainsKey("photoUrl") ? entry["photoUrl"].ToString() : string.Empty;
                
                // Check if this is the current player
                bool isMe = !string.IsNullOrEmpty(currentUserId) && entry.ContainsKey("userId") && entry["userId"].ToString() == currentUserId;
                if (isMe)
                {
                    PopulateSelfPanel(i + 1, score, photoUrl);
                    foundSelf = true;
                }

                if (i == 0) PopulateTopPanel(top1Panel, name, score, 1, isMe, photoUrl);
                else if (i == 1) PopulateTopPanel(top2Panel, name, score, 2, isMe, photoUrl);
                else if (i == 2) PopulateTopPanel(top3Panel, name, score, 3, isMe, photoUrl);
                else
                {
                    CreateScrollEntry(i + 1, name, score, isMe, photoUrl);
                }
            }

            // If player is not in top 100, show their local data (rank unknown)
            if (!foundSelf && selfPanel != null)
            {
                int localScore = GameManager.Instance != null ? GameManager.Instance.stars : 0;
                PopulateSelfPanel(-1, localScore, PlayerData.PhotoUrl);
            }

            // Force layout rebuild to ensure Scroll View bounds are correct
            if (contentParent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
            }
        });
    }

    private void PopulateTopPanel(GameObject panel, string name, int score, int rank, bool isMe, string photoUrl)
    {
        if (panel == null) return;
        
        panel.SetActive(true);
        TMP_Text nameText = FindChildText(panel, "Name");
        TMP_Text scoreText = FindChildText(panel, "Points");
        
        // Search for specific rank text objects (Top1Txt, Top2Txt, Top3Txt)
        TMP_Text rankText = FindChildText(panel, "Top" + rank + "Txt");

        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = score.ToString();

        // Load Profile Image
        Transform profileImage = panel.transform.Find("ProfileImage");

        // Ensure rank text is visible and not masked by ProfileImage
        if (rankText != null)
        {
            rankText.text = rank.ToString();
            if (profileImage != null && rankText.transform.IsChildOf(profileImage))
            {
                // Move out of ProfileImage to avoid masking, keeping world position
                rankText.transform.SetParent(panel.transform, true);
                rankText.transform.SetAsLastSibling();
            }
            rankText.gameObject.SetActive(true);
        }

        if (profileImage != null && !string.IsNullOrEmpty(photoUrl))
        {
            ProfileImageLoader loader = profileImage.GetComponent<ProfileImageLoader>();
            if (loader == null) loader = profileImage.gameObject.AddComponent<ProfileImageLoader>();
            loader.LoadProfileImage(photoUrl);
        }

        // Handle "You" indicator
        if (profileImage != null)
        {
            Transform youIndicator = profileImage.Find("YouIndicator") ?? panel.transform.Find("YouIndicator");
            if (isMe)
            {
                if (youIndicator == null)
                {
                    GameObject go = new GameObject("YouIndicator", typeof(RectTransform), typeof(TextMeshProUGUI));
                    go.transform.SetParent(panel.transform, false);
                    youIndicator = go.transform;
                    
                    TMP_Text txt = go.GetComponent<TMP_Text>();
                    txt.text = "You";
                    txt.fontSize = 30;
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.color = Color.black;

                    RectTransform rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0);
                    rt.anchorMax = new Vector2(0.5f, 0);
                    rt.pivot = new Vector2(0.5f, 1f);
                    
                    // Position it relative to the panel, matching the old relative position to ProfileImage
                    float piY = profileImage.GetComponent<RectTransform>().anchoredPosition.y;
                    rt.anchoredPosition = new Vector2(0, piY + 51.801f); 
                    rt.sizeDelta = new Vector2(100, 40);
                }
                else
                {
                    if (youIndicator.parent == profileImage)
                    {
                        youIndicator.SetParent(panel.transform, true);
                    }
                }
                youIndicator.gameObject.SetActive(true);
                youIndicator.SetAsLastSibling();
            }
            else if (youIndicator != null)
            {
                youIndicator.gameObject.SetActive(false);
            }
        }
    }

    private void CreateScrollEntry(int rank, string name, int score, bool isMe, string photoUrl)
    {
        if (contentParent == null || entryPrefab == null) return;

        GameObject newEntry = Instantiate(entryPrefab, contentParent);
        newEntry.name = "Rank_" + rank;
        newEntry.SetActive(true);

        TMP_Text rankText = FindChildText(newEntry, "Top");
        TMP_Text nameText = FindChildText(newEntry, "Name");
        TMP_Text scoreText = FindChildText(newEntry, "Points");

        if (rankText != null) rankText.text = rank.ToString();
        if (nameText != null) nameText.text = isMe ? name + " (You)" : name;
        if (scoreText != null) scoreText.text = score.ToString();

        // Load Profile Image
        Transform profileImage = newEntry.transform.Find("ProfileImage");
        if (profileImage != null && !string.IsNullOrEmpty(photoUrl))
        {
            ProfileImageLoader loader = profileImage.GetComponent<ProfileImageLoader>();
            if (loader == null) loader = profileImage.gameObject.AddComponent<ProfileImageLoader>();
            loader.LoadProfileImage(photoUrl);
        }
    }
    private void PopulateSelfPanel(int rank, int score, string photoUrl)
    {
        if (selfPanel == null) return;

        selfPanel.SetActive(true);
        TMP_Text nameText = FindChildText(selfPanel, "Name");
        TMP_Text scoreText = FindChildText(selfPanel, "Points");
        TMP_Text rankText = FindChildText(selfPanel, "Top");

        if (nameText != null) nameText.text = "You";
        if (scoreText != null) scoreText.text = score.ToString();
        if (rankText != null) rankText.text = rank > 0 ? rank.ToString() : "--";

        // Load Profile Image
        Transform profileImage = selfPanel.transform.Find("ProfileImage");
        if (profileImage != null && !string.IsNullOrEmpty(photoUrl))
        {
            ProfileImageLoader loader = profileImage.GetComponent<ProfileImageLoader>();
            if (loader == null) loader = profileImage.gameObject.AddComponent<ProfileImageLoader>();
            loader.LoadProfileImage(photoUrl);
        }
    }

    private TMP_Text FindChildText(GameObject parent, string childName)
    {
        // Try direct child first
        Transform t = parent.transform.Find(childName);
        if (t != null) return t.GetComponent<TMP_Text>();

        // Try recursive search if not found
        return FindChildRecursive(parent.transform, childName);
    }

    private TMP_Text FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child.GetComponent<TMP_Text>();
            TMP_Text found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
}



