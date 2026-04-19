using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PerkConfig
{
    public PerkType type;
    public string name;
    public string description;
    public Sprite icon;
    [Tooltip("Effect value: chance (0-1), damage multiplier, etc.")]
    public float value = 1f;
}

public class PerkDatabase : MonoBehaviour
{
    public static PerkDatabase Instance { get; private set; }
    
    [Header("Perk Configurations - leave empty for defaults")]
    public List<PerkConfig> customPerks;
    
    // Legacy fields - used by PassiveItemManager
    public float dodgeChance;
    public float cursedSoulChance;
    public float vampirismChance;
    public float doubleHitChance;
    
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float attackSpeedMultiplier = 1f;
    public float enemyHealthMultiplier = 1f;
    public float weaponSizeMultiplier = 1f;
    public float enemySlowMultiplier = 1f;
    
    public float reflectionMultiplier;
    
    public bool piercingActive;
    
    private HashSet<PerkType> purchasedPerks = new HashSet<PerkType>();
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        
        // Initialize default values
        dodgeChance = 0f;
        cursedSoulChance = 0f;
        vampirismChance = 0f;
        doubleHitChance = 0f;
        speedMultiplier = 1f;
        damageMultiplier = 1f;
        attackSpeedMultiplier = 1f;
        enemyHealthMultiplier = 1f;
        weaponSizeMultiplier = 1f;
        enemySlowMultiplier = 1f;
        reflectionMultiplier = 0f;
        piercingActive = false;
    }
    
    public bool HasPerk(PerkType perk)
    {
        return purchasedPerks.Contains(perk);
    }
    
    public bool BuyPerk(PerkType perk)
    {
        if (purchasedPerks.Contains(perk)) return false;
        
        purchasedPerks.Add(perk);
        return true;
    }
    
    // Legacy compatibility methods
    public bool TryDodge() => dodgeChance > 0 && Random.value < dodgeChance;
    public bool TryDoubleHit() => doubleHitChance > 0 && Random.value < doubleHitChance;
    
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetSpeedMultiplier() => speedMultiplier;
    public float GetAttackSpeedMultiplier() => attackSpeedMultiplier;
    public float GetEnemyHealthMultiplier() => enemyHealthMultiplier;
    public float GetWeaponSizeMultiplier() => weaponSizeMultiplier;
    public float GetEnemySlowMultiplier() => enemySlowMultiplier;
    public float GetReflectionMultiplier() => reflectionMultiplier;
    
    public float GetPiercingDamage(float baseDamage)
    {
        if (!piercingActive) return baseDamage;
        return baseDamage * 1.5f;
    }
}
