using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OwnedPerksUI : MonoBehaviour {
    [Header("Контейнер для иконок")]
    public Transform perksContainer;

    [Header("Префаб иконки перка")]
    public GameObject perkIconPrefab;

    [Header("Размер иконки")]
    public float iconSize = 64f;
    public float iconSpacing = 8f;
    public int iconsPerRow = 5;

    private List<GameObject> perkIcons = new List<GameObject>();
    private bool isInitialized = false;

    void Start()
    {
        RefreshOwnedPerks();
        isInitialized = true;
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            RefreshOwnedPerks();
        }
    }

    public void RefreshOwnedPerks()
    {
        ClearAllIcons();

        var pm = PassiveItemManager.Instance;
        if (pm == null) return;

        int index = 0;
        foreach (PerkType perk in System.Enum.GetValues(typeof(PerkType)))
        {
            if (pm.HasPerk(perk))
            {
                CreatePerkIcon(perk, index);
                index++;
            }
        }
    }

    private void CreatePerkIcon(PerkType perk, int index)
    {
        if (perkIconPrefab == null || perksContainer == null) return;

        GameObject icon = Instantiate(perkIconPrefab, perksContainer);

        Image iconImage = icon.GetComponent<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = GetPerkSprite(perk);
        }

        RectTransform rect = icon.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(iconSize, iconSize);
        }

        int row = index / iconsPerRow;
        int col = index % iconsPerRow;
        float x = col * (iconSize + iconSpacing);
        float y = -row * (iconSize + iconSpacing);
        rect.anchoredPosition = new Vector2(x, y);

        perkIcons.Add(icon);
    }

    private Sprite GetPerkSprite(PerkType perk)
    {
        var shop = FindObjectOfType<ShopUI>();
        if (shop == null) return null;

        foreach (var icon in shop.perkIcons)
        {
            if (icon.perk == perk)
                return icon.sprite;
        }
        return null;
    }

    private void ClearAllIcons()
    {
        foreach (var icon in perkIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        perkIcons.Clear();
    }

    public void OnPerkAdded(PerkType perk)
    {
        RefreshOwnedPerks();
    }
}
