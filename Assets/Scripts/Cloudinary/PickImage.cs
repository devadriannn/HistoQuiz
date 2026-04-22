using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PickImage : MonoBehaviour
{
    [Header("UI References")]
    public Image previewImage;
    
    [Header("Selected Data")]
    public Texture2D selectedTexture;
    public string selectedPath;

    /// <summary>
    /// Opens the gallery to pick an image.
    /// This should be called from your Upload button's onClick event.
    /// </summary>
    public void PickFromGallery()
    {
        // NativeGallery internally handles permission requests and Editor file picking.
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Debug.Log("Image selected: " + path);
                ProcessSelectedImage(path);
            }
            else
            {
                Debug.Log("User cancelled image selection or permission denied.");
            }
        }, "Select Profile Image", "image/*");
    }

    private void ProcessSelectedImage(string path)
    {
        // Validate extension (png, jpg, jpeg)
        string ext = Path.GetExtension(path).ToLower();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
        {
            Debug.LogError("Unsupported file format: " + ext + ". Please select PNG or JPG.");
            return;
        }

        selectedPath = path;

        // Load image from path
        // Max size 1024 to keep memory usage low for profile pics
        Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);

        if (texture != null)
        {
            selectedTexture = texture;

            if (previewImage != null)
            {
                // Convert texture to sprite for the UI Image component
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                previewImage.sprite = sprite;
                
                // If there's an AspectRatioFitter, set it to the sprite's aspect to fill the circular mask
                AspectRatioFitter fitter = previewImage.GetComponent<AspectRatioFitter>();
                if (fitter != null)
                {
                    fitter.aspectRatio = (float)texture.width / (float)texture.height;
                }
                else
                {
                    previewImage.preserveAspect = true;
                }
                
                // Show the image if it was hidden
                previewImage.gameObject.SetActive(true);
            }
            
            Debug.Log("Profile image successfully updated in preview.");
        }
        else
        {
            Debug.LogError("Failed to load texture from path: " + path);
        }
    }
}