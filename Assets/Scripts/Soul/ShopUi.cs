using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ShopUI : MonoBehaviour {
    public static ShopUI Instance { get; private set; }

    [Header("UI элементы")]
    public GameObject shopPanel;
    public TextMeshProUGUI perkNameText;
    public TextMeshProUGUI perkDescText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI soulsText;
    public Button buyButton;
    public Image perkIcon;

    [Header("Статы")]
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI attackSpeedText;

    [System.Serializable]
    public class PerkIcon {
        public PerkType perk;
        public Sprite sprite;
    }

    [Header("Иконки перков")]
    public PerkIcon[] perkIcons;

    [Header("Анимация прокрутки")]
    public float spinDuration = 1.5f;
    public float spinSpeed = 0.05f;

    private int purchaseCount = 0;
    private PerkType currentPerk;
    private bool isSpinning = false;

    private static readonly Dictionary<PerkType, string> perkNames = new()
    {
        { PerkType.Dodge,      "Уклонение" },
        { PerkType.Horror,     "Ужас" },
        { PerkType.Power,      "Сила" },
        { PerkType.Piercing,   "Пронзание" },
        { PerkType.NutrFood,   "Полезное питание" },
        { PerkType.CursedSoul, "Проклятая душа" },
        { PerkType.BigSize,    "Размер не главное" },
        { PerkType.Reflection, "Отражение" },
        { PerkType.Escape,     "Побег" },
        { PerkType.Vitality,   "Жизненная сила" },
        { PerkType.Vampirism,  "Вампиризм" },
        { PerkType.BattlePace, "Боевой темп" },
        { PerkType.DoubleHit,  "Двойной удар" },
        { PerkType.Amulet,     "Амулет" },
        { PerkType.FireRing,   "Огненный круг" },
        { PerkType.Orda,       "Орда" },
        { PerkType.Void,       "Войд" }
    };

    private static readonly Dictionary<PerkType, string> perkDescs = new()
{
    { PerkType.Dodge,      "19% шанс не получить урон" },
    { PerkType.Horror,     "Враги получают -15% здоровья" },
    { PerkType.Power,      "+20% к урону" },
    { PerkType.Piercing,   "Первый удар по врагу +50% урона" },
    { PerkType.NutrFood,   "+1 HP контейнер, +10% урон" },
    { PerkType.CursedSoul, "60% шанс получить +1 душу с врага" },
    { PerkType.BigSize,    "+20% размер оружия и радиус" },
    { PerkType.Reflection, "При уроне — враги получают урон x2.5" },
    { PerkType.Escape,     "+20% к скорости движения" },
    { PerkType.Vitality,   "+1 HP контейнер, +15% скорость" },
    { PerkType.Vampirism,  "17.5% шанс восстановить HP при убийстве" },
    { PerkType.BattlePace, "+25% к скорости атаки" },
    { PerkType.DoubleHit,  "25% шанс нанести двойной удар" },
    { PerkType.Amulet,     "Враги замедляются на 20%" },
    { PerkType.FireRing,   "Огненный круг вокруг тебя" },
    { PerkType.Orda,       "Призывает дракона-миньона" },
    { PerkType.Void,       "Портал над игроком выпускает самонаводящийся снаряд каждые 3 сек (урон 7), если враги рядом" }
};

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        shopPanel.SetActive(false);
        buyButton.onClick.AddListener(OnBuy);
    }

    public void ToggleShop()
    {
        if (isSpinning) return;

        if (shopPanel.activeSelf)
        {
            shopPanel.SetActive(false);
            Time.timeScale = 1f;
            GameplayMusicManager.Instance?.StopShopMusic();
        }
        else
        {
            shopPanel.SetActive(true);
            Time.timeScale = 0f;
            GameplayMusicManager.Instance?.PlayShopMusic();
            UpdateStats();
            StartCoroutine(SpinAndReveal());
        }
    }

    public void ExitToMainMenu()
    {
        Debug.Log("Выход в Main Menu...");
        shopPanel.SetActive(false);
        Time.timeScale = 1f;
        GameplayMusicManager.Instance?.StopShopMusic();

        SoulUI.TotalSouls = 0;
        PassiveItemManager.ActivePerks.Clear();

        if (PlayerHealth.Instance != null)
        {
            Destroy(PlayerHealth.Instance.gameObject);
        }

        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator SpinAndReveal()
    {
        isSpinning = true;
        buyButton.interactable = false;

        float elapsed = 0f;
        var allPerks = System.Enum.GetValues(typeof(PerkType));

        while (elapsed < spinDuration)
        {
            PerkType random = (PerkType)allPerks.GetValue(Random.Range(0, allPerks.Length));
            perkNameText.text = perkNames[random];
            perkDescText.text = perkDescs[random];
            if (perkIcon != null)
                perkIcon.sprite = GetPerkSprite(random);
            elapsed += spinSpeed;
            yield return new WaitForSecondsRealtime(spinSpeed);
        }

        currentPerk = GetRandomAvailablePerk();
        perkNameText.text = perkNames[currentPerk];
        perkDescText.text = perkDescs[currentPerk];
        if (perkIcon != null)
            perkIcon.sprite = GetPerkSprite(currentPerk);

        int price = 5 + purchaseCount * 5;
        priceText.text = $"Цена: {price} душ";

        SoulUI soulUI = FindObjectOfType<SoulUI>();
        soulsText.text = $"Душ: {soulUI?.GetSouls() ?? 0}";

        buyButton.interactable = true;
        isSpinning = false;
        UpdateStats();
    }

    private Sprite GetPerkSprite(PerkType perk)
    {
        foreach (var p in perkIcons)
            if (p.perk == perk) return p.sprite;
        return null;
    }

    private PerkType GetRandomAvailablePerk()
    {
        var manager = PassiveItemManager.Instance;
        var all = (PerkType[])System.Enum.GetValues(typeof(PerkType));
        List<PerkType> available = new();

        foreach (var p in all)
            if (!manager.HasPerk(p))
                available.Add(p);

        if (available.Count == 0) return all[0];
        return available[Random.Range(0, available.Count)];
    }

    private void OnBuy()
    {
        int price = 5 + purchaseCount * 5;
        SoulUI soulUI = FindObjectOfType<SoulUI>();

        if (soulUI == null || soulUI.GetSouls() < price)
        {
            perkDescText.text = "Недостаточно душ!";
            return;
        }

        soulUI.SpendSouls(price);
        PassiveItemManager.Instance.ApplyPerk(currentPerk);
        purchaseCount++;

        shopPanel.SetActive(false);
        Time.timeScale = 1f;
        GameplayMusicManager.Instance?.StopShopMusic();
    }

    private void UpdateStats()
    {
        var pm = PassiveItemManager.Instance;
        if (pm == null) return;

        float damage = 15f * pm.damageMultiplier;
        float speed = 5f * pm.speedMultiplier;
        float attackSpeed = 0.6f / pm.attackSpeedMultiplier;

        if (damageText != null)
            damageText.text = $"{damage:F0}";
        if (speedText != null)
            speedText.text = $"{speed:F1}";
        if (attackSpeedText != null)
            attackSpeedText.text = $"{attackSpeed:F2}";
    }
}