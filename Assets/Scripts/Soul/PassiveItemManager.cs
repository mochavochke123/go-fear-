using UnityEngine;
using System.Collections.Generic;

public enum PerkType {
    Dodge,          // Уклонение
    Horror,         // Ужас
    Power,          // Сила
    Piercing,       // Пронзание
    NutrFood,       // Полезное питание
    CursedSoul,     // Проклятая душа
    BigSize,        // Размер не главное
    Reflection,     // Отражение
    Escape,         // Побег
    Vitality,       // Жизненная сила
    Vampirism,      // Вампиризм
    BattlePace,     // Боевой темп
    DoubleHit,      // Двойной удар
    Amulet,         // Амулет
    FireRing,       // Огненный круг
    Orda            // Орда
}

public class PassiveItemManager : MonoBehaviour {
    public static PassiveItemManager Instance;

    private HashSet<PerkType> activePerks = new HashSet<PerkType>();

    [Header("Ссылки")]
    public PlayerHealth playerHealth;
    public SwordWeapon swordWeapon;
    public GameObject fireRingPrefab;
    public GameObject minionPrefab;

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

    private HashSet<EnemyAI> piercedEnemies = new HashSet<EnemyAI>();
    private HashSet<DasherAI> piercedDashers = new HashSet<DasherAI>();
    private List<GameObject> fireRings = new List<GameObject>();
    private List<GameObject> minions = new List<GameObject>();
    private int ordaCount = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool HasPerk(PerkType perk) => activePerks.Contains(perk);

    public void ClearPiercedEnemies()
    {
        piercedEnemies.Clear();
        piercedDashers.Clear();
    }

    public bool ApplyPerk(PerkType perk)
    {
        if (activePerks.Contains(perk)) return false;
        activePerks.Add(perk);

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
                break;
            case PerkType.Piercing:
                piercingActive = true;
                break;
            case PerkType.NutrFood:
                playerHealth?.AddMaxHP(1);
                FindObjectOfType<Healthcontainer>()?.AddHeartContainer();
                damageMultiplier *= 1.1f;
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
                playerHealth?.AddMaxHP(1);
                FindObjectOfType<Healthcontainer>()?.AddHeartContainer();
                speedMultiplier *= 1.15f;
                break;
            case PerkType.Vampirism:
                vampirismChance += 0.175f;
                break;
            case PerkType.BattlePace:
                attackSpeedMultiplier *= 1.25f;
                break;
            case PerkType.DoubleHit:
                doubleHitChance += 0.25f;
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
        }

        FindObjectOfType<OwnedPerksUI>()?.OnPerkAdded(perk);

        return true;
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
