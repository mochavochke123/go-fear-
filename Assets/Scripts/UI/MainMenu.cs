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
        if (backgroundObject != null)
            bgRenderer = backgroundObject.GetComponent<SpriteRenderer>();

        if (bgRenderer != null && backgroundSprites != null && backgroundSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, backgroundSprites.Length);
            bgRenderer.sprite = backgroundSprites[randomIndex];
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
