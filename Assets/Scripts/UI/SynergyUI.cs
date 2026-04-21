using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SynergyUI : MonoBehaviour
{
    public static SynergyUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject synergyPanel;
    public TextMeshProUGUI titleText;
    public float displayDuration = 3f;

    private Coroutine currentCoroutine;
    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (synergyPanel == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Transform shopPanel = canvas.transform.Find("ShopPanel");
                if (shopPanel != null)
                {
                    Transform synergy = shopPanel.Find("SynergyPanel");
                    if (synergy != null)
                        synergyPanel = synergy.gameObject;
                }
            }
        }

        if (synergyPanel != null)
        {
            originalScale = synergyPanel.transform.localScale;
            synergyPanel.SetActive(false);
            synergyPanel.transform.localScale = Vector3.zero;
        }
    }

    public void ShowSynergy(string title)
    {
        if (synergyPanel == null)
        {
            Debug.Log($"⚔️ SYNERGY: {title}");
            return;
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(ShowSynergyRoutine(title));
    }

    public static void Show(string title)
    {
        if (Instance != null)
            Instance.ShowSynergy(title);
        else
            Debug.Log($"⚔️ SYNERGY: {title}");
    }

    private IEnumerator ShowSynergyRoutine(string title)
    {
        if (titleText != null)
            titleText.text = title;

        synergyPanel.SetActive(true);

        float elapsed = 0;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            synergyPanel.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        synergyPanel.transform.localScale = originalScale;

        yield return new WaitForSeconds(displayDuration);

        elapsed = 0;
        while (elapsed < duration)
        {
            synergyPanel.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        synergyPanel.transform.localScale = Vector3.zero;
        synergyPanel.SetActive(false);
    }
}