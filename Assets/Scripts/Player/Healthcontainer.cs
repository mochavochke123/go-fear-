
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Healthcontainer : MonoBehaviour
{
    [SerializeField] private List<Image> images;  
    [SerializeField] private List<Sprite> spritesHealthstates;
    [SerializeField] private PlayerHealth playerHealth;
    private float maxhealth;
    public void UpdateUI()
    {
        float health = playerHealth.GetHealth();

        for (int i = 0; i < images.Count; i++)
        {
            if (health >= (i + 1))
            {
                images[i].sprite = spritesHealthstates[2]; // Полное
            }
            else if (health > i)
            {
                images[i].sprite = spritesHealthstates[1]; // Половина
            }
            else
            {
                images[i].sprite = spritesHealthstates[0]; // Пусто
            }
        }
    }
    private void Initialize()
    {
         maxhealth = playerHealth.GetHealth();
    }
    private void Start()
    {
        Initialize();
    }

}
