using UnityEngine;
using System.Collections;

public class RoomManager : MonoBehaviour {
    [Header("����� ������")]
    public Door exitDoor;

    private int enemiesAlive = 0;

    void Start()
    {
        // ��������� �������� � ��� ���� ������� ������� ������
        StartCoroutine(InitAfterSpawn());
    }

    private IEnumerator InitAfterSpawn()
    {
        yield return new WaitForEndOfFrame();

        PassiveItemManager.Instance?.ClearPiercedEnemies();

        EnemyAI[] slimes = GetComponentsInChildren<EnemyAI>();
        DasherAI[] dashers = GetComponentsInChildren<DasherAI>();
        enemiesAlive = slimes.Length + dashers.Length;

        Debug.Log($"[RoomManager] Врагов в комнате: {enemiesAlive}");

        if (enemiesAlive > 0)
        {
            exitDoor?.SetClosed();
        }
        else
        {
            exitDoor?.SetOpen();
        }
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        Debug.Log($"[RoomManager] ���� ����, ��������: {enemiesAlive}");

        if (enemiesAlive <= 0)
        {
            Debug.Log("[RoomManager] ��� ����� �����! �������� �����.");
            exitDoor?.SetOpen();
        }
    }
}