using UnityEngine;

[DisallowMultipleComponent]
public sealed class MapManager : MonoBehaviour
{
    private static MapManager instance;
    private const string HighestUnlockedStageKey = "MapManager.HighestUnlockedStage";

    public static MapManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MapManager>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("MapManager");
                    instance = managerObject.AddComponent<MapManager>();
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int GetHighestUnlockedStage()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt(HighestUnlockedStageKey, 1));
    }

    public bool IsStageUnlocked(int stageIndex)
    {
        return stageIndex <= GetHighestUnlockedStage();
    }

    public void UnlockStage(int stageIndex)
    {
        int highest = GetHighestUnlockedStage();
        if (stageIndex > highest)
        {
            PlayerPrefs.SetInt(HighestUnlockedStageKey, stageIndex);
            PlayerPrefs.Save();
        }
    }

    public void UnlockNextStage(int currentStageIndex)
    {
        UnlockStage(currentStageIndex + 1);
    }

    public void ResetMapProgress()
    {
        PlayerPrefs.SetInt(HighestUnlockedStageKey, 1);
        PlayerPrefs.Save();
    }
}
