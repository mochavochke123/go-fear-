using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameReset : MonoBehaviour {
    public static GameReset Instance { get; private set; }

    [Header("Death UI")]
    public GameObject deathScreen;

    [Header("Victory UI")]
    public TextMeshProUGUI victoryText;

    [Header("Error UI")]
    public GameObject errorPanel;
    public TextMeshProUGUI errorText;
    public Button dismissButton;
    public Button restartButton;

    [Header("Settings")]
    [SerializeField] private float delayBeforeMenu = 2f;

    private string lastError = "";
    private bool errorShown = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        HideDeathScreen();
        HideError();
        if (victoryText != null) victoryText.gameObject.SetActive(false);
        
        Application.logMessageReceived += HandleLog;
    }

    private void Start()
    {
        if (dismissButton != null)
            dismissButton.onClick.AddListener(DismissError);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartScene);
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            if (!errorShown)
            {
                lastError = $"{logString}\n\n{stackTrace}";
                ShowError(lastError);
            }
        }
    }

    public void ShowError(string message)
    {
        if (errorPanel == null || errorText == null) return;
        
        errorShown = true;
        errorText.text = message;
        errorPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideError()
    {
        if (errorPanel != null)
            errorPanel.SetActive(false);
        
        errorShown = false;
        lastError = "";
        Time.timeScale = 1f;
    }

    public void DismissError()
    {
        HideError();
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

    public void ShowVictory()
    {
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            
            AudioSource audio = victoryText.GetComponent<AudioSource>();
            if (audio != null)
                audio.Play();
        }

        StartCoroutine(VictoryRoutine());
    }

public void HideVictory()
    {
        if (victoryText != null)
            victoryText.gameObject.SetActive(false);
    }

    private IEnumerator VictoryRoutine()
    {
        yield return new WaitForSecondsRealtime(3f);
        
        if (victoryText != null)
            victoryText.gameObject.SetActive(false);
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator AutoReturnToMenu()
    {
        yield return new WaitForSecondsRealtime(delayBeforeMenu);

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
