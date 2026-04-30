using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour {
    [Header("Background")]
    public SpriteRenderer backgroundRenderer;
    public Sprite[] backgroundSprites;
    [SerializeField] private float changeInterval = 4f;

    private float timer;

    void Start()
    {
        if (backgroundSprites != null && backgroundSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, backgroundSprites.Length);
            if (backgroundRenderer != null)
                backgroundRenderer.sprite = backgroundSprites[randomIndex];
        }
    }

    void Update()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0;
            if (backgroundRenderer != null)
            {
                int randomIndex = Random.Range(0, backgroundSprites.Length);
                backgroundRenderer.sprite = backgroundSprites[randomIndex];
            }
        }
    }

    public void OnPlayClicked()
    {
        if (MainMenuMusic.Instance != null)
        {
            MainMenuMusic.Instance.StopMusic();
        }

        SoulUI.TotalSouls = 0;
        PassiveItemManager.ActivePerks.Clear();
        PlayerHealth.IsNewGame = true;

        if (PlayerHealth.Instance != null)
        {
            Destroy(PlayerHealth.Instance.gameObject);
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void OnExitClicked()
    {
        Debug.Log("ВЫХОД нажат!");
        Application.Quit();
    }
}
