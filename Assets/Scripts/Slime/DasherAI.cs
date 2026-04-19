using UnityEngine;
using System.Collections;

public class DasherAI : MonoBehaviour {
    [Header("��������")]
    [SerializeField] private float maxHealth = 20f;
    private float currentHealth;
    private bool isDead = false;

    [Header("��������")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float dashTowardChance = 0.7f;

    [Header("�����")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackDamage = 0.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float detectionRange = 7f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private float attackTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        dashCooldownTimer = Random.Range(0.5f, dashCooldown);
    }

    void Update()
    {
        if (isDead || player == null || isDashing) return;

        float dist = Vector3.Distance(transform.position, player.position);

        attackTimer -= Time.deltaTime;
        dashCooldownTimer -= Time.deltaTime;

        if (dist < detectionRange)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = player.position.x < transform.position.x;

            if (dist < attackRange && attackTimer <= 0f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                ph?.TakeDamage(attackDamage);
                attackTimer = attackCooldown;
            }

            if (dashCooldownTimer <= 0f)
            {
                bool toward = Random.value < dashTowardChance;
                StartCoroutine(Dash(toward));
                dashCooldownTimer = dashCooldown;
            }
            else if (dist > attackRange)
            {
                MoveTowardsPlayer();
            }
        }
        else
        {
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (rb == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir += GetSeparationVector();
        dir.Normalize();
        rb.velocity = dir * moveSpeed;
    }

    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private IEnumerator Dash(bool toward)
    {
        isDashing = true;
        animator?.SetBool("isDashing", true);

        Vector3 dir = (player.position - transform.position).normalized;
        if (!toward) dir = -dir;

        float elapsed = 0f;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.cyan;

        while (elapsed < dashDuration)
        {
            transform.position += dir * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        animator?.SetBool("isDashing", false);
        isDashing = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        StartCoroutine(FlashDamage());
        if (currentHealth <= 0) Die();
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;
        animator?.SetTrigger("isDead");

        PassiveItemManager.Instance?.OnEnemyKilled(gameObject);

        SoulUI soulUI = FindObjectOfType<SoulUI>();
        soulUI?.AddSouls(1);

GetComponentInParent<RoomManager>()?.OnEnemyDied();
        Destroy(gameObject, 1f);
    }

    private Vector3 GetSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        float minDist = 1.2f;
        int count = 0;

        foreach (DasherAI other in FindObjectsOfType<DasherAI>())
        {
            if (other == this || other == null) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < minDist && dist > 0)
            {
                Vector3 pushDir = (transform.position - other.transform.position).normalized;
                separation += pushDir / dist;
                count++;
            }
        }

        if (count > 0)
            separation /= count;

        return separation * 1.2f;
    }
}

