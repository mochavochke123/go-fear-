using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoulUI : MonoBehaviour {
    public static SoulUI Instance { get; private set; }
    public static int TotalSouls = 0;

    [SerializeField] private Image soulIcon;
    [SerializeField] private TextMeshProUGUI soulCountText;

    private bool initialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if (!initialized)
        {
            FindSoulText();
            UpdateDisplay();
            initialized = true;
        }
    }

    private void FindSoulText()
    {
        if (soulCountText == null)
        {
            GameObject soulCountObj = GameObject.Find("SoulCountText");
            if (soulCountObj != null)
                soulCountText = soulCountObj.GetComponent<TextMeshProUGUI>();
        }
    }

    public void AddSouls(int amount)
    {
        TotalSouls += amount;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        FindSoulText();
        if (soulCountText != null)
        {
            soulCountText.text = TotalSouls.ToString();
        }
    }

    public int GetTotalSouls() => TotalSouls;
    public int GetSouls() => TotalSouls;

    public void SpendSouls(int amount)
    {
        TotalSouls -= amount;
        UpdateDisplay();
    }
}