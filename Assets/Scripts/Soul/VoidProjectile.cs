using UnityEngine;

public class VoidProjectile : MonoBehaviour
{
    [Header("Параметры")]
    [SerializeField] private float speed = 9f;
    [SerializeField] private float damage = 7f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float searchRadius = 10f;

    private Transform target;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
        FindNearestEnemy();
    }

    private void Update()
    {
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            FindNearestEnemy();
        }

        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position = (Vector2)transform.position + direction * speed * Time.deltaTime;
        }
    }

    private void FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        float closestDist = float.MaxValue;
        Transform closest = null;

        foreach (Collider2D enemy in enemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            DasherAI dasherAI = enemy.GetComponent<DasherAI>();
            GhostAI ghostAI = enemy.GetComponent<GhostAI>();
            MimicAI mimicAI = enemy.GetComponent<MimicAI>();
            FireSkeletAI fireSkeletAI = enemy.GetComponent<FireSkeletAI>();
            
            if (enemyAI != null || dasherAI != null || ghostAI != null || mimicAI != null || fireSkeletAI != null)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = enemy.transform;
                }
            }
        }

        target = closest;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"VoidProjectile OnTriggerEnter2D: {collision.name}");
        
        EnemyAI enemyAI = collision.GetComponent<EnemyAI>();
        DasherAI dasherAI = collision.GetComponent<DasherAI>();
        GhostAI ghostAI = collision.GetComponent<GhostAI>();
        MimicAI mimicAI = collision.GetComponent<MimicAI>();
        FireSkeletAI fireSkeletAI = collision.GetComponent<FireSkeletAI>();
        BossBullet bossBullet = collision.GetComponent<BossBullet>();
        
        if (enemyAI != null)
        {
            Debug.Log($"Попали в EnemyAI! Урон {damage}");
            enemyAI.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (dasherAI != null)
        {
            Debug.Log($"Попали в DasherAI! Урон {damage}");
            dasherAI.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (ghostAI != null)
        {
            Debug.Log($"Попали в GhostAI! Урон {damage}");
            ghostAI.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (mimicAI != null)
        {
            Debug.Log($"Попали в MimicAI! Урон {damage}");
            mimicAI.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (fireSkeletAI != null)
        {
            Debug.Log($"Попали в FireSkeletAI! Урон {damage}");
            fireSkeletAI.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (bossBullet != null)
        {
            Debug.Log($"Попали в БОССА (BossBullet)! Урон {damage}");
            bossBullet.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}