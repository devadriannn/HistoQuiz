using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[DisallowMultipleComponent]
public sealed class StudentDashboardLogout : MonoBehaviour
{
    [SerializeField] private string loginSceneName = "Login";
    [SerializeField] private float logoutDelaySeconds = 5f;

    private GameObject logoutAlertModal;
    private TMP_Text logoutText;
    private Button yesButton;
    private Button noButton;
    private GameObject loadingObject;
    private VideoPlayer loadingVideo;
    private bool isProcessing;

    private void Awake()
    {
        ResolveReferences();
        BindModalButtons();
        HideModalImmediate();
    }

    public void Logout()
    {
        OpenLogoutPrompt();
    }

    public void OpenLogoutPrompt()
    {
        ResolveReferences();

        if (isProcessing)
        {
            return;
        }

        if (logoutText != null)
        {
            logoutText.text = "Are you sure you want to logout?";
        }

        SetButtonVisible(yesButton, true);
        SetButtonVisible(noButton, true);
        SetLoadingVisible(false);

        if (logoutAlertModal != null)
        {
            logoutAlertModal.SetActive(true);
        }
    }

    public void CancelLogout()
    {
        if (!isProcessing)
        {
            HideModalImmediate();
        }
    }

    public void ConfirmLogout()
    {
        ResolveReferences();

        if (!isProcessing)
        {
            StartCoroutine(LogoutRoutine());
        }
    }

    private IEnumerator LogoutRoutine()
    {
        isProcessing = true;

        if (logoutAlertModal != null)
        {
            logoutAlertModal.SetActive(true);
        }

        if (logoutText != null)
        {
            logoutText.text = "Logging out...";
        }

        SetButtonVisible(yesButton, false);
        SetButtonVisible(noButton, false);
        SetLoadingVisible(true);

        // Set the specific loading label if found in the loading object
        if (loadingObject != null)
        {
            var loadingTxt = loadingObject.transform.Find("LoadingTxt")?.GetComponent<TMP_Text>();
            if (loadingTxt != null) loadingTxt.text = "Logging Out...";
        }

        float elapsed = 0f;
        while (elapsed < logoutDelaySeconds)
        {
            elapsed += Time.unscaledDeltaTime;

            if (loadingObject != null && loadingVideo == null)
            {
                loadingObject.transform.Rotate(0f, 0f, -240f * Time.unscaledDeltaTime);
            }

            yield return null;
        }

        FirebaseManager.Instance.SignOut();
        PlayerData.Clear();

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetToDefaults();
        }

        PlayerPrefs.DeleteKey("FirebaseIdToken");
PlayerPrefs.DeleteKey("FirebaseRefreshToken");
        PlayerPrefs.DeleteKey("FirebaseLocalId");
        PlayerPrefs.DeleteKey("FirebaseEmail");
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToLogin();
            yield break;
        }

        SceneManager.LoadScene(loginSceneName);
    }

    private void ResolveReferences()
    {
        if (logoutAlertModal == null)
        {
            logoutAlertModal = FindSceneObject("LogoutAlertModal");
        }

        if (logoutText == null)
        {
            GameObject textObject = FindSceneObject("LogoutText");
            if (textObject != null)
            {
                logoutText = textObject.GetComponent<TMP_Text>();
            }
        }

        if (yesButton == null)
        {
            GameObject yesObject = FindSceneObject("YesBtn");
            if (yesObject != null)
            {
                yesButton = yesObject.GetComponent<Button>();
            }
        }

        if (noButton == null)
        {
            GameObject noObject = FindSceneObject("NoBtn");
            if (noObject != null)
            {
                noButton = noObject.GetComponent<Button>();
            }
        }

        if (loadingObject == null)
        {
            loadingObject = FindSceneObject("Loading");
        }

        if (loadingVideo == null && loadingObject != null)
        {
            loadingVideo = loadingObject.GetComponent<VideoPlayer>();
        }

        BindModalButtons();
    }

    private void HideModalImmediate()
    {
        SetLoadingVisible(false);
        SetButtonVisible(yesButton, true);
        SetButtonVisible(noButton, true);

        if (logoutText != null)
        {
            logoutText.text = "Are you sure you want to logout?";
        }

        if (logoutAlertModal != null)
        {
            logoutAlertModal.SetActive(false);
        }

        isProcessing = false;
    }

    private void SetLoadingVisible(bool visible)
    {
        if (loadingObject == null)
        {
            return;
        }

        loadingObject.SetActive(visible);

        if (loadingVideo != null)
        {
            if (visible)
            {
                loadingVideo.Play();
            }
            else
            {
                loadingVideo.Stop();
            }
        }
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(visible);
        button.interactable = visible;
    }

    private void BindModalButtons()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(ConfirmLogout);
            yesButton.onClick.AddListener(ConfirmLogout);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(CancelLogout);
            noButton.onClick.AddListener(CancelLogout);
        }
    }

    private GameObject FindSceneObject(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid() && activeScene.isLoaded)
        {
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                Transform match = FindChildRecursive(rootObjects[i].transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }
        }

        return GameObject.Find(objectName);
    }

    private static Transform FindChildRecursive(Transform parent, string objectName)
    {
        if (parent == null)
        {
            return null;
        }

        if (string.Equals(parent.name, objectName, System.StringComparison.Ordinal))
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform match = FindChildRecursive(parent.GetChild(i), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
