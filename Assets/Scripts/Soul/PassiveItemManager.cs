using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public enum PerkType {
    Dodge,
    Horror,         
    Power,           
    Piercing,       
    NutrFood,       
    CursedSoul,     
    BigSize,        
    Reflection,     
    Escape,         
    Vitality,       
    Vampirism,      
    BattlePace,     
    DoubleHit,      
    Amulet,         
    FireRing,       
    Orda,
    Void            
}

public class PassiveItemManager : MonoBehaviour {
    public static PassiveItemManager Instance;
    public static HashSet<PerkType> ActivePerks = new HashSet<PerkType>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Ссылки")]
    public PlayerHealth playerHealth;
    public SwordWeapon swordWeapon;
    public GameObject fireRingPrefab;
    public GameObject minionPrefab;
    public GameObject voidPortalPrefab;
    public GameObject voidProjectilePrefab;

    // Статы модифицированные перками
    [HideInInspector] public float damageMultiplier = 1f;
    [HideInInspector] public float speedMultiplier = 1f;
    [HideInInspector] public float attackSpeedMultiplier = 1f;
    [HideInInspector] public float enemyHealthMultiplier = 1f;
    [HideInInspector] public float weaponSizeMultiplier = 1f;
    [HideInInspector] public float enemySlowMultiplier = 1f;

    [HideInInspector] public float dodgeChance = 0f;
    [HideInInspector] public float cursedSoulChance = 0f;
    [HideInInspector] public float vampirismChance = 0f;
    [Header("Vampirism Settings")]
    public float vampirismHealAmount = 0.5f;
    public GameObject vampirismParticlePrefab;
    [HideInInspector] public float doubleHitChance = 0f;
    [HideInInspector] public bool piercingActive = false;
    [HideInInspector] public float reflectionMultiplier = 0f;
    public bool ReflectionActive => reflectionMultiplier > 0;

