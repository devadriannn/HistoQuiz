using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager instance;

    public static LeaderboardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LeaderboardManager>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("LeaderboardManager");
                    instance = managerObject.AddComponent<LeaderboardManager>();
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

    public PlayerProgress GetPlayerProgress()
    {
        return SaveSystem.LoadProgress();
    }
    }
