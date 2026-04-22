using UnityEngine;

public class ModeSelectUI : MonoBehaviour
{
    public SceneLoader loader;

    public void SelectFourPics()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetMode("FourPics");

        loader.LoadScene("Questions");
    }

    public void SelectTrueFalse()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetMode("TrueFalse");

        loader.LoadScene("Questions");
    }

    public void SelectMultipleChoice()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetMode("MultipleChoice");

        loader.LoadScene("Questions");
    }

    public void Back()
    {
        loader.LoadScene("StudentDashboard");
    }
}