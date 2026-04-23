using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireSkeletAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 60f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Attack")]
    [SerializeField] private float aggroRange = 15f;
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileCount = 8;

    private bool isAggro = false;

    private float currentHealth;
    private bool isDead = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    private float attackTimer = 0f;
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

        if (dist < aggroRange && !isAggro)
        {
            isAggro = true;
            Debug.Log("👁 FireSkelet: AGGRO!");
        }

        attackTimer -= Time.deltaTime;

        if (dist < attackRange && attackTimer <= 0f && !isAttacking && isAggro)
        {
            Debug.Log("🔥 FireSkelet: АТАКА!");
            if (animator != null)
                animator.SetBool("isWalking", false);
            StartCoroutine(FlameAttack());
            return;
        }

        if (!isAttacking && dist > 1.5f && isAggro)
        {
            ChasePlayer();
        }
        else if (!isAttacking && !isAttacking)
        {
            if (animator != null)
                animator.SetBool("isWalking", false);
        }
    }

    private void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x > 0;

        if (animator != null)
            animator.SetBool("isWalking", true);
    }

    private IEnumerator FlameAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("isAttacking");
        }

        yield return new WaitForSeconds(0.5f);

        SpawnFlameProjectiles();

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
        attackTimer = attackCooldown;
    }

    private void SpawnFlameProjectiles()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("⚠️ Projectile Prefab не назначен!");
            return;
        }

        Debug.Log($"🔥 Спавню {projectileCount} проджектайлов!");
        float angleStep = 360f / projectileCount;

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            proj.GetComponent<FireProjectile>()?.Initialize(dir);
        }

        StartCoroutine(FireAura());
    }

    private IEnumerator FireAura()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        Debug.Log($"🔥 FireSkelet получил урон: {damage} | HP: {currentHealth}");
        currentHealth -= damage;
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0) Die();
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
        if (soulUI != null) soulUI.AddSouls(2);

        GetComponentInParent<RoomManager>()?.OnEnemyDied();
        GetComponentInParent<WaveRoomManager>()?.OnEnemyDied();

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}