    private HashSet<EnemyAI> piercedEnemies = new HashSet<EnemyAI>();
    private HashSet<DasherAI> piercedDashers = new HashSet<DasherAI>();
    private List<GameObject> fireRings = new List<GameObject>();
    private List<GameObject> minions = new List<GameObject>();
    private int ordaCount = 0;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            swordWeapon = player.GetComponentInChildren<SwordWeapon>();
        }
    }

    public bool HasPerk(PerkType perk) => ActivePerks.Contains(perk);

    public HashSet<PerkType> GetAllPerks() => new HashSet<PerkType>(ActivePerks);

    public void RestorePerks(HashSet<PerkType> perks)
    {
        ActivePerks = new HashSet<PerkType>(perks);
    }

    public bool HasBloodBerserkerSynergy()
    {
        return ActivePerks.Contains(PerkType.Power) && ActivePerks.Contains(PerkType.Vitality);
    }

    public float GetBloodBerserkerMultiplier()
    {
        if (!HasBloodBerserkerSynergy()) return 1f;

        PlayerHealth ph = playerHealth;
        if (ph == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                ph = player.GetComponent<PlayerHealth>();
        }

        if (ph == null) return 1f;

        float hpCurrent = ph.GetHealth();
        float hpMax = ph.GetMaxHealth();

        if (hpMax <= 0) return 1f;

        float hpPercent = hpCurrent / hpMax;
        return 1f + (1f - hpPercent) * 0.5f;
    }

    public void ClearPiercedEnemies()
    {
        piercedEnemies.Clear();
        piercedDashers.Clear();
    }

    public bool ApplyPerk(PerkType perk)
    {
        if (ActivePerks.Contains(perk)) return false;
        ActivePerks.Add(perk);

        switch (perk)
        {
            case PerkType.Dodge:
                dodgeChance += 0.19f;
                break;
            case PerkType.Horror:
                enemyHealthMultiplier *= 0.85f;
                break;
            case PerkType.Power:
                damageMultiplier *= 1.2f;
                CheckBloodBerserkerSynergy(PerkType.Power);
                break;
            case PerkType.Piercing:
                piercingActive = true;
                break;
case PerkType.NutrFood:
                playerHealth?.AddMaxHP(0);
                FindObjectOfType<Healthcontainer>()?.AddHeartContainer();
                FindObjectOfType<PassiveItemManager>()?.ApplyDamageBonus(0.1f);
                break;

            case PerkType.BigSize:
                weaponScale *= 1.2f;
                attackRangeMultiplier *= 1.2f;
                break;
            case PerkType.CursedSoul:
                cursedSoulChance += 0.6f;
                break;
            case PerkType.BigSize:
                weaponSizeMultiplier *= 1.2f;
                break;
            case PerkType.Reflection:
                reflectionMultiplier = 2.5f;
                break;
            case PerkType.Escape:
                speedMultiplier *= 1.2f;
                break;
case PerkType.Vitality:
                playerHealth?.AddMaxHP(0);
                FindObjectOfType<Healthcontainer>()?.AddHeartContainer();
                FindObjectOfType<PassiveItemManager>()?.ApplySpeedBonus(0.15f);
                CheckBloodBerserkerSynergy(PerkType.Vitality);
                break;
            case PerkType.Vampirism:
                vampirismChance += 0.175f;
                break;
            case PerkType.BattlePace:
                attackSpeedMultiplier *= 1.25f;
                break;
            case PerkType.DoubleHit:
                doubleHitChance += 0.25f;
                Debug.Log("🎯 DoubleHit активирован! Шанс: " + (doubleHitChance * 100) + "%");
                break;
            case PerkType.Amulet:
                enemySlowMultiplier = 0.8f;
                break;
            case PerkType.FireRing:
                if (fireRingPrefab != null)
                {
                    GameObject ring = Instantiate(fireRingPrefab, transform.position, Quaternion.identity, transform);
                    fireRings.Add(ring);
                }
                break;
            case PerkType.Orda:
                SpawnOrda();
                break;
            case PerkType.Void:
                if (voidPortalPrefab != null && voidProjectilePrefab != null)
                {
                    GameObject portal = Instantiate(voidPortalPrefab, transform.position + Vector3.up * 2f, Quaternion.identity, transform);
                    StartCoroutine(SpawnVoidProjectileRoutine(portal));
                }
                break;
        }

        FindObjectOfType<OwnedPerksUI>()?.OnPerkAdded(perk);

        return true;
    }

    private System.Collections.IEnumerator SpawnVoidProjectileRoutine(GameObject portal)
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            Debug.Log($"Void tick | playerHealth={ph != null} | hp={ph?.GetHealth()} | portal={portal != null} | prefab={voidProjectilePrefab != null}");

            if (ph != null && ph.GetHealth() > 0 && portal != null && voidProjectilePrefab != null)
            {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(ph.transform.position, 10f);
                Debug.Log($"Colliders в радиусе 10: {enemies.Length}");

                bool hasEnemy = false;
                foreach (Collider2D enemy in enemies)
                {
                    Debug.Log($"  Объект: {enemy.name}");
                    if (enemy.GetComponent<EnemyAI>() != null ||
                        enemy.GetComponent<DasherAI>() != null ||
                        enemy.GetComponent<GhostAI>() != null ||
                        enemy.GetComponent<MimicAI>() != null ||
                        enemy.GetComponent<FireSkeletAI>() != null)
                    {
                        hasEnemy = true;
                        break;
                    }
                }

                Debug.Log($"hasEnemy={hasEnemy}");

                if (hasEnemy)
                {
                    Debug.Log("Спавним снаряд!");
                    Instantiate(voidProjectilePrefab, portal.transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void CheckBloodBerserkerSynergy(PerkType newPerk)
    {
        if ((newPerk == PerkType.Power && HasPerk(PerkType.Vitality)) ||
            (newPerk == PerkType.Vitality && HasPerk(PerkType.Power)))
        {
            Debug.Log("⚔️ SYNERGY ACTIVATED: BLOOD BERSERKER! Чем меньше HP — тем сильнее урон!");
            SynergyUI.Show("BLOOD BERSERKER");
        }
    }

    // Вызывается из EnemyAI.Die()
    public void OnEnemyKilled(GameObject enemy)
    {
        if (vampirismChance > 0 && Random.value < vampirismChance)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player?.GetComponent<PlayerHealth>()?.Heal(vampirismHealAmount);

            SpawnVampirismEffect(enemy.transform.position);
        }

        if (cursedSoulChance > 0 && Random.value < cursedSoulChance)
            FindObjectOfType<SoulUI>()?.AddSouls(1);
    }

    private void SpawnVampirismEffect(Vector3 enemyPos)
    {
        if (vampirismParticlePrefab != null)
        {
            Instantiate(vampirismParticlePrefab, enemyPos, Quaternion.identity);
        }
        else
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = enemyPos;
            particle.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            particle.GetComponent<Renderer>().material.color = Color.red;
            Destroy(particle.GetComponent<Collider>());
            particle.AddComponent<VampirismParticle>();
        }
    }

    // Вызывается из PlayerHealth.TakeDamage()
    public bool TryDodge() => dodgeChance > 0 && Random.value < dodgeChance;

    // Вызывается из PlayerHealth.TakeDamage() при получении урона
    public void OnPlayerDamaged(float damage)
    {
        if (reflectionMultiplier <= 0) return;
        float reflectDamage = damage * reflectionMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 12f);
        foreach (var col in hits)
        {
            col.GetComponent<EnemyAI>()?.TakeDamage(reflectDamage);
            col.GetComponent<DasherAI>()?.TakeDamage(reflectDamage);
            col.GetComponent<GhostAI>()?.TakeDamage(reflectDamage);
            col.GetComponent<MimicAI>()?.TakeDamage(reflectDamage);
            col.GetComponent<FireSkeletAI>()?.TakeDamage(reflectDamage);
            col.GetComponent<BossBullet>()?.TakeDamage(reflectDamage);
        }
    }

    // Вызывается из SwordWeapon при атаке
    public bool TryDoubleHit() => doubleHitChance > 0 && Random.value < doubleHitChance;

    // Пронзание — первый удар по врагу (EnemyAI)
    public float GetPiercingDamage(EnemyAI enemy, float baseDamage)
    {
        if (!piercingActive) return baseDamage;
        if (piercedEnemies.Contains(enemy)) return baseDamage;
        piercedEnemies.Add(enemy);
        return baseDamage * 1.5f;
    }

    // Пронзание — первый удар по врагу (DasherAI)
    public float GetDasherPiercingDamage(DasherAI dasher, float baseDamage)
    {
        if (!piercingActive) return baseDamage;
        if (piercedDashers.Contains(dasher)) return baseDamage;
        piercedDashers.Add(dasher);
        return baseDamage * 1.5f;
    }
    private void OnDestroy()
    {
        foreach (var ring in fireRings)
        {
            if (ring != null) Destroy(ring);
        }
        fireRings.Clear();

        foreach (var minion in minions)
        {
            if (minion != null) Destroy(minion);
        }
        minions.Clear();
    }

    private void SpawnOrda()
    {
        if (minionPrefab == null)
            return;

        Vector3 pos = transform.position;
        pos.x += Random.Range(-1f, 1f);
        pos.y += Random.Range(-1f, 1f);

        GameObject minion = Instantiate(minionPrefab, pos, Quaternion.identity);
        minions.Add(minion);
ordaCount++;
    }

    public void RemoveMinion(GameObject minion)
    {
        minions.Remove(minion);
    }

    public void AddMinion(GameObject minion)
    {
        minions.Add(minion);
    }
}
