using UnityEngine;

public class Room : MonoBehaviour {
    private int roomIndex;
    private bool isBossRoom;
    private int soulReward;
    private bool isActive = false;
    private bool isCleared = false;

    /// <summary>
    /// Инициализировать комнату
    /// </summary>
    public void Initialize(int index, bool boss, int souls)
    {
        roomIndex = index;
        isBossRoom = boss;
        soulReward = souls;

        Debug.Log($"📍 Комната инициализирована (босс: {boss}, награда: {souls} душ)");
    }

    /// <summary>
    /// Активировать комнату - враги начинают атаковать
    /// </summary>
    public void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
        Debug.Log($"🔓 Комната активирована");
    }

    /// <summary>
    /// Деактивировать комнату
    /// </summary>
    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActive || isCleared)
            return;

        // Проверяем остались ли враги (слаймы)
        EnemyAI[] enemies = GetComponentsInChildren<EnemyAI>();

        if (enemies.Length == 0)
        {
            ClearRoom();
        }
    }

    /// <summary>
    /// Комната очищена - все враги убиты
    /// </summary>
    private void ClearRoom()
    {
        if (isCleared)
            return;

        isCleared = true;
        Debug.Log($"✅ Комната очищена! Награда: {soulReward} душ");

        // Даём награду - добавляем души игроку
        SoulUI soulUI = FindObjectOfType<SoulUI>();
        if (soulUI != null)
        {
            soulUI.AddSouls(soulReward);
        }

        // Переходим в следующую комнату через 1 секунду
        Invoke(nameof(GoToNextRoom), 1f);
    }

    private void GoToNextRoom()
    {
        LevelGenerator levelGen = GetComponentInParent<LevelGenerator>();
        if (levelGen != null)
        {
            levelGen.LoadNextRoom();
        }
    }

    public bool IsCleared => isCleared;
    public bool IsBossRoom => isBossRoom;
    public int SoulReward => soulReward;
}