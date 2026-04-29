using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LockedDoor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string nextLevelSceneName = "Level2";
    [SerializeField] private float interactionRange = 1.5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    private bool isUnlocked = false;
    private Transform player;
    private Collider2D coll;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        coll = GetComponent<Collider2D>();
        SetLocked(true);
    }

    private void Update()
    {
        if (!isUnlocked || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= interactionRange && Input.GetKeyDown(KeyCode.E))
        {
            EnterDoor();
        }
    }

    public void Unlock()
    {
        Debug.Log("🚪 Дверь разблокирована!");
        SetLocked(false);
    }

    private void SetLocked(bool locked)
    {
        isUnlocked = !locked;
        coll.enabled = locked;

        if (spriteRenderer != null)
            spriteRenderer.sprite = locked ? lockedSprite : unlockedSprite;
    }

    private void EnterDoor()
    {
        Debug.Log($"🚪 Переход на {nextLevelSceneName}...");
        SceneManager.LoadScene(nextLevelSceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isUnlocked && other.CompareTag("Player"))
        {
            EnterDoor();
        }
    }
}
