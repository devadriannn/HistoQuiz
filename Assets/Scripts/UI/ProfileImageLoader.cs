using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class ProfileImageLoader : MonoBehaviour
{
    [SerializeField] private Image displayImage; // The image component that shows the downloaded photo
    [SerializeField] private Sprite defaultSprite;

    private void Awake()
    {
        SetupMasking();
    }

    private void SetupMasking()
    {
        // 1. Ensure this object has an Image component for the mask shape
        Image maskImage = GetComponent<Image>();
        if (maskImage == null)
        {
            maskImage = gameObject.AddComponent<Image>();
        }

        // 2. Ensure this object has a Mask component
        Mask mask = GetComponent<Mask>();
        if (mask == null)
        {
            mask = gameObject.AddComponent<Mask>();
        }
        mask.showMaskGraphic = true; // Show the background shape if desired, or false to hide

        // 3. Create or find the display child
        if (displayImage == null)
        {
            // Search for common preview names first
            Transform child = transform.Find("PreviewImg") ?? transform.Find("DisplayPhoto") ?? transform.Find("UserPhoto");
            
            if (child == null)
            {
                GameObject go = new GameObject("UserPhoto", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                
                displayImage = go.GetComponent<Image>();
            }
            else
            {
                displayImage = child.GetComponent<Image>();
            }
        }
        
        if (displayImage != null)
        {
            displayImage.preserveAspect = true;
            // The display image should not be the mask, it should be the child
            if (displayImage.gameObject == gameObject)
            {
                // If it was assigned to self, we need to fix that
                Debug.LogWarning("ProfileImageLoader: displayImage should be a child of the mask object. Creating one.");
                displayImage = null;
                SetupMasking();
            }
        }
    }

    public void LoadProfileImage(string url)
    {
        if (displayImage == null) SetupMasking();

        if (string.IsNullOrWhiteSpace(url))
        {
            if (defaultSprite != null && displayImage != null) displayImage.sprite = defaultSprite;
            return;
        }

        StartCoroutine(LoadImageRoutine(url));
    }

    private IEnumerator LoadImageRoutine(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(request);
                if (tex != null && displayImage != null)
                {
                    Sprite sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    displayImage.sprite = sprite;
                }
            }
            else
            {
                Debug.LogWarning("Failed to load profile image: " + request.error);
                if (defaultSprite != null && displayImage != null) displayImage.sprite = defaultSprite;
            }
        }
    }
}
