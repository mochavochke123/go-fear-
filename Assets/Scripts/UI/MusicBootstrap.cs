using UnityEngine;

public class MusicBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (GameplayMusicManager.Instance == null)
        {
            GameObject musicObj = new GameObject("GameplayMusic");
            GameplayMusicManager manager = musicObj.AddComponent<GameplayMusicManager>();
            
            AudioClip level1 = Resources.Load<AudioClip>("Audio/Music/Level1music");
            AudioClip level2 = Resources.Load<AudioClip>("Audio/Music/Level2music");
            AudioClip shop = Resources.Load<AudioClip>("Audio/Music/Xtal");
            
            manager.level1Music = level1;
            manager.level2Music = level2;
            manager.shopMusic = shop;
            
            DontDestroyOnLoad(musicObj);
        }
    }
}