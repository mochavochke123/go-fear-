using UnityEngine;
using System.Collections;

public class MinionAI : MonoBehaviour {
    [Header("Параметры")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Респавн")]
    [SerializeField] private float respawnDelay = 3f;

    private Transform player;
    private Transform target;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isRespawning = false;

    private SpriteRenderer spriteRenderer;
    private PassiveItemManager passiveManager;

    void Start()
    {
        player = Player.Instance?.transform;
        passiveManager = PassiveItemManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player != null)
        {
            transform.position = GetSpawnPosition();
        }

        Debug.Log($"[MinionAI] Дракончик {gameObject.name} заспавнен!");
    }

    void Update()
    {
        if (isDead || player == null) return;

        attackTimer -= Time.deltaTime;

        if (target != null)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            if (distToTarget <= detectionRange)
            {
                if (distToTarget > attackRange)
                {
                    MoveToTarget();
                }
                else
                {
                    AttackTarget();
                }
            }
            else
            {
                target = FindNearestEnemy();
            }
        }
        else
        {
            target = FindNearestEnemy();
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        if (distToPlayer > followDistance && target == null)
        {
            FollowPlayer();
        }
        else if (target == null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }

            DasherAI dasher = hit.GetComponent<DasherAI>();
            if (dasher != null)
            {
                float dist = Vector3.Distance(transform.position, dasher.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = dasher.transform;
                }
            }

            MimicAI mimic = hit.GetComponent<MimicAI>();
            if (mimic != null)
            {
                float dist = Vector3.Distance(transform.position, mimic.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = mimic.transform;
                }
            }
        }

        return nearest;
    }

    private void MoveToTarget()
    {
        if (target == null || rb == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0;
    }

    private void FollowPlayer()
    {
        if (player == null || rb == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0;
    }

    private void AttackTarget()
    {
        if (target == null || rb == null) return;

        rb.velocity = Vector2.zero;

        if (attackTimer <= 0f)
        {
            EnemyAI enemy = target.GetComponent<EnemyAI>();
            enemy?.TakeDamage(attackDamage);

            DasherAI dasher = target.GetComponent<DasherAI>();
            dasher?.TakeDamage(attackDamage);

            MimicAI mimic = target.GetComponent<MimicAI>();
            mimic?.TakeDamage(attackDamage);

            attackTimer = attackCooldown;
            StartCoroutine(LeanTowardsTarget());
        }
    }

    private IEnumerator LeanTowardsTarget()
    {
        if (target == null) yield break;

        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 leanOffset = dir * 0.3f;

        transform.position -= leanOffset;
        yield return new WaitForSeconds(0.05f);
        transform.position += leanOffset * 2f;
        yield return new WaitForSeconds(0.05f);
        transform.position -= leanOffset;
    }

    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(followDistance, followDistance + 1f);
        return player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
    }

    public void Die()
    {
        if (isDead || isRespawning) return;
        isDead = true;
        isRespawning = true;
        gameObject.SetActive(false);

        passiveManager?.RemoveMinion(gameObject);
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        Debug.Log($"[MinionAI] Дракончик мёртв, респавн через {respawnDelay} сек...");
        yield return new WaitForSeconds(respawnDelay);

        if (player == null) yield break;

        passiveManager?.RemoveMinion(gameObject);

        transform.position = GetSpawnPosition();
        gameObject.SetActive(true);
        isDead = false;
        isRespawning = false;
        target = null;
        attackTimer = 0f;

        passiveManager?.AddMinion(gameObject);

        Debug.Log($"[MinionAI] Дракончик респавнулся!");
    }

    public void TakeDamage(float damage)
    {
    }
}
