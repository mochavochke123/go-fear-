using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoulUI : MonoBehaviour {
    [SerializeField] private Image soulIcon; // Спрайт фиолетовой души
    [SerializeField] private TextMeshProUGUI soulCountText; // Число рядом

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

        Debug.Log($"🔍 SoulUI инициализирован. SoulCountText найден: {soulCountText != null}");
        UpdateDisplay();
    }

    /// <summary>
    /// Добавить души
    /// </summary>
    public void AddSouls(int amount)
    {
        totalSouls += amount;
        Debug.Log($"💜 SOULS +{amount}! Всего: {totalSouls}");
        UpdateDisplay();
    }

    /// <summary>
    /// Обновить отображение
    /// </summary>
    private void UpdateDisplay()
    {
        if (soulCountText != null)
        {
            soulCountText.text = totalSouls.ToString();
            Debug.Log($"📊 UI обновлён: {totalSouls}");
        }
        else
        {
            Debug.LogError($"❌ soulCountText не назначен!");
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