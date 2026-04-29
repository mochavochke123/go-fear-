using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (FindObjectsOfType<GameBootstrap>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        CreateMusicManager();
        CreateSoulUI();
        CreatePassiveItemManager();
    }
    
    void CreateMusicManager()
    {
        if (GameplayMusicManager.Instance == null)
        {
            GameObject musicObj = new GameObject("GameplayMusicManager");
            musicObj.AddComponent<GameplayMusicManager>();
        }
    }
    
    void CreateSoulUI()
    {
        if (SoulUI.Instance == null)
        {
            GameObject soulUIobj = new GameObject("SoulUI");
            soulUIobj.AddComponent<SoulUI>();
        }
    }
    
    void CreatePassiveItemManager()
    {
        if (PassiveItemManager.Instance == null)
        {
            GameObject pim = new GameObject("PassiveItemManager");
            pim.AddComponent<PassiveItemManager>();
        }
    }
}