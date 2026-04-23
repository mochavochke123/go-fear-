using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SwordWeapon : MonoBehaviour {
    [Header("Атака")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackRadius = 0.8f;
    [SerializeField] private float attackCooldown = 0.6f;

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

        animator?.SetTrigger("Attack");

        if (PassiveItemManager.Instance != null && PassiveItemManager.Instance.attackSpeedMultiplier > 1f)
        {
            StartCoroutine(BattlePaceEffect());
        }

        PlaySlashSound();

        if (slashEffectPrefab != null)
        {
            float angle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
            Quaternion slashRotation = Quaternion.Euler(0f, 0f, angle);
            GameObject effect = Instantiate(slashEffectPrefab, swordVisual.position, slashRotation);
            Destroy(effect, 0.5f);
        }

        DealDamageToEnemies();

        if (PassiveItemManager.Instance?.TryDoubleHit() == true)
        {
            Debug.Log("⚔️ ДВОЙНОЙ УДАР!");
            if (slashEffectPrefab != null)
            {
                float baseAngle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
                for (int i = 0; i < 6; i++)
                {
                    float offset = (i - 2.5f) * 12f;
                    Quaternion slashRotation = Quaternion.Euler(0f, 0f, baseAngle + offset);
                    GameObject effect = Instantiate(slashEffectPrefab, playerTransform.position, slashRotation);
                    Destroy(effect, 0.4f);
                }
            }
            DealDamageToEnemies();
        }
    }

private void DealDamageToEnemies()
    {
        var pm = PassiveItemManager.Instance;
        float damage = attackDamage * (pm?.damageMultiplier ?? 1f);
        damage *= pm?.GetBloodBerserkerMultiplier() ?? 1f;
        float radius = attackRadius * (pm?.weaponSizeMultiplier ?? 1f);

        Vector3 attackPos = playerTransform != null ? playerTransform.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, radius);

        foreach (var col in hits)
        {
            if (col.CompareTag("Player")) continue;

            var enemy = col.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                float finalDamage = pm != null ? pm.GetPiercingDamage(enemy, damage) : damage;
                enemy.TakeDamage(finalDamage);
                continue;
            }

            var dasher = col.GetComponent<DasherAI>();
            if (dasher != null)
            {
                float dasherDamage = pm != null ? pm.GetDasherPiercingDamage(dasher, damage) : damage;
                dasher.TakeDamage(dasherDamage);
                continue;
            }

            var ghost = col.GetComponent<GhostAI>();
            if (ghost != null)
            {
                ghost.TakeDamage(damage);
                continue;
            }

            var mimic = col.GetComponent<MimicAI>();
            if (mimic != null)
            {
                mimic.TakeDamage(damage);
                continue;
            }

            var fireSkelet = col.GetComponent<FireSkeletAI>();
            if (fireSkelet != null)
            {
                fireSkelet.TakeDamage(damage);
                continue;
            }

            var bossBullet = col.GetComponent<BossBullet>();
            if (bossBullet != null)
            {
                bossBullet.TakeDamage(damage);
                continue;
            }
        }
    }

private void PlaySlashSound()
    {
        if (audioSource != null && slashSound != null)
            audioSource.PlayOneShot(slashSound);
    }

    private IEnumerator BattlePaceEffect()
    {
        if (swordVisual != null)
        {
            SpriteRenderer sr = swordVisual.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color original = sr.color;
                sr.color = Color.yellow;
                yield return new WaitForSeconds(0.15f);
                sr.color = original;
            }
        }
    }

    public bool IsReadyToAttack() => lastAttackTime <= 0f;
    public float GetTimeToNextAttack() => Mathf.Max(0f, lastAttackTime);

    private void OnDestroy()
    {
        if (inputActions == null) return;
        inputActions.Combat.Attack.performed -= OnAttack;
        inputActions.Combat.Disable();
        inputActions.Player.Disable();
        inputActions.Dispose();
    }
}