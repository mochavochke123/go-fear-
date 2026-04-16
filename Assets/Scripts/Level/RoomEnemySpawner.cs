using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour {
    [Header("Враги")]
    public GameObject[] enemyPrefabs;

    [Header("Сколько спавнить")]
    public int minEnemies = 2;
    public int maxEnemies = 4;

    [Header("Зона спавна (задай размер комнаты)")]
    public Vector2 spawnAreaSize = new Vector2(8f, 8f);
    public Vector2 spawnAreaOffset = Vector2.zero;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[RoomEnemySpawner] Нет префабов врагов!");
            return;
        }

        int count = Random.Range(minEnemies, maxEnemies + 1);

        RoomManager roomManager = GetComponentInParent<RoomManager>();
        Transform parent = roomManager != null ? roomManager.transform : transform;

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f)
            );

            Vector3 spawnPos = transform.position
                             + new Vector3(spawnAreaOffset.x, spawnAreaOffset.y, 0f)
                             + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(enemy, spawnPos, Quaternion.identity, parent);
        }

        Debug.Log($"[RoomEnemySpawner] Заспавнено врагов: {count}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + new Vector3(spawnAreaOffset.x, spawnAreaOffset.y, 0f);
        Gizmos.DrawWireCube(center, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));
    }
}