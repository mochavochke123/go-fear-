using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthcontainer : MonoBehaviour {
    [Header("UI Links")]
    [SerializeField] private List<Image> images;
    [SerializeField] private List<Sprite> spritesHealthstates;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Dynamic Hearts")]
    public GameObject heartPrefab;
    public Transform heartsParent;

    private void Start()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    public void UpdateUI()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null) return;

        float health = playerHealth.GetHealth();
        float maxHealth = playerHealth.GetMaxHealth();
        int containerCount = images.Count;

        float damageTaken = maxHealth - health;
        float remaining = damageTaken;

        for (int i = containerCount - 1; i >= 0; i--)
        {
            if (spritesHealthstates == null || spritesHealthstates.Count < 3)
                continue;

            Sprite targetSprite;

            if (remaining >= 1)
            {
                targetSprite = spritesHealthstates[0];
                remaining -= 1;
            }
            else if (remaining >= 0.5f)
            {
                targetSprite = spritesHealthstates[1];
                remaining -= 0.5f;
            }
            else
            {
                targetSprite = spritesHealthstates[2];
            }

            if (targetSprite != null)
            {
                images[i].sprite = targetSprite;
            }
        }
    }

    public void AddHeartContainer()
    {
        if (heartPrefab == null || heartsParent == null)
            return;

        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        GameObject newHeart = Instantiate(heartPrefab, heartsParent);
        Image heartImage = newHeart.GetComponent<Image>();

        Sprite fullSprite = spritesHealthstates?[2];
        if (fullSprite != null)
        {
            heartImage.sprite = fullSprite;
        }

        images.Add(heartImage);
        playerHealth?.AddMaxHP(2f);
        UpdateUI();
    }
}
