using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour {
    [Header("Здоровье")]
    [SerializeField] private float maxHealth = 6f;
    private float currentHealth;
    private bool isDead = false;
    private bool isInvulnerable = false;   // ← ДОБАВЛЕНО

    private Healthcontainer healthcontainer;

    private void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"✅ PlayerHealth: HP = {currentHealth}/{maxHealth}");

        healthcontainer = FindObjectOfType<Healthcontainer>();
        Debug.Log($"🔍 Ищу Healthcontainer... Найден: {healthcontainer != null}");

        if (healthcontainer != null)
        {
            Debug.Log($"📍 Healthcontainer найден на: {healthcontainer.gameObject.name}");
            healthcontainer.UpdateUI();
            Debug.Log("✓ Healthcontainer.UpdateUI() вызван");
        }
        else
        {
            Debug.LogError("❌ HEALTHCONTAINER НЕ НАЙДЕН! Проверь имя скрипта и объект в сцене");
        }
    }

    /// <summary>
    /// Получить урон
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        if (PassiveItemManager.Instance?.TryDodge() == true)
        {
            Debug.Log("🌀 Уклонение!");
            return;
        }

        currentHealth -= damage;
        PassiveItemManager.Instance?.OnPlayerDamaged(damage);

        Debug.Log($"💔 Игрок получил {damage} урона! HP: {currentHealth}/{maxHealth}");

        healthcontainer?.UpdateUI();
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Управление неуязвимостью извне (рывок, катсцены и т.д.)
    /// </summary>
    public void SetInvulnerable(bool value)   // ← ДОБАВЛЕНО
    {
        isInvulnerable = value;
        Debug.Log($"🛡️ Неуязвимость: {value}");
    }

    /// <summary>
    /// Красная вспышка при уроне
    /// </summary>
    private IEnumerator FlashDamage()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Смерть игрока
    /// </summary>
    private void Die()
    {
        isDead = true;
        Debug.Log($"💀 Игрок умер!");
        GameReset.Instance?.ShowDeathScreen();
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        healthcontainer?.UpdateUI();
    }

    public void AddMaxHP(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        healthcontainer?.UpdateUI();
    }
}