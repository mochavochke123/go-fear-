using UnityEngine;
using System.Collections;

public class MimicAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 80f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float awakeRange = 5f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 8f;
    [SerializeField] private float dashRange = 4f;
    [SerializeField] private float dashCooldown = 6f;

    private float currentHealth;
    private bool isDead = false;
    private bool isAwake = false;
    private bool isAttacking = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float attackTimer = 0f;
    private float dashTimer = 0f;

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

        walkTimer = 0f;
        dashTimer = dashCooldown;
    }

    private float walkTimer = 0f;

    private void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (!isAwake)
        {
            if (dist < awakeRange)
            {
                isAwake = true;
                if (animator != null) animator.SetTrigger("isAwake");
            }
            return;
        }

        attackTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        walkTimer -= Time.deltaTime;

        if (dist < attackRange)
        {
            TryAttack();
            return;
        }

        if (dashTimer <= 0f)
        {
            StartCoroutine(DashMovement());
        }
        else
        {
            ChasePlayer();
        }

        if (spriteRenderer != null)
            spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    private void ChasePlayer()
    {
        if (walkTimer > 0f) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        if (animator != null)
            animator.SetBool("isChasing", true);
    }

    private IEnumerator DashMovement()
    {
        walkTimer = 3f;
        isAttacking = true;

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetTrigger("isAttacking");
        }

        Vector2 startPos = transform.position;
        Vector2 dashDir = (player.position - transform.position).normalized;
        float traveled = 0f;

        while (traveled < dashRange)
        {
            transform.position = startPos + dashDir * traveled;
            traveled += dashSpeed * Time.deltaTime;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < 2f)
            {
                DealDamageToPlayer();
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        walkTimer = 0f;
        isAttacking = false;
        dashTimer = dashCooldown;
    }

    private void TryAttack()
    {
        if (attackTimer > 0f || isAttacking) return;

        StartCoroutine(BiteAttack());
    }

    private IEnumerator DashAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        dashTimer = dashCooldown;

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetTrigger("isAttacking");
        }

        Vector2 startPos = transform.position;
        Vector2 dashDir = (player.position - transform.position).normalized;
        float traveled = 0f;

        while (traveled < dashRange)
        {
            transform.position = startPos + dashDir * traveled;
            traveled += dashSpeed * Time.deltaTime;

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist < 2f)
            {
                DealDamageToPlayer();
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    private IEnumerator BiteAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        if (animator != null)
        {
            animator.SetBool("isChasing", false);
            animator.SetTrigger("isAttacking");
        }

        yield return new WaitForSeconds(0.3f);

        DealDamageToPlayer();

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    private void DealDamageToPlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < 3f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            ph?.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0) Die();
    }

    private Color originalColor;

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
        if (soulUI != null) soulUI.AddSouls(10);

        GetComponentInParent<RoomManager>()?.OnEnemyDied();
        GetComponentInParent<WaveRoomManager>()?.OnEnemyDied();

        UnlockDoors();
    }

    private void UnlockDoors()
    {
        var doors = FindObjectsOfType<LockedDoor>();
        Debug.Log($"🚪 Найдено дверей: {doors.Length}");
        foreach (var door in doors)
        {
            door.Unlock();
        }
    }
}