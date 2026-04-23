using UnityEngine;

public class BossBulletProjectile : MonoBehaviour
{
    public float damage = 0.5f;
    public float speed = 8f;

    private Vector3 direction;

    public void Initialize(Vector3 dir, float dmg, float spd)
    {
        direction = dir.normalized;
        damage = dmg;
        speed = spd;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BossBullet>() != null) return;

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}