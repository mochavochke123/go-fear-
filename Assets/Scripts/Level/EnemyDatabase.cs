using UnityEngine;
using System.Collections.Generic;

public enum EnemyType
{
    Normal,      // Regular enemy (Slime)
    Dasher,      // Charges at player
    Boss,        // Boss enemy
    Ranged,      // Shoots projectiles
    MiniBoss,    // Stronger than normal, weaker than boss
    Split,       // Splits into smaller enemies
    Flying,      // Can fly over obstacles
    Minion,      // Friendly minion (like Orda dragon)
    Ghost,       // Phases through walls, vanishes and appears behind player
    Mimic        // Boss chest - pretends to be treasure, attacks when player is close
}

[System.Serializable]
public class EnemyConfig : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Normal;
    
    [Header("Base Stats")]
    public float health = 1f;
    public float speed = 3f;
    public float damage = 0.5f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    
    [Header("Type-Specific Settings")]
    public float dashChargeDistance = 7f;
    public float dashCooldown = 3f;
    public GameObject splitIntoPrefab;
    public int splitCount = 2;
    public float shootCooldown = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    
    [Header("Spawn Settings")]
    [Range(0, 100)]
    public int spawnWeight = 50;
    public bool spawnInBossRoom = true;
    public bool spawnInNormalRooms = true;
    public bool spawnInEliteRooms = true;
    public int minRoomLevel = 1;
    public int maxRoomLevel = 999;
    
    [Header("Visual")]
    public Color enemyColor = Color.white;
    public float spriteScale = 1f;
    public bool showDetectionRadius = false;
    public bool showAttackRadius = false;
}

[System.Serializable]
public class EnemyTypeConfig
{
    public string enemyName;
    public GameObject enemyPrefab;
    public EnemyConfig config;
    public Sprite icon;
}

public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase Instance { get; private set; }
    
    [Header("Enemy Prefabs")]
    public List<EnemyTypeConfig> enemyTypes = new List<EnemyTypeConfig>();
    
    [Header("Default Prefabs (fallback)")]
    public GameObject defaultSlimePrefab;
    public GameObject defaultDasherPrefab;
    public GameObject defaultBossPrefab;
    public GameObject defaultGhostPrefab;
    public GameObject defaultMimicPrefab;
    public GameObject minionPrefab;
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    public EnemyTypeConfig GetRandomEnemyConfig()
    {
        if (enemyTypes.Count == 0)
            return null;
        
        // Filter by spawn settings
        List<EnemyTypeConfig> validEnemies = new List<EnemyTypeConfig>();
        int totalWeight = 0;
        
        foreach (var e in enemyTypes)
        {
            if (e.config == null) continue;
            
            validEnemies.Add(e);
            totalWeight += e.config.spawnWeight;
        }
        
        if (validEnemies.Count == 0)
            return null;
        
        int random = Random.Range(0, totalWeight);
        
        int current = 0;
        foreach (var e in validEnemies)
        {
            current += e.config.spawnWeight;
            if (random <= current)
                return e;
        }
        
        return validEnemies[0];
    }
    
    public EnemyTypeConfig GetEnemyByType(EnemyType type)
    {
        foreach (var e in enemyTypes)
        {
            if (e.config != null && e.config.enemyType == type)
                return e;
        }
        return null;
    }
    
    public GameObject GetRandomEnemyPrefab()
    {
        EnemyTypeConfig config = GetRandomEnemyConfig();
        return config?.enemyPrefab;
    }
    
    public GameObject GetMinionPrefab()
    {
        return minionPrefab;
    }

    public GameObject GetGhostPrefab()
    {
        return defaultGhostPrefab;
    }

    public GameObject GetMimicPrefab()
    {
        return defaultMimicPrefab;
    }
    
    public void AddEnemyType(string name, GameObject prefab, EnemyConfig config)
    {
        enemyTypes.Add(new EnemyTypeConfig
        {
            enemyName = name,
            enemyPrefab = prefab,
            config = config
        });
    }
}
