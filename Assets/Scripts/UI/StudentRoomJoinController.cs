using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StudentRoomJoinController : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text statusText;
    
    [SerializeField] private MultiplayerQuizManager quizManager;

    private void Awake()
    {
        if (roomCodeInput == null) roomCodeInput = transform.Find("JoinPanel/RoomCodeInput")?.GetComponent<TMP_InputField>();
        if (nameInput == null) nameInput = transform.Find("JoinPanel/NameInput")?.GetComponent<TMP_InputField>();
        if (joinButton == null) joinButton = transform.Find("JoinPanel/JoinBtn")?.GetComponent<Button>();
        if (statusText == null) statusText = transform.Find("JoinPanel/StatusText")?.GetComponent<TMP_Text>();
        if (quizManager == null) quizManager = GetComponent<MultiplayerQuizManager>();
    }

    private void Start()
    {
        if (PlayerData.Role != null && PlayerData.Role.ToLower() == "teacher")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TeacherDashboard");
            return;
        }

        if (joinButton != null) joinButton.onClick.AddListener(OnJoinClicked);
        if (nameInput != null) nameInput.text = PlayerData.Name;
    }

    private void OnJoinClicked()
    {
        if (roomCodeInput == null) return;
        
        string code = roomCodeInput.text;
        string name = string.IsNullOrEmpty(nameInput.text) ? PlayerData.Name : nameInput.text;

        if (string.IsNullOrEmpty(code))
        {
            if (statusText != null) statusText.text = "Please enter a room code.";
            return;
        }

        if (statusText != null) statusText.text = "Joining Room...";
        FirebaseRoomManager.Instance.JoinRoom(code, name, (success, error) =>
        {
            if (success)
            {
                if (statusText != null) statusText.text = "Joined! Waiting for teacher...";
                
                FirebaseRoomManager.Instance.SetReady(code, true, (readySuccess, readyError) =>
                {
                    if (quizManager != null) quizManager.Initialize(code);
                    gameObject.SetActive(false); // Hide join UI
                });
            }
            else
            {
                if (statusText != null) statusText.text = "Error: " + error;
            }
        });
    }
}
