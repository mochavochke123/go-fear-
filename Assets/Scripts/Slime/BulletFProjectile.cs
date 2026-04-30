using UnityEngine;

public class BulletFProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 0.5f;
    [SerializeField] private float gravityIncrease = 0.3f;

    private Rigidbody2D rb;
    private bool initialized = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0.5f;
        }
    }

    private void Update()
    {
        if (rb != null && rb.gravityScale < 5f)
        {
            rb.gravityScale += gravityIncrease * Time.deltaTime;
        }
    }

    public void Initialize(float damageAmount)
    {
        damage = damageAmount;
        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized) return;

        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        PassiveItemManager pm = PassiveItemManager.Instance;
        if (pm != null)
        {
            bool hit = false;
            
            EnemyAI enemyAI = collision.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);
                hit = true;
            }
            DasherAI dasherAI = collision.GetComponent<DasherAI>();
            if (dasherAI != null)
            {
                dasherAI.TakeDamage(damage);
                hit = true;
            }
            GhostAI ghostAI = collision.GetComponent<GhostAI>();
            if (ghostAI != null)
            {
                ghostAI.TakeDamage(damage);
                hit = true;
            }
            MimicAI mimicAI = collision.GetComponent<MimicAI>();
            if (mimicAI != null)
            {
                mimicAI.TakeDamage(damage);
                hit = true;
            }
            FireSkeletAI fireSkeletAI = collision.GetComponent<FireSkeletAI>();
            if (fireSkeletAI != null)
            {
                fireSkeletAI.TakeDamage(damage);
                hit = true;
            }

            if (hit)
            {
                Destroy(gameObject);
            }
        }
    }
}