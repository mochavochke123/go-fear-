using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    [Header("Background")]
    public GameObject backgroundObject;
    public Sprite[] backgroundSprites;
    [SerializeField] private float changeInterval = 4f;

    private SpriteRenderer bgRenderer;
    private float timer;
    private bool initialized = false;

    void Start()
    {
        if (backgroundObject != null)
        {
            bgRenderer = backgroundObject.GetComponent<SpriteRenderer>();
            if (bgRenderer == null)
                bgRenderer = backgroundObject.GetComponentInChildren<SpriteRenderer>();
        }

        if (bgRenderer != null)
        {
            bgRenderer.drawMode = SpriteDrawMode.Tiled;
            bgRenderer.size = new Vector2(551.4796f, 324.839f);
            bgRenderer.color = Color.white;
            bgRenderer.sortingOrder = -100;
            
            // Force refresh
            bgRenderer.enabled = false;
            bgRenderer.enabled = true;
            
            if (backgroundSprites != null && backgroundSprites.Length > 0)
            {
                initialized = true;
            }
        }
    }

    void Update()
    {
        if (!initialized || bgRenderer == null || backgroundSprites == null || backgroundSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0;
            int randomIndex = Random.Range(0, backgroundSprites.Length);
            
            // Temporarily disable to force texture reload
            bgRenderer.enabled = false;
            bgRenderer.sprite = backgroundSprites[randomIndex];
            bgRenderer.enabled = true;
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
