using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour {
    [Header("Health")]
    [SerializeField] private float maxHealth = 6f;
    private float currentHealth;
    private bool isDead = false;
    private bool isInvulnerable = false;

    private Healthcontainer healthcontainer;

    private void Start()
    {
        currentHealth = maxHealth;
        healthcontainer = FindObjectOfType<Healthcontainer>();

        if (healthcontainer != null)
        {
            healthcontainer.UpdateUI();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        if (PassiveItemManager.Instance?.TryDodge() == true)
        {
            return;
        }

        currentHealth -= damage;
        PassiveItemManager.Instance?.OnPlayerDamaged(damage);

        healthcontainer?.UpdateUI();
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
            Die();
    }

    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
    }

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

    private void Die()
    {
        isDead = true;
        GameReset.Instance?.ShowDeathScreen();
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth + 1f);
        healthcontainer?.UpdateUI();
    }

    public void AddMaxHP(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        healthcontainer?.UpdateUI();
    }
}