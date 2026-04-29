using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMusicManager : MonoBehaviour
{
    public static GameplayMusicManager Instance { get; private set; }

    [Header("Music Clips")]
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip shopMusic;
    public float fadeDuration = 1f;

    private AudioSource mainAudio;
    private AudioSource shopAudio;
    private bool isShopPlaying = false;
    private string currentScene;
    private float shopMusicSavedTime = 0f;
    private bool shopMusicWasPlaying = false;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject); 
            return; 
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        SetupAudioSources();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        PlayCurrentSceneMusic();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayCurrentSceneMusic();
    }

    private void PlayCurrentSceneMusic()
    {
        currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"GameplayMusicManager on scene: {currentScene}");
        
        if (currentScene == "SampleScene")
            PlayLevel1Music();
        else if (currentScene == "Level2")
            PlayLevel2Music();
        else
            StopMusic();
    }

    private void SetupAudioSources()
    {
        if (mainAudio == null)
        {
            mainAudio = gameObject.AddComponent<AudioSource>();
            mainAudio.playOnAwake = false;
            mainAudio.loop = true;
            mainAudio.volume = 1f;
        }

        if (shopAudio == null)
        {
            shopAudio = gameObject.AddComponent<AudioSource>();
            shopAudio.playOnAwake = false;
            shopAudio.loop = true;
            shopAudio.volume = 0f;
        }
        
        Debug.Log("AudioSources created");
    }

    public void PlayLevel1Music()
    {
        if (level1Music == null) return;
        mainAudio.clip = level1Music;
        mainAudio.Play();
    }

    public void PlayLevel2Music()
    {
        if (level2Music == null) return;
        mainAudio.clip = level2Music;
        mainAudio.Play();
    }

    public void PlayShopMusic()
    {
        if (shopMusic == null || isShopPlaying) 
        {
            Debug.LogWarning("PlayShopMusic: shopMusic null or already playing");
            return;
        }
        
        isShopPlaying = true;
        
        mainAudio.volume = 0f;
        
        if (!shopMusicWasPlaying || shopAudio.clip != shopMusic)
        {
            shopAudio.clip = shopMusic;
            shopAudio.time = shopMusicSavedTime;
        }
        
        shopAudio.volume = 1f;
        shopAudio.Play();
        Debug.Log($"PlayShopMusic started (from {shopMusicSavedTime}s)");
    }

    public void StopShopMusic()
    {
        if (!isShopPlaying) 
        {
            Debug.LogWarning("StopShopMusic: not playing");
            return;
        }

        isShopPlaying = false;
        
        shopMusicSavedTime = shopAudio.time;
        shopMusicWasPlaying = true;
        
        Debug.Log($"StopShopMusic called - saved at {shopMusicSavedTime}s");

        shopAudio.Stop();
        
        mainAudio.volume = 1f;
    }

    public void StopMusic()
    {
        mainAudio.Stop();
        shopAudio.Stop();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}