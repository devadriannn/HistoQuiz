using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private string dashboardSceneName = "StudentDashboard";
    [SerializeField] private string loginSceneName = "Login";

    public int coins;
    public int stars;
    public string selectedMode = "FourPics";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMeta();
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        coins = Mathf.Max(0, coins + amount);
        SaveMeta();
    }

    public bool SpendCoin(int amount)
    {
        int safeAmount = Mathf.Max(0, amount);
        if (coins < safeAmount)
        {
            return false;
        }

        coins -= safeAmount;
        SaveMeta();
        return true;
    }

    public void AddStars(int amount)
    {
        stars = Mathf.Max(0, stars + amount);
        SaveMeta();
    }

    public void SetMode(string mode)
    {
        selectedMode = string.IsNullOrWhiteSpace(mode) ? "FourPics" : mode.Trim();
        SaveMeta();
    }

    public void SetCurrentScore(int score)
    {
        PlayerData.Score = Mathf.Max(0, score);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("GameManager cannot load an empty scene name.");
            return;
        }

        SceneManager.LoadScene(sceneName.Trim());
    }

    public void GoToDashboard()
    {
        string scene = dashboardSceneName;
        if (PlayerData.Role != null && PlayerData.Role.ToLower() == "teacher")
        {
            scene = "TeacherDashboard";
        }
        LoadScene(scene);
    }

    public void GoToLogin()
    {
        LoadScene(loginSceneName);
    }

    public void SaveMeta()
    {
        SaveSystem.SaveCoins(coins);
        SaveSystem.SaveStars(stars);
        SaveSystem.SaveSelectedMode(selectedMode);
    }

    public void LoadMeta()
    {
        coins = SaveSystem.LoadCoins();
        stars = SaveSystem.LoadStars();
        selectedMode = SaveSystem.LoadSelectedMode();
    }
    }
