using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [Header("Background")]
    public GameObject backgroundObject;
    public Sprite[] backgroundSprites;
    [SerializeField] private float changeInterval = 4f;

    private SpriteRenderer bgRenderer;
    private float timer;

    void Start()
    {
        Debug.Log("MainMenu Start! BackgroundObject: " + backgroundObject + ", Sprites count: " + (backgroundSprites?.Length ?? 0));
        
        if (backgroundObject != null)
        {
            bgRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            if (bgRenderer == null)
            {
                Debug.Log("No SpriteRenderer on backgroundObject, checking children...");
                bgRenderer = backgroundObject.GetComponentInChildren<SpriteRenderer>();
            }
        }
        
        Debug.Log("bgRenderer found: " + bgRenderer);

        if (bgRenderer != null && backgroundSprites != null && backgroundSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, backgroundSprites.Length);
            bgRenderer.sprite = backgroundSprites[randomIndex];
            Debug.Log("Set background sprite to: " + backgroundSprites[randomIndex].name);
        }
        else
        {
            Debug.Log("FAILED: bgRenderer=" + bgRenderer + ", backgroundSprites=" + (backgroundSprites?.Length ?? 0));
        }
    }

    void Update()
    {
        if (bgRenderer == null || backgroundSprites == null || backgroundSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0;
            int randomIndex = Random.Range(0, backgroundSprites.Length);
            bgRenderer.sprite = backgroundSprites[randomIndex];
            Debug.Log("Changed background to sprite: " + backgroundSprites[randomIndex].name);
        }
    }

    public void OnPlayClicked()
    {
        if (MainMenuMusic.Instance != null)
            MainMenuMusic.Instance.StopMusic();

        SoulUI.TotalSouls = 0;
        PassiveItemManager.ActivePerks.Clear();
        PlayerHealth.IsNewGame = true;

        if (PlayerHealth.Instance != null)
            Destroy(PlayerHealth.Instance.gameObject);

        SceneManager.LoadScene("SampleScene");
    }

    public void OnExitClicked()
    {
        Debug.Log("ВЫХОД нажат!");
        Application.Quit();
    }
}
