using UnityEngine;
using System.Collections;

public class VoidPerk : MonoBehaviour
{
    [Header("Префабы")]
    [SerializeField] public GameObject portalPrefab;
    [SerializeField] public GameObject projectilePrefab;

    [Header("Настройки")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float searchRadius = 10f;

    private GameObject portal;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        SpawnPortal();
        StartCoroutine(SpawnProjectileRoutine());
    }

    private void SpawnPortal()
    {
        if (portalPrefab == null) return;

        Transform player = transform;
        Vector3 portalPos = player.position + Vector3.up * 2f;
        portal = Instantiate(portalPrefab, portalPos, Quaternion.identity, player);
    }

    private IEnumerator SpawnProjectileRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            TrySpawnProjectile();
        }
    }

    private void TrySpawnProjectile()
    {
        if (playerHealth == null || projectilePrefab == null) return;
        if (playerHealth.GetHealth() <= 0) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        bool hasEnemy = false;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy") || enemy.CompareTag("Boss") || 
                enemy.GetComponent<EnemyAI>() != null ||
                enemy.GetComponent<DasherAI>() != null ||
                enemy.GetComponent<GhostAI>() != null ||
                enemy.GetComponent<MimicAI>() != null ||
                enemy.GetComponent<FireSkeletAI>() != null ||
                enemy.GetComponent<BossBullet>() != null)
            {
                hasEnemy = true;
                break;
            }
        }

        if (!hasEnemy) return;

        if (portal != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, portal.transform.position, Quaternion.identity);
            Destroy(projectile, 4f);
        }
    }

    private void OnDestroy()
    {
        if (portal != null)
        {
            Destroy(portal);
        }
        StopAllCoroutines();
    }
}