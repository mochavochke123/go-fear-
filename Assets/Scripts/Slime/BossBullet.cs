using UnityEngine;
using System.Collections;

public class BossBullet : MonoBehaviour
{
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
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > aggroRange) return;

        meleeTimer -= Time.deltaTime;
        bulletTimer -= Time.deltaTime;
        chillTimer -= Time.deltaTime;

        // Chill состояние
        if (chillTimer > 0f)
        {
            if (animator != null)
            {
                animator.SetBool("isChill", true);
                animator.SetBool("isWalking", false);
                Debug.Log("👹 ВКЛЮЧЕН CHILL!");
            }
            return;
        }

        if (animator != null)
        {
            animator.SetBool("isChill", false);
        }

        if (isAttacking) return;

        // Рандомный выбор атаки
        if (dist < meleeRange && meleeTimer <= 0f)
        {
            Debug.Log("👹 MELEE АТАКА!");
            StartCoroutine(MeleeAttack());
            return;
        }

        if (dist < bulletRange && bulletTimer <= 0f)
        {
            Debug.Log("👹 BULLET АТАКА!");
            StartCoroutine(BulletAttack());
            return;
        }

        if (bulletFTimer <= 0f)
        {
            Debug.Log("👹 BULLETF АТАКА!");
            StartCoroutine(BulletFAttack());
            return;
        }

        ChasePlayer();
    }

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
            animator.SetTrigger("isAttacking1");
            animator.SetBool("isChill", false);
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

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < meleeRange + 1f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            ph?.TakeDamage(meleeDamage);
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        meleeTimer = meleeCooldown;
        chillTimer = chillDuration;
        
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isChill", false);
            animator.Play("MimicChase");
            animator.ResetTrigger("isAttacking1");
        }
        
        Debug.Log("👹 CHILL ПОСЛЕ MELEE!");
    }

    private IEnumerator BulletAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("isAttacking2");
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
            animator.SetBool("isWalking", true);
            animator.SetBool("isChill", false);
            animator.Play("MimicChase");
            animator.ResetTrigger("isAttacking2");
        }
        Debug.Log("👹 CHILL ПОСЛЕ BULLET!");
    }

    private IEnumerator BulletFAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("isChill");
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
                {
                    rb.gravityScale = 0.5f;
                }
                
                BulletFProjectile projectile = bulletF.GetComponent<BulletFProjectile>();
                if (projectile != null)
                {
                    projectile.Initialize(bulletFDamage);
                }
            }
            
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        bulletFTimer = bulletFCooldown;
        chillTimer = chillDuration;
        
        if (animator != null)
        {
            animator.SetBool("isChill", false);
            animator.SetBool("isWalking", true);
            animator.Play("MimicChase");
        }
        
        Debug.Log("👹 CHILL ПОСЛЕ BULLETF!");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        Debug.Log($"👹 BossBullet получил урон: {damage} | HP: {currentHealth}");
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

    private bool hasDied = false;

    private void Die()
    {
        if (hasDied) return;
        hasDied = true;
        isDead = true;

        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        if (animator != null)
            animator.SetTrigger("isDead");

        PassiveItemManager.Instance?.OnEnemyKilled(gameObject);

        SoulUI soulUI = FindObjectOfType<SoulUI>();
        if (soulUI != null) soulUI.AddSouls(15);

        StartCoroutine(DestroyAfterDeath());
        StartCoroutine(ShowVictoryDelayed());
    }

    private IEnumerator ShowVictoryDelayed()
    {
        yield return new WaitForSeconds(3f);
        GameReset.Instance?.ShowVictory();
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}