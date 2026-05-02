using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public static MainMenuMusic Instance { get; private set; }

    private AudioSource audioSource;
    private static AudioClip cachedMenuMusic;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }
        
        if (cachedMenuMusic == null)
        {
            cachedMenuMusic = Resources.Load<AudioClip>("Audio/Music/MainMenuMusic");
        }
        
        if (cachedMenuMusic != null && audioSource.clip == null)
        {
            audioSource.clip = cachedMenuMusic;
        }
        
        if (audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.loop = true;
                }
            }
            
            if (cachedMenuMusic == null)
            {
                cachedMenuMusic = Resources.Load<AudioClip>("Audio/Music/MainMenuMusic");
            }
            
            if (cachedMenuMusic != null && audioSource.clip == null)
            {
                audioSource.clip = cachedMenuMusic;
            }
            
            if (audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PlayMusic()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    public bool IsPlaying() => audioSource != null && audioSource.isPlaying;
}