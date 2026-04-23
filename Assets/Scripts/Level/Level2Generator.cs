using UnityEngine;
using System.Collections.Generic;

public class Level2Generator : MonoBehaviour {
    public static Level2Generator Instance;

    [Header("Level 2 Rooms")]
    public GameObject startRoomPrefab;
    public GameObject corridorPrefab;
    public GameObject waveRoomPrefab;

    [Header("Wave Settings")]
    public int wavesToBoss = 10;

    private int currentWave = 0;
    private bool bossSpawned = false;
    private List<GameObject> spawnedRooms = new List<GameObject>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        SpawnRooms();
    }

    private void SpawnRooms()
    {
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(24.6f, 0f, 0f),
            new Vector3(50.1f, 0f, 0f)
        };

        if (startRoomPrefab != null)
        {
            GameObject start = Instantiate(startRoomPrefab, positions[0], Quaternion.identity);
            spawnedRooms.Add(start);
        }

        if (corridorPrefab != null)
        {
            GameObject corr = Instantiate(corridorPrefab, positions[1], Quaternion.identity);
            spawnedRooms.Add(corr);
        }

        if (waveRoomPrefab != null)
        {
            GameObject wave = Instantiate(waveRoomPrefab, positions[2], Quaternion.identity);
            spawnedRooms.Add(wave);
        }

        Debug.Log("🗺️ Level 2: 3 комнаты спавны");
    }

    public void NextWave()
    {
        currentWave++;
        Debug.Log($"🌊 Волна {currentWave}/{wavesToBoss}");

        if (currentWave >= wavesToBoss && !bossSpawned)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        bossSpawned = true;
        Debug.Log("👹 БОСС ПОЯВИЛСЯ!");
    }

    public int GetCurrentWave() => currentWave;
    public bool IsBossWave() => currentWave >= wavesToBoss;
}