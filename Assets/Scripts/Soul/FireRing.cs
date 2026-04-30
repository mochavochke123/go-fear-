using UnityEngine;

public class FireRing : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float orbitRadius = 7f;
    [SerializeField] private float damageRadius = 7f;
    [SerializeField] private float damageInterval = 0.5f;
    
    private float damage = 5f;
    private float timer;
    private Transform player;
    
    void Start()
    {
        orbitRadius = 5.5f;
        damageRadius = 5.5f;
        damageInterval = 0.5f;
        rotationSpeed = 120f;
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            transform.position = player.position + (Vector3)(Vector2.up * orbitRadius);
        }
        
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null) col.isTrigger = true;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.simulated = false;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        transform.RotateAround(player.position, Vector3.forward, rotationSpeed * Time.deltaTime);
        
        timer += Time.deltaTime;
        if (timer >= damageInterval)
        {
            timer = 0;
            DamageEnemies();
        }
    }
    
    void DamageEnemies()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Player")) continue;
            
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null) { enemy.TakeDamage(damage); continue; }
            
            DasherAI dasher = col.GetComponent<DasherAI>();
            if (dasher != null) { dasher.TakeDamage(damage); continue; }
            
            GhostAI ghost = col.GetComponent<GhostAI>();
            if (ghost != null) { ghost.TakeDamage(damage); continue; }
            
            MimicAI mimic = col.GetComponent<MimicAI>();
            if (mimic != null) { mimic.TakeDamage(damage); continue; }
            
            FireSkeletAI fs = col.GetComponent<FireSkeletAI>();
            if (fs != null) { fs.TakeDamage(damage); continue; }
            
            BossBullet boss = col.GetComponent<BossBullet>();
            if (boss != null) { boss.TakeDamage(damage); }
        }
    }
}