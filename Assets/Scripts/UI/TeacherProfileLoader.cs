using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class TeacherProfileLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private UnityEngine.UI.Image profileImage;
    [SerializeField] private Sprite defaultProfileSprite;

    private void Start()
    {
        LoadProfileData();
    }

    public void LoadProfileData()
    {
        // 1. Set Name/Role from PlayerData
        string name = PlayerData.Name;
        string role = PlayerData.Role;

        if (nameText != null)
        {
            string displayName = "Teacher"; // Default

            if (!string.IsNullOrWhiteSpace(name))
            {
                // If it's a teacher, maybe prefix it. If student, just show name (though they shouldn't be here)
                if (!string.IsNullOrWhiteSpace(role) && role.ToLower() == "teacher")
                {
                    displayName = "Teacher " + name;
                }
                else
                {
                    displayName = name;
                }
            }
            else if (!string.IsNullOrWhiteSpace(role))
            {
                // Fallback to role name if real name is empty
                displayName = char.ToUpper(role[0]) + role.Substring(1);
            }

            nameText.text = displayName;
            Debug.Log($"TeacherProfileLoader: Setting display name to {displayName}");
        }

        // 2. Load Image if PhotoUrl exists
        if (profileImage != null)
{
            string photoUrl = PlayerData.PhotoUrl;
            if (!string.IsNullOrWhiteSpace(photoUrl))
            {
                Debug.Log($"TeacherProfileLoader: Loading photo from {photoUrl}");
                StartCoroutine(DownloadImage(photoUrl));
            }
            else if (defaultProfileSprite != null)
            {
                profileImage.sprite = defaultProfileSprite;
            }
        }
    }

    private IEnumerator DownloadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    profileImage.sprite = sprite;
                }
            }
            else
            {
                Debug.LogWarning("Failed to download teacher profile image: " + request.error);
                if (defaultProfileSprite != null)
                {
                    profileImage.sprite = defaultProfileSprite;
                }
            }
        }
    }
}
