using UnityEngine;
using UnityEngine.InputSystem;

public class SwordWeapon : MonoBehaviour {
    [Header("Атака")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float attackArcAngle = 90f;

    [Header("Ротация")]
    [SerializeField] private Transform swordVisual;
    [SerializeField] private Camera mainCamera;

    [Header("Эффекты")]
    [SerializeField] private GameObject slashEffectPrefab;

    [Header("Звук")]
    [SerializeField] private AudioClip slashSound;
    private AudioSource audioSource;

    [Header("Ссылки")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    private float lastAttackTime = 0f;
    private Vector3 lastAttackDirection = Vector3.right;
    private PlayerinputActions inputActions;
    private float currentScaleX = 1f; // ← храним scaleX отдельно

    private void Start()
    {
        if (playerTransform == null) playerTransform = GetComponentInParent<Transform>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (swordVisual == null) swordVisual = transform;
        if (mainCamera == null) mainCamera = Camera.main;

        audioSource = GetComponent<AudioSource>()
                      ?? gameObject.AddComponent<AudioSource>();

        inputActions = new PlayerinputActions();
        inputActions.Enable();
        inputActions.Combat.Enable();
        inputActions.Combat.Attack.performed += OnAttack;

        Debug.Log("✅ SwordWeapon инициализирован");
    }

    private void Update()
    {
        if (lastAttackTime > 0f)
            lastAttackTime -= Time.deltaTime;

        RotateSwordTowardMouse();

        // ✅ Учитываем currentScaleX при изменении размера
        float size = PassiveItemManager.Instance?.weaponSizeMultiplier ?? 1f;
        if (swordVisual != null)
            swordVisual.localScale = new Vector3(size * currentScaleX, size, 1f);
    }

    // ПОВОРОТ — точная копия старого рабочего кода, ничего не меняем!
    private void RotateSwordTowardMouse()
    {
        if (mainCamera == null || swordVisual == null || playerTransform == null)
            return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        Vector3 dirForFlip = (mouseWorld - playerTransform.position).normalized;
        float scaleX = dirForFlip.x < 0f ? -1f : 1f;
        currentScaleX = scaleX;

        playerTransform.localScale = new Vector3(
            scaleX,
            playerTransform.localScale.y,
            playerTransform.localScale.z
        );

        Vector3 dir = (mouseWorld - swordVisual.position).normalized;
        float angle = Mathf.Atan2(dir.y, scaleX * dir.x) * Mathf.Rad2Deg;
        swordVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        var pm = PassiveItemManager.Instance;
        float cooldown = pm != null ? attackCooldown / pm.attackSpeedMultiplier : attackCooldown;

        if (lastAttackTime <= 0f)
        {
            PerformAttack();
            lastAttackTime = cooldown;
        }
    }

    private void PerformAttack()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        lastAttackDirection = (mouseWorld - swordVisual.position).normalized;

        Debug.Log("⚔️ Атака мечом!");

        animator?.SetTrigger("Attack");
        PlaySlashSound();

        if (slashEffectPrefab != null)
        {
            float angle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
            Quaternion slashRotation = Quaternion.Euler(0f, 0f, angle);
            GameObject effect = Instantiate(slashEffectPrefab, swordVisual.position, slashRotation);
            Destroy(effect, 0.5f);
        }

        DealDamageToEnemies();
    }

    private void DealDamageToEnemies()
    {
        var pm = PassiveItemManager.Instance;
        float damage = attackDamage * (pm?.damageMultiplier ?? 1f);
        float radius = attackRadius * (pm?.weaponSizeMultiplier ?? 1f);

        Vector3 attackPos = swordVisual != null ? swordVisual.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, radius);


        foreach (var col in hits)
        {
            if (col.CompareTag("Player")) continue;

            Vector3 toEnemy = (col.transform.position - attackPos).normalized;
            float angle = Vector3.Angle(lastAttackDirection, toEnemy);

            if (angle <= attackArcAngle / 2f)
            {
                var enemy = col.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    float finalDamage = pm != null ? pm.GetPiercingDamage(enemy, damage) : damage;
                    enemy.TakeDamage(finalDamage);
                    
                    Debug.Log($"💥 {col.gameObject.name} получил {finalDamage} урона!");
                    continue;
                }

                var dasher = col.GetComponent<DasherAI>();
                if (dasher != null)
                {
                    dasher.TakeDamage(damage);
                    
                    Debug.Log($"💥 {col.gameObject.name} получил {damage} урона!");
                }
            }
        }
        if (pm != null && pm.TryDoubleHit())
        {
            Debug.Log("⚡ Двойной удар!");

            if (slashEffectPrefab != null)
            {
                int slashCount = 6;
                for (int i = 0; i < slashCount; i++)
                {
                    float angle = i * (360f / slashCount);
                    Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
                    GameObject effect = Instantiate(
                        slashEffectPrefab,
                        playerTransform.position,
                        rotation
                    );
                    Destroy(effect, 0.4f);
                }
            }

            foreach (var col in hits)
            {
                if (col.CompareTag("Player")) continue;
                col.GetComponent<EnemyAI>()?.TakeDamage(damage);
                col.GetComponent<DasherAI>()?.TakeDamage(damage);
            }
        }
    }

    private void PlaySlashSound()
    {
        if (audioSource != null && slashSound != null)
            audioSource.PlayOneShot(slashSound);
    }

    public bool IsReadyToAttack() => lastAttackTime <= 0f;
    public float GetTimeToNextAttack() => Mathf.Max(0f, lastAttackTime);

    private void OnDestroy()
    {
        if (inputActions == null) return;
        inputActions.Combat.Attack.performed -= OnAttack;
        inputActions.Dispose();
    }
}