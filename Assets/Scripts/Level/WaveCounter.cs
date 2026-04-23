using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveCounter : MonoBehaviour
{
    [Header("Holes")]
    public int totalHoles = 6;
    public GameObject[] holeIndicators;

    [Header("Visual")]
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;
    public Color bossColor = Color.red;

    private int currentWave = 0;
    private bool waveInProgress = false;
    private bool bossMode = false;

    [Header("Wave Settings")]
    public WaveRoomManager roomManager;

    public UnityEvent onWaveStarted;
    public UnityEvent onBossSpawned;

    void Start()
    {
        DeactivateAllHoles();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryActivateNextWave();
        }
    }

    public void TryActivateNextWave()
    {
        int enemiesAlive = roomManager != null ? roomManager.GetEnemiesAlive() : 0;

        if (enemiesAlive > 0)
        {
            Debug.Log($"⏳ Враги ещё есть! Осталось: {enemiesAlive}");
            return;
        }

        Debug.Log($"🌊 TryActivateNextWave: waveInProgress={waveInProgress}");

        if (bossMode)
        {
            SpawnBoss();
            return;
        }

        if (currentWave >= totalHoles)
        {
            ActivateBossMode();
            return;
        }

        ActivateNextHole();
    }

    private void ActivateNextHole()
    {
        currentWave++;
        waveInProgress = true;

        if (holeIndicators != null && currentWave <= holeIndicators.Length)
        {
            holeIndicators[currentWave - 1].SetActive(true);
        }

        Debug.Log($"🌊 Волна {currentWave}/{totalHoles}!");

        if (roomManager != null)
        {
            roomManager.StartWave(currentWave);
        }

        onWaveStarted?.Invoke();
    }

    public void OnWaveCleared()
    {
        waveInProgress = false;
        Debug.Log("✅ Волна пройдена!");

        if (currentWave >= totalHoles && !bossMode)
        {
            Debug.Log("💥 Все волны пройдены! Ударь ещё раз для БОССА!");
        }
    }

    private void ActivateBossMode()
    {
        bossMode = true;
        DeactivateAllHoles();

        Debug.Log("👹 БОССОВЫЙ РЕЖИМ! Ударь для босса!");
    }

    private void SpawnBoss()
    {
        if (holeIndicators != null && holeIndicators[totalHoles] != null)
        {
            holeIndicators[totalHoles].SetActive(true);
        }

        Debug.Log("👹 БОСС ПОЯВИЛСЯ!");

        if (roomManager != null)
        {
            roomManager.SpawnBoss();
        }

        onBossSpawned?.Invoke();
    }

    private void DeactivateAllHoles()
    {
        if (holeIndicators == null) return;

        for (int i = 0; i < holeIndicators.Length; i++)
        {
            if (holeIndicators[i] != null)
                holeIndicators[i].SetActive(false);
        }
    }

    public int GetCurrentWave() => currentWave;
    public bool IsWaveInProgress() => waveInProgress;
    public bool IsBossMode() => bossMode;
}