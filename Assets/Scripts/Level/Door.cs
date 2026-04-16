using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour {
    [Header("Настройки")]
    public bool isEntrance = false;

    [Header("Визуал")]
    public SpriteRenderer doorSprite;

    private readonly Color COLOR_OPEN = new Color(0.15f, 0.9f, 0.25f, 1f);
    private readonly Color COLOR_ENTRANCE = new Color(0.0f, 0.75f, 0.3f, 1f);

    private BoxCollider2D doorCollider;
    private bool isOpen = false;
    private Coroutine pulseCoroutine;

    void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorSprite == null)
            doorSprite = GetComponent<SpriteRenderer>();
        if (isEntrance) SetOpen();
        else SetClosed();
    }

    public void SetClosed()
    {
        isOpen = false;
        doorCollider.isTrigger = false;
        doorCollider.enabled = true;
        StopPulse();
        if (doorSprite != null) doorSprite.color = Color.white;
    }

    public void SetOpen()
    {
        isOpen = true;
        doorCollider.isTrigger = true;
        doorCollider.enabled = true;
        if (doorSprite != null)
            doorSprite.color = isEntrance ? COLOR_ENTRANCE : COLOR_OPEN;
        if (!isEntrance)
            pulseCoroutine = StartCoroutine(PulseEffect());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpen) return;
        if (!other.CompareTag("Player")) return;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        StopPulse();
        float duration = 0.4f;
        float elapsed = 0f;
        Color startColor = doorSprite.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            doorSprite.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        LevelGenerator.Instance?.LoadNextRoom();
    }

    private IEnumerator PulseEffect()
    {
        Color bright = COLOR_OPEN;
        Color dim = new Color(COLOR_OPEN.r * 0.5f, COLOR_OPEN.g * 0.5f, COLOR_OPEN.b * 0.5f, 1f);
        float speed = 2.5f;
        while (true)
        {
            float t = (Mathf.Sin(Time.time * speed * Mathf.PI) + 1f) / 2f;
            if (doorSprite != null) doorSprite.color = Color.Lerp(dim, bright, t);
            yield return null;
        }
    }

    private void StopPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }
}