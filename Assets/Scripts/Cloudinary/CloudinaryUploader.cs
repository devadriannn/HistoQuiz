using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class CloudinaryUploader : MonoBehaviour
{
    [Header("Cloudinary")]
    public string cloudName = "dar3vwcwt";
    public string uploadPreset = "profile_pics";

    public IEnumerator UploadImage(byte[] imageBytes, System.Action<string> onSuccess, System.Action<string> onError)
    {
        return UploadFile(imageBytes, "image.png", "image/png", onSuccess, onError);
    }

    public IEnumerator UploadFile(byte[] fileBytes, string fileName, string mimeType, System.Action<string> onSuccess, System.Action<string> onError)
    {
        // Cloudinary handles PDFs as 'image' or 'raw' usually depending on needs, 
        // but 'auto' is best for mixed types.
        string url = $"https://api.cloudinary.com/v1_1/{cloudName}/auto/upload";

        WWWForm form = new WWWForm();
        form.AddField("upload_preset", uploadPreset);
        form.AddBinaryData("file", fileBytes, fileName, mimeType);

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

    #if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
    #else
            if (request.isNetworkError || request.isHttpError)
    #endif
            {
                onError?.Invoke(request.error + "\n" + request.downloadHandler.text);
            }
            else
            {
                string json = request.downloadHandler.text;
                Debug.Log("Cloudinary response: " + json);

                CloudinaryUploadResponse response = JsonUtility.FromJson<CloudinaryUploadResponse>(json);
                onSuccess?.Invoke(response.secure_url);
            }
        }
    }
}

[System.Serializable]
public class CloudinaryUploadResponse
{
    public string asset_id;
    public string public_id;
    public string version;
    public string secure_url;
    public string url;
}