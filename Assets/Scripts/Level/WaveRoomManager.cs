using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveConfig
{
    public string waveName = "Wave 1";
    public int slimeCount = 3;
    public int dasherCount = 0;
    public int ghostCount = 0;
    public int mimicCount = 0;
    public int fireSkeletCount = 0;
    public int bossBulletCount = 0;
}

public class WaveRoomManager : MonoBehaviour
{
    [Header("Waves Configuration")]
    public WaveConfig[] waves;

    [Header("Enemy Prefabs")]
    public GameObject slimePrefab;
    public GameObject dasherPrefab;
    public GameObject ghostPrefab;
    public GameObject mimicPrefab;
    public GameObject fireSkeletPrefab;
    public GameObject bossBulletPrefab;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaCenter;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20f, 15f);
    public float spawnPadding = 2f;
    public int maxSpawnAttempts = 20;

    [Header("Boss")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition;

    [Header("Timing")]
    public float spawnDelay = 0.5f;

    private int currentWaveIndex = -1;
    private int enemiesAlive = 0;
    private bool bossSpawned = false;
    private bool waveInProgress = false;

    private WaveCounter waveCounter;
    private List<Vector3> usedPositions = new List<Vector3>();

    void Awake()
    {
        UpdateSpawnArea();
    }

    void Start()
    {
        waveCounter = GetComponent<WaveCounter>();
    }

    private void UpdateSpawnArea()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spawnAreaSize = sr.sprite.bounds.size;
            spawnAreaCenter = (Vector2)transform.position;
            Debug.Log($"Размер комнаты: {spawnAreaSize}, Центр: {spawnAreaCenter}");
        }
    }

    public void StartWave(int waveNumber)
    {
        if (waveInProgress)
        {
            Debug.LogWarning("⏳ Волна уже идёт!");
            return;
        }

        waveInProgress = true;
        updateSpawnAreaForPlayer();
        currentWaveIndex = waveNumber - 1;

        if (waves == null || currentWaveIndex >= waves.Length)
        {
            Debug.LogWarning($"Волна {waveNumber} не настроена!");
            return;
        }

        WaveConfig config = waves[currentWaveIndex];
        enemiesAlive = 0;
        usedPositions.Clear();

        StartCoroutine(SpawnWave(config));
    }

    private void updateSpawnAreaForPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            spawnAreaCenter = player.transform.position;
            spawnAreaSize = new Vector2(15f, 10f);
            Debug.Log($"Волна спавнится около игрока: {spawnAreaCenter}");
        }
    }

    private IEnumerator SpawnWave(WaveConfig config)
    {
        waveInProgress = true;
        int totalEnemies = config.slimeCount + config.dasherCount + config.ghostCount + config.mimicCount + config.fireSkeletCount + config.bossBulletCount;
        List<GameObject> prefabs = new List<GameObject>();

        for (int i = 0; i < config.slimeCount; i++) prefabs.Add(slimePrefab);
        for (int i = 0; i < config.dasherCount; i++) prefabs.Add(dasherPrefab);
        for (int i = 0; i < config.ghostCount; i++) prefabs.Add(ghostPrefab);
        for (int i = 0; i < config.mimicCount; i++) prefabs.Add(mimicPrefab);
        for (int i = 0; i < config.fireSkeletCount; i++) prefabs.Add(fireSkeletPrefab);
        for (int i = 0; i < config.bossBulletCount; i++) prefabs.Add(bossBulletPrefab);

        foreach (var prefab in prefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("⚠️ Префаб врага null!");
                continue;
            }

            Vector3 pos = GetRandomSpawnPosition();
            Debug.Log($"Пытаюсь заспавнить в позиции: {pos}");

            if (pos != Vector3.zero)
            {
                GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
                enemy.transform.SetParent(transform);
                enemiesAlive++;
                Debug.Log($"Враг заспавнен! Всего: {enemiesAlive}");
            }
            yield return new WaitForSeconds(spawnDelay);
        }

        Debug.Log($"Волна {currentWaveIndex + 1}: {enemiesAlive} врагов (после спавна)");
        waveInProgress = enemiesAlive > 0;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float minX = spawnAreaCenter.x - spawnAreaSize.x / 2 + spawnPadding;
        float maxX = spawnAreaCenter.x + spawnAreaSize.x / 2 - spawnPadding;
        float minY = spawnAreaCenter.y - spawnAreaSize.y / 2 + spawnPadding;
        float maxY = spawnAreaCenter.y + spawnAreaSize.y / 2 - spawnPadding;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            Vector3 candidate = new Vector3(x, y, 0);

            bool tooClose = false;
            foreach (var pos in usedPositions)
            {
                if (Vector3.Distance(candidate, pos) < 3f) { tooClose = true; break; }
            }

            if (tooClose) continue;

            Collider2D hit = Physics2D.OverlapCircle(candidate, 1f, LayerMask.GetMask("Wall", "Obstacle", "Ground"));
            if (hit == null)
            {
                usedPositions.Add(candidate);
                return candidate;
            }
        }

        return transform.position;
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        Debug.Log($"Враг убит! Осталось: {enemiesAlive}");
        if (enemiesAlive <= 0)
        {
            Debug.Log("✅ ВСЕ ВРАГИ ПОГИБЛИ!");
            waveInProgress = false;
            waveCounter?.OnWaveCleared();
        }
    }

    public void SpawnBoss()
    {
        if (bossSpawned) return;
        bossSpawned = true;

        if (bossPrefab != null)
            Instantiate(bossPrefab, bossSpawnPosition != Vector3.zero ? bossSpawnPosition : transform.position, Quaternion.identity);
    }

    public int GetEnemiesAlive() => enemiesAlive;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube((Vector3)spawnAreaCenter, spawnAreaSize);
    }
}