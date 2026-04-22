using TMPro;
using UnityEngine;

public class AboutUI : MonoBehaviour
{
    public TMP_Text aboutText;
    public SceneLoader loader;

    private void Start()
    {
        if (aboutText != null)
        {
            aboutText.text =
                "HistoQuiz is an educational quiz game about popular Philippine heroes.\n\n" +
                "Modes:\n" +
                "- 4 Pics\n" +
                "- True or False\n" +
                "- Multiple Choice\n\n" +
                "Version 1.0\n" +
                "Developer: Your Team";
        }
    }

    public void Back()
    {
        loader.LoadScene("StudentDashboard");
    }
}