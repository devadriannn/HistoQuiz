using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// AudioManager handles global BGM and SFX.
[DisallowMultipleComponent]
public sealed class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    Debug.LogWarning("[AudioManager] No instance found in scene. Creating a dummy AudioManager. Note that BGM may not play if bgmClip is not assigned.");
                    GameObject managerObject = new GameObject("AudioManager");
                    instance = managerObject.AddComponent<AudioManager>();
                }
}

            return instance;
        }
    }

    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip levelCompletedClip;
    [SerializeField] private AudioClip wrongAnswerClip;
    [SerializeField] private AudioClip correctAnswerClip;
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
            return;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private bool lastMusicOn = true;

    private void Start()
    {
        UpdateMusicState(true);
        SceneManager.sceneLoaded += OnSceneLoaded;
        AddButtonListenersInActiveScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        // Periodically sync state in case SettingsManager changed it
        UpdateMusicState(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddButtonListenersInActiveScene();
        UpdateMusicState(true);
    }

    private void InitializeAudio()
    {
        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (bgmClip != null && bgmSource.clip != bgmClip)
        {
            bgmSource.clip = bgmClip;
        }
        
        bgmSource.loop = true;
        bgmSource.playOnAwake = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = 1f;

        if (sfxSource == null)
        {
            // Use a separate source for SFX
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length > 1)
            {
                sfxSource = (sources[0] == bgmSource) ? sources[1] : sources[0];
            }
            else
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = 1f;

        UpdateMusicState(true);
    }

    public void AddButtonListenersInActiveScene()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (var btn in buttons)
        {
            if (btn.gameObject.scene.isLoaded)
            {
                btn.onClick.RemoveListener(PlayButtonClickSound);
                btn.onClick.AddListener(PlayButtonClickSound);
            }
        }
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(buttonClickClip);
    }

    public void PlayLevelCompletedSound()
    {
        PlaySFX(levelCompletedClip);
    }

    public void PlayWrongAnswerSound()
    {
        PlaySFX(wrongAnswerClip);
    }

    public void PlayCorrectAnswerSound()
    {
        PlaySFX(correctAnswerClip);
    }

    public void PlaySFX(AudioClip clip)
{
        if (clip == null || sfxSource == null) return;

        bool sfxOn = true;
        if (SettingsManager.Instance != null)
            sfxOn = SettingsManager.Instance.sfxOn;
        else
            sfxOn = PlayerPrefs.GetInt("sfxOn", 1) == 1;

        if (sfxOn)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null) InitializeAudio();
        
        bgmClip = clip;
        bgmSource.clip = clip;
        bgmSource.Play();
        UpdateMusicState(true);
    }

    public void UpdateMusicState()
{
        UpdateMusicState(true);
    }

    public void UpdateMusicState(bool forceUpdate)
    {
        if (bgmSource == null) 
        {
            InitializeAudio();
            if (bgmSource == null) return;
        }

        bool currentMusicOn = true;
        if (SettingsManager.Instance != null)
            currentMusicOn = SettingsManager.Instance.musicOn;
        else
            currentMusicOn = PlayerPrefs.GetInt("musicOn", 1) == 1;

        // Apply mute state and ensure volume is 1
        bgmSource.mute = !currentMusicOn;
        bgmSource.volume = 1f;

        if (currentMusicOn)
        {
            if (bgmSource.clip == null && bgmClip != null)
            {
                bgmSource.clip = bgmClip;
            }

            if (bgmSource.clip != null && !bgmSource.isPlaying)
            {
                bgmSource.Play();
                Debug.Log($"[AudioManager] Music turned ON: '{bgmSource.clip.name}' is now playing.");
            }
        }
        else
        {
            // Even if muted, we let it 'play' so position is kept, 
            // unless we want to stop it to save CPU. Mute is safer.
        }

        lastMusicOn = currentMusicOn;
    }

    public void SetBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource == null) InitializeAudio();

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmClip = clip;
        bgmSource.clip = clip;
        bgmSource.Play();
        UpdateMusicState(true);
    }
}
