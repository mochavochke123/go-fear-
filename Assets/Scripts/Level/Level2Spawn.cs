using UnityEngine;

public class Level2Spawn : MonoBehaviour
{
    [Header("Spawn Point")]
    public Vector3 spawnPosition = new Vector3(0, 0, 0);

    void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = spawnPosition;
            Debug.Log($"🧍 Игрок заспавнен на {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("⚠️ Игрок не найден!");
        }
    }

    public void SetSpawnPosition(Vector3 pos)
    {
        spawnPosition = pos;
    }
}