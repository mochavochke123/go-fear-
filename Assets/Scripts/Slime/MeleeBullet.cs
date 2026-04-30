using UnityEngine;

public class MeleeBullet : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifetime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}