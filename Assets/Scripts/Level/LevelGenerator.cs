using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {
    public static LevelGenerator Instance;

    [Header("Фиксированные префабы")]
    public GameObject room01Prefab;
    public GameObject corridorPrefab;
    public GameObject bossRoomPrefab;

    [Header("Случайные комнаты (заполни в Inspector)")]
    public GameObject[] room02Variants;
    public GameObject[] room03Variants;

    private GameObject[] sequence;
    private int currentIndex = 0;
    private List<GameObject> spawnedRooms = new List<GameObject>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BuildSequence();
        SpawnAllRooms();
    }

    private void BuildSequence()
    {
        GameObject room02 = PickRandom(room02Variants, room02Variants[0]);
        GameObject room03 = PickRandom(room03Variants, room03Variants[0]);

        sequence = new GameObject[]
        {
            room01Prefab,
            corridorPrefab,
            room02,
            corridorPrefab,
            room03,
            corridorPrefab,
            bossRoomPrefab
        };

        Debug.Log("[LevelGenerator] Последовательность построена, комнат: " + sequence.Length);
    }

    private void SpawnAllRooms()
    {
        Vector3[] positions = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(24.6f, -10.15f, 0f),
            new Vector3(50.1f, 0f, 0f),
            new Vector3(75.6f, -10.15f, 0f),
            new Vector3(101.1f, 0f, 0f),
            new Vector3(126.6f, -10.15f, 0f),
            new Vector3(152.1f, 0f, 0f)
        };

        for (int i = 0; i < sequence.Length; i++)
        {
            GameObject spawned = Instantiate(sequence[i], positions[i], Quaternion.identity);
            spawnedRooms.Add(spawned);
            Debug.Log("[LevelGenerator] Spawned: " + spawned.name + " at " + positions[i]);
        }
    }

    public void LoadNextRoom()
    {
        currentIndex++;

        if (currentIndex >= sequence.Length)
        {
            Debug.Log("[LevelGenerator] Level complete");
            foreach (GameObject room in spawnedRooms)
            {
                Destroy(room);
            }
            spawnedRooms.Clear();
            return;
        }

        Debug.Log("[LevelGenerator] currentIndex: " + currentIndex);
    }

    private GameObject PickRandom(GameObject[] arr, GameObject fallback)
    {
        if (arr == null || arr.Length == 0) return fallback;
        return arr[Random.Range(0, arr.Length)];
    }
}
