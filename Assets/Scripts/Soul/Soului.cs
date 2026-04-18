using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoulUI : MonoBehaviour {
    [SerializeField] private Image soulIcon;
    [SerializeField] private TextMeshProUGUI soulCountText;

    private int totalSouls = 0;

    private void Start()
    {
        if (soulCountText == null)
        {
            GameObject soulCountObj = GameObject.Find("SoulCountText");
            if (soulCountObj != null)
                soulCountText = soulCountObj.GetComponent<TextMeshProUGUI>();
        }

        totalSouls = 20;
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