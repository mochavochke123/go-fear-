using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameReset : MonoBehaviour {
    public static GameReset Instance { get; private set; }

    [Header("UI")]
    public GameObject deathScreen;

    [Header("Settings")]
    [SerializeField] private float delayBeforeMenu = 2f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        HideDeathScreen();
    }

    public void ShowDeathScreen()
    {
        if (deathScreen != null)
            deathScreen.SetActive(true);

        StartCoroutine(AutoReturnToMenu());
    }

    public void HideDeathScreen()
    {
        if (deathScreen != null)
            deathScreen.SetActive(false);
    }

    private IEnumerator AutoReturnToMenu()
    {
        yield return new WaitForSecondsRealtime(delayBeforeMenu);

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
