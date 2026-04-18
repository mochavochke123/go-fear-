using UnityEngine;

public class VampirismParticle : MonoBehaviour
{
    private GameObject player;
    public float speed = 15f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.transform.position) < 0.5f)
        {
            Destroy(gameObject);
        }
    }
}
