using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SoulUI : MonoBehaviour {
    public static SoulUI Instance { get; private set; }

    [SerializeField] private Image soulIcon;
    [SerializeField] private TextMeshProUGUI soulCountText;

    private int totalSouls = 0;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject soulCountObj = GameObject.Find("SoulCountText");
        if (soulCountObj != null)
            soulCountText = soulCountObj.GetComponent<TextMeshProUGUI>();
        UpdateDisplay();
    }

    private void Start()
    {
        if (soulCountText == null)
        {
            GameObject soulCountObj = GameObject.Find("SoulCountText");
            if (soulCountObj != null)
                soulCountText = soulCountObj.GetComponent<TextMeshProUGUI>();
        }

        UpdateDisplay();
    }

    public void AddSouls(int amount)
    {
        totalSouls += amount;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (soulCountText != null)
        {
            soulCountText.text = totalSouls.ToString();
        }
    }

    public int GetTotalSouls() => totalSouls;
    public int GetSouls() => totalSouls;

    public void SpendSouls(int amount)
    {
        totalSouls -= amount;
        UpdateDisplay();
    }
}