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

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (playerHealth == null || images == null) return;

        float remaining = playerHealth.GetHealth();
        float maxHealth = playerHealth.GetMaxHealth();

        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] == null) continue;

            float segmentHealth = maxHealth / images.Count;

            Sprite targetSprite;
            if (remaining >= segmentHealth)
            {
                targetSprite = spritesHealthstates[2];
                remaining -= segmentHealth;
            }
            else if (remaining >= 0.5f)
            {
                targetSprite = spritesHealthstates[1];
                remaining -= 0.5f;
            }
            else
            {
                targetSprite = spritesHealthstates[0];
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

        if (images == null) images = new List<Image>();
        images.Add(heartImage);
        playerHealth?.AddMaxHP(2f);
        UpdateUI();
    }
}