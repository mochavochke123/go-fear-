using UnityEngine;

public class CameraFollower : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 0.08f;

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
                Debug.LogError("Player не найден!");
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        // Целевая позиция
        Vector3 targetPos = new Vector3(player.position.x, player.position.y, -10f);

        // Плавное движение Lerp
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
    }
}