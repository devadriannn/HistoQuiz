using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToDashboard : MonoBehaviour
{
    [SerializeField] private string dashboardSceneName = "StudentDashboard";

    public void GoBack()
    {
        SceneManager.LoadScene(dashboardSceneName);
    }
}
