using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProfileImagePicker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Image targetImage; // Preview image if any
    [SerializeField] private string[] allowedExtensions = { "png", "jpg", "jpeg" };

    public string LastSelectedPath { get; private set; }
    public byte[] LastSelectedImageData { get; private set; }

    public void OpenFilePicker()
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Select Profile Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            ProcessImage(path);
        }
#elif UNITY_ANDROID
        Debug.Log("File manager triggered on Android. (Requires Native Plugin for full implementation)");
        // On mobile, you typically use a plugin like 'Native Gallery' 
        // to access the Android file system and handle permissions.
#else
        Debug.Log("File manager not implemented for this platform.");
#endif
    }

    private void ProcessImage(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        string extension = Path.GetExtension(path).ToLower().Replace(".", "");
        bool isAllowed = false;
        foreach (string ext in allowedExtensions)
        {
            if (ext == extension)
            {
                isAllowed = true;
                break;
            }
        }

        if (!isAllowed)
        {
            Debug.LogError("Unsupported file format: " + extension);
            return;
        }

        LastSelectedPath = path;
        LastSelectedImageData = File.ReadAllBytes(path);

        if (targetImage != null)
        {
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(LastSelectedImageData))
            {
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                targetImage.sprite = sprite;
            }
        }

        Debug.Log("Image loaded: " + path);
    }
}
