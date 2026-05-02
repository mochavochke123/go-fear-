using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour {
    public static PlayerHealth Instance { get; private set; }
    public static bool IsNewGame { get; set; } = false;
    private static float savedHealth = 6f;
    private static float savedMaxHealth = 6f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 6f;
    private float currentHealth;
    private bool isDead = false;
    private bool isInvulnerable = false;

    private Healthcontainer healthcontainer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (IsNewGame)
        {
            currentHealth = maxHealth;
            savedMaxHealth = maxHealth;
            savedHealth = maxHealth;
            IsNewGame = false;
        }
        else
        {
            currentHealth = savedHealth;
            maxHealth = savedMaxHealth;
        }
        
        healthcontainer = FindObjectOfType<Healthcontainer>();

        if (healthcontainer != null)
        {
            healthcontainer.UpdateUI();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(UpdateHealthAfterDelay());
    }

    private IEnumerator UpdateHealthAfterDelay()
    {
        yield return null;
        healthcontainer = FindObjectOfType<Healthcontainer>();
        if (healthcontainer != null)
        {
            healthcontainer.UpdateUI();
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        healthcontainer = FindObjectOfType<Healthcontainer>();
        healthcontainer?.UpdateUI();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        healthcontainer = FindObjectOfType<Healthcontainer>();

        if (PassiveItemManager.Instance?.TryDodge() == true)
        {
            Debug.Log("💨 Уклонение!");
            StartCoroutine(DodgeEffect());
            return;
        }

        currentHealth -= damage;

        var pm = PassiveItemManager.Instance;
        bool hasReflection = pm != null && pm.ReflectionActive;

        if (hasReflection)
        {
            StartCoroutine(ReflectionEffect());
        }

        pm?.OnPlayerDamaged(damage);

        healthcontainer?.UpdateUI();

        var player = FindObjectOfType<Player>();
        player?.OnDamageTaken();

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator DodgeEffect()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    private GameObject shieldEffect;
    private SpriteRenderer shieldSr;

    private IEnumerator ReflectionEffect()
    {
        if (shieldEffect == null)
        {
            shieldEffect = new GameObject("ShieldEffect");
            shieldEffect.transform.SetParent(transform);
            shieldEffect.transform.localPosition = Vector3.zero;
            shieldSr = shieldEffect.AddComponent<SpriteRenderer>();
            shieldSr.sprite = CreateCircleSprite();
            shieldSr.sortingOrder = 10;
            shieldSr.color = new Color(0.5f, 0.5f, 1f, 0.5f);
            shieldEffect.transform.localScale = Vector3.one * 1.5f;
        }

        shieldEffect.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        shieldEffect.SetActive(false);
    }

    private Sprite CreateCircleSprite()
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        Vector2 c = new Vector2(32, 32);
        float r = 30f;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                if (d < r)
                {
                    float a = 1f - (d / r);
                    colors[y * 64 + x] = new Color(1f, 1f, 1f, a);
                }
                else colors[y * 64 + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
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
        healthcontainer = FindObjectOfType<Healthcontainer>();
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth + 1f);
        healthcontainer?.UpdateUI();
    }

    public void AddMaxHP(float amount)
    {
        healthcontainer = FindObjectOfType<Healthcontainer>();
        maxHealth += amount;
        savedMaxHealth += amount;
        currentHealth += amount;
        savedHealth += amount;
        healthcontainer?.UpdateUI();
    }
}