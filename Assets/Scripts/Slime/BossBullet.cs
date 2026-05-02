using UnityEngine;
using System.Collections;

public class BossBullet : MonoBehaviour {
    [Header("Stats")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Attack 1 - Ближний удар")]
    [SerializeField] private float meleeDamage = 1f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeCooldown = 2f;
    [SerializeField] private GameObject meleeBulletPrefab;

    [Header("Attack 2 - Выстрел")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletDamage = 0.5f;
    [SerializeField] private float bulletCooldown = 3f;
    [SerializeField] private float bulletSpeed = 8f;

    [Header("Attack 3 - Падающие пули")]
    [SerializeField] private GameObject bulletFPrefab;
    [SerializeField] private float bulletFDamage = 0.5f;
    [SerializeField] private float bulletFCooldown = 6f;

    [Header("Rage - Фаза ярости (50% HP)")]
    [SerializeField] private float rageDuration = 7f;        // сколько длится ярость
    [SerializeField] private float rageSpawnInterval = 0.4f; // как часто спавнить bulletF во время ярости
    [SerializeField] private float rageBulletFDamage = 0.3f; // урон пуль во время ярости

    [Header("Chill режим")]
    [SerializeField] private float chillDuration = 0.5f;

    [Header("Aggro")]
    [SerializeField] private float aggroRange = 20f;
    [SerializeField] private float bulletRange = 8f;

    [Header("Death")]
    [SerializeField] private float deathDelay = 1.5f;

    private float currentHealth;
    private bool isDead = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    private float meleeTimer = 0f;
    private float bulletTimer = 0f;
    private float bulletFTimer = 0f;
    private float chillTimer = 0f;
    private bool isAttacking = false;

    private bool rageTriggered = false; // флаг — ярость уже была или нет
    private bool isRaging = false;      // флаг — сейчас в режиме ярости

    private SoulUI soulUI;

    private void Start()
    {
        var pm = PassiveItemManager.Instance;
        maxHealth *= pm?.enemyHealthMultiplier ?? 1f;
        currentHealth = maxHealth;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        soulUI = FindObjectOfType<SoulUI>();
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > aggroRange) return;

        // Проверка на 50% HP — запускаем ярость один раз
        if (!rageTriggered && !isRaging && currentHealth <= maxHealth * 0.5f)
        {
            rageTriggered = true;
            StartCoroutine(RageAttack());
            return;
        }

        // Во время ярости Update не управляет атаками
        if (isRaging) return;

        meleeTimer -= Time.deltaTime;
        bulletTimer -= Time.deltaTime;
        bulletFTimer -= Time.deltaTime;
        chillTimer -= Time.deltaTime;

        if (chillTimer > 0f)
        {
            if (animator != null)
            {
                animator.SetBool("isChill", true);
                animator.SetBool("isWalking", false);
            }
            return;
        }

        if (animator != null)
            animator.SetBool("isChill", false);

        if (isAttacking) return;

        if (dist < meleeRange && meleeTimer <= 0f)
        {
            StartCoroutine(MeleeAttack());
            return;
        }

        if (dist < bulletRange && bulletTimer <= 0f)
        {
            StartCoroutine(BulletAttack());
            return;
        }

        if (bulletFTimer <= 0f)
        {
            StartCoroutine(BulletFAttack());
            return;
        }

        ChasePlayer();
    }

    // ===================== RAGE ATTACK =====================
    private IEnumerator RageAttack()
    {
        isRaging = true;
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isChill", false);
            animator.SetTrigger("isAttacking2");
        }

        float timer = 0f;

        while (timer < rageDuration)
        {
            if (player != null && bulletFPrefab != null)
            {
                // Спавним 2 пули с разбросом каждый интервал
                for (int i = 0; i < 2; i++)
                {
                    Vector3 targetPos = player.position;
                    Vector3 spawnPos = targetPos + Vector3.up * 10f + new Vector3(Random.Range(-4f, 4f), 0, 0);

                    GameObject bulletF = Instantiate(bulletFPrefab, spawnPos, Quaternion.identity);

                    Rigidbody2D rb = bulletF.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.gravityScale = 0.5f;

                    bulletF.GetComponent<BulletFProjectile>()?.Initialize(rageBulletFDamage);
                }
            }

            yield return new WaitForSeconds(rageSpawnInterval);
            timer += rageSpawnInterval;
        }

        // Ярость закончилась
        isAttacking = false;
        isRaging = false;
        chillTimer = 1.5f; // небольшой отдых после ярости

        if (animator != null)
        {
            animator.SetBool("isChill", true);
            animator.SetBool("isWalking", false);
        }
    }

    // ===================== ОБЫЧНЫЕ АТАКИ =====================
    private void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0;

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isChill", false);
        }
    }

    private IEnumerator MeleeAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isChill", false);
            animator.SetTrigger("isAttacking1");
        }

        yield return new WaitForSeconds(0.3f);

        if (meleeBulletPrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = -30f + i * 30f;
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                Instantiate(meleeBulletPrefab, transform.position, rot);
            }
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        meleeTimer = meleeCooldown;
        chillTimer = chillDuration;

        if (animator != null)
        {
            animator.SetBool("isChill", true);
            animator.SetBool("isWalking", false);
        }
    }

    private IEnumerator BulletAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("isAttacking1");
        }

        yield return new WaitForSeconds(0.3f);

        if (bulletPrefab != null)
        {
            Vector3 baseDir = (player.position - transform.position).normalized;

            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
                Vector3 dir = (baseDir + offset).normalized;

                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<BossBulletProjectile>()?.Initialize(dir, bulletDamage, bulletSpeed);

                if (i < 2)
                    yield return new WaitForSeconds(0.15f);
            }
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        bulletTimer = bulletCooldown;
        chillTimer = chillDuration;

        if (animator != null)
        {
            animator.SetBool("isChill", true);
            animator.SetBool("isWalking", false);
        }
    }

    private IEnumerator BulletFAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("isAttacking2");
        }

        if (player == null)
        {
            isAttacking = false;
            bulletFTimer = bulletFCooldown;
            yield break;
        }

        Vector3 targetPos = player.position;
        float startY = targetPos.y + 10f;

        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPos = targetPos + Vector3.up * startY + new Vector3(Random.Range(-3f, 3f), 0, 0);

            if (bulletFPrefab != null)
            {
                GameObject bulletF = Instantiate(bulletFPrefab, spawnPos, Quaternion.identity);

                Rigidbody2D rb = bulletF.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.gravityScale = 0.5f;

                bulletF.GetComponent<BulletFProjectile>()?.Initialize(bulletFDamage);
            }

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        bulletFTimer = bulletFCooldown;
        chillTimer = chillDuration;

        if (animator != null)
        {
            animator.SetBool("isChill", true);
            animator.SetBool("isWalking", false);
        }
    }

    // ===================== УРОН / СМЕРТЬ =====================
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        if (animator != null)
            animator.SetTrigger("isDead");

        PassiveItemManager.Instance?.OnEnemyKilled(gameObject);

        if (soulUI != null)
            soulUI.AddSouls(15);

        StartCoroutine(ShowVictoryDelayed());
    }

    private IEnumerator ShowVictoryDelayed()
    {
        yield return new WaitForSeconds(3f);
        GameReset.Instance?.ShowVictory();
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}