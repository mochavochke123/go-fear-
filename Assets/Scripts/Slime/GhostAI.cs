using UnityEngine;
using System.Collections;

public class GhostAI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 15f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Ghost Ability")]
    [SerializeField] private float vanishDuration = 0.8f;
    [SerializeField] private float vanishCooldown = 4f;

    private float currentHealth;
    private bool isDead = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;
    private Rigidbody2D rb;

    private float attackTimer = 0f;
    private float vanishTimer = 0f;
    private bool isVanishing = false;
    private bool isAppearing = false;
    private bool isAttacking = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        rb.gravityScale = 0f;
    }

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
        vanishTimer = Random.Range(1f, vanishCooldown);
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;
        vanishTimer -= Time.deltaTime;

        if (dist < detectionRange)
        {
            if (spriteRenderer != null)
                spriteRenderer.flipX = player.position.x > transform.position.x;

            if (dist < attackRange && attackTimer <= 0f && !isVanishing)
            {
                StartCoroutine(AttackPlayer());
            }
            else if (dist > attackRange && !isVanishing && !isAttacking)
            {
                MoveToPlayer();
            }

            if (vanishTimer <= 0f && dist < detectionRange * 0.7f)
            {
                StartCoroutine(VanishAndStrike());
                vanishTimer = vanishCooldown;
            }
        }
        else
        {
            if (rb != null)
                rb.velocity = Vector2.zero;

            if (animator != null && !isVanishing && !isAttacking)
                animator.SetBool("isMoving", false);
        }
    }

    private Vector3 GetSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        float minDist = 1.5f;
        int count = 0;

        foreach (GhostAI other in FindObjectsOfType<GhostAI>())
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

        return separation * 1.5f;
    }

    private void MoveToPlayer()
    {
        if (rb == null || isVanishing || isAttacking) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir += GetSeparationVector();
        dir.Normalize();
        rb.velocity = dir * moveSpeed;

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
            animator.SetBool("isVanishing", false);
        }
    }

    private IEnumerator VanishAndStrike()
    {
        isVanishing = true;

        if (animator != null)
            animator.SetBool("isVanishing", true);

        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            while (elapsed < vanishDuration)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - (elapsed / vanishDuration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        }

        if (rb != null)
            rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.2f);

        transform.position = player.position + (transform.position - player.position).normalized * 0.8f;
        transform.position = player.position + (player.position.x < transform.position.x ? Vector3.right : Vector3.left);

        if (spriteRenderer != null)
        {
            yield return new WaitForSeconds(0.1f);

            isVanishing = false;
            if (animator != null)
                animator.SetBool("isVanishing", false);

            yield return new WaitForSeconds(0.05f);

            isAppearing = true;
            if (animator != null)
                animator.SetBool("isAppearing", true);

            float elapsed = 0f;
            while (elapsed < vanishDuration)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, elapsed / vanishDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            spriteRenderer.color = originalColor;

            yield return new WaitForSeconds(0.3f);

            isAppearing = false;
            if (animator != null)
                animator.SetBool("isAppearing", false);
        }

        yield return new WaitForSeconds(0.1f);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < attackRange)
        {
            AttackDirect();
        }

        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        if (animator != null)
            animator.SetBool("isMoving", false);

        MoveToPlayer();
        yield return new WaitForSeconds(0.3f);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < attackRange)
        {
            AttackDirect();
        }

        isAttacking = false;

        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    private void AttackDirect()
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        ph?.TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isVanishing || isAppearing) return;
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
        soulUI?.AddSouls(2);

        GetComponentInParent<RoomManager>()?.OnEnemyDied();
        GetComponentInParent<WaveRoomManager>()?.OnEnemyDied();

        Destroy(gameObject, 1.2f);
    }
}