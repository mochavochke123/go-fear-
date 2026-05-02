using UnityEngine;

public class MeleeBullet : MonoBehaviour {
    [SerializeField] private float damage = 1f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float speed = 6f;

    private bool canHit = false; // задержка перед уроном

    private void Start()
    {
        Destroy(gameObject, lifetime);
        Invoke(nameof(EnableHit), 0.1f); // через 0.1 сек разрешаем хитать
    }

    private void EnableHit()
    {
        canHit = true;
    }

    private void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canHit) return; // ещЄ не летим Ч игнорируем

        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}