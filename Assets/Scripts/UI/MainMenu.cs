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
            bgRenderer.drawMode = SpriteDrawMode.Sliced;
            bgRenderer.size = new Vector2(20, 12);
            bgRenderer.color = Color.white;
            bgRenderer.sortingOrder = -100;
            
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
            bgRenderer.sprite = null;
            bgRenderer.sprite = backgroundSprites[randomIndex];
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
