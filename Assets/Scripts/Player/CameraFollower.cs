using UnityEngine;

public class CameraFollower : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.1f;

    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // ћгновенно ставим камеру на старте
        if (player != null)
            transform.position = new Vector3(player.position.x, player.position.y, -10f);
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = new Vector3(player.position.x, player.position.y, -10f);

        transform.position = Vector3.SmoothDamp(
            transform.position, target, ref _velocity, smoothTime
        );
    }
}