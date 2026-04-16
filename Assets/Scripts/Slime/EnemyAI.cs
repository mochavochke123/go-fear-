using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour {
    [Header("Здоровье")]
    [SerializeField] private float maxHealth = 30f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Враг")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackDamage = 0.5f;
    [SerializeField] private float attackCooldown = 1f;
    private float lastAttackTime = 0;

    [Header("Ссылки")]
    private Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private NavMeshAgent agent;
    private Color originalColor;
    private float distanceToPlayer;

    // Состояния
    private bool isRoaming = true;
    private bool isChasing = false;
    private bool isAttacking = false;

    private void Start()
    {
        var pm = PassiveItemManager.Instance;
        maxHealth *= pm?.enemyHealthMultiplier ?? 1f; // Ужас
        currentHealth = maxHealth;

        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (agent != null)
            agent.enabled = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetRoamingState();
        Debug.Log($"✨ {gameObject.name} спавнился!");
    }

    /// <summary>
    /// Создаёт видимые радиусы
    /// </summary>


    /// <summary>
    /// Рисует круг LineRenderer
    /// </summary>
    private void DrawRadiusLine(LineRenderer lineRenderer, float radius, int segments)
    {
        Vector3[] positions = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            positions[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0.1f);
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    /// <summary>
    /// Белая вспышка при спавне
    /// </summary>
    private IEnumerator SpawnFlash()
    {
        if (spriteRenderer == null)
            yield break;

        // Белая вспышка - очень яркая чтобы видно было
        spriteRenderer.color = Color.white;
        Debug.Log($"✨✨✨ {gameObject.name} БЕЛАЯ ВСПЫШКА ✨✨✨");

        yield return new WaitForSeconds(0.25f);

        spriteRenderer.color = originalColor;
    }

    private void Update()
    {
        if (isDead || player == null)
            return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Обнаруживаем игрока
        if (distanceToPlayer < detectionRange)
        {
            // Двигаемся к игроку
            if (distanceToPlayer > attackRange)
            {
                SetChasingState();
                MoveToPlayer();
            }
            else
            {
                // Атакуем если близко
                SetAttackingState();
                AttackPlayer();
            }
        }
        else
        {
            // Враг далеко - возвращаемся к бродячеству
            if (isChasing || isAttacking)
            {
                SetRoamingState();
            }
        }
    }

    private void SetRoamingState()
    {
        if (isRoaming)
            return;

        isRoaming = true;
        isChasing = false;
        isAttacking = false;

        UpdateAnimator();
    }

    private void SetChasingState()
    {
        if (isChasing)
            return;

        isRoaming = false;
        isChasing = true;
        isAttacking = false;

        UpdateAnimator();
    }

    private void SetAttackingState()
    {
        if (isAttacking)
            return;

        isRoaming = false;
        isChasing = false;
        isAttacking = true;

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("isRoaming", isRoaming);
            animator.SetBool("isChasing", isChasing);
            animator.SetBool("isAttacking", isAttacking);
        }
    }

    private void MoveToPlayer()
    {
        if (rb == null) return;

        var pm = PassiveItemManager.Instance;
        float speed = moveSpeed * (pm?.enemySlowMultiplier ?? 1f);

        Vector3 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < 0;
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

    private void AttackPlayer()
    {
        lastAttackTime -= Time.deltaTime;
        if (lastAttackTime <= 0 && player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                lastAttackTime = attackCooldown;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
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
        if (animator != null)
            animator.SetTrigger("isDead");

        PassiveItemManager.Instance?.OnEnemyKilled(gameObject); // добавь это

        SoulUI soulUI = FindObjectOfType<SoulUI>();
        if (soulUI != null)
            soulUI.AddSouls(1);

        GetComponentInParent<RoomManager>()?.OnEnemyDied();
        Destroy(gameObject, 1f);
    }
}