using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour {
    // Singleton
    public static Player Instance { get; private set; }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float minSpeedForRun = 0.1f;
    [SerializeField] private GameObject playerVisual; // PlayerVisio

    private Vector2 moveDirection = Vector2.zero;
    private Vector2 lookDirection = Vector2.one;
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerinputActions playerInputActions;
    private SpriteRenderer spriteRenderer;
    private PlayerDash playerDash;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerInputActions = new PlayerinputActions();
        playerInputActions.Enable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerDash = GetComponent<PlayerDash>();

        // Ищем Animator на чилде PlayerVisio
        Transform playerVisioTransform = transform.Find("PlayerVisio");

        if (playerVisioTransform != null)
        {
            animator = playerVisioTransform.GetComponent<Animator>();
            spriteRenderer = playerVisioTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
                Debug.Log($"✓ PlayerVisio найден, baseColor={baseColor}");
            }
        }
        else
        {
            Debug.LogError("✗ PlayerVisio не найден!");
        }

        BloodBerserkerAura aura = FindObjectOfType<BloodBerserkerAura>();
        if (aura == null)
        {
            gameObject.AddComponent<BloodBerserkerAura>();
        }
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        UpdateAnimations();
        UpdateBloodBerserkerEffect();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PassiveItemManager.Instance?.ApplyPerk(PerkType.DoubleHit);
            Debug.Log("DEBUG: DoubleHit perk added");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PassiveItemManager.Instance?.ApplyPerk(PerkType.Power);
            Debug.Log("DEBUG: Power perk added");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PassiveItemManager.Instance?.ApplyPerk(PerkType.Vitality);
            Debug.Log("DEBUG: Vitality perk added");
        }
#endif
    }

    private Color baseColor = Color.white;
    private bool isFlashing = false;

    public void OnDamageTaken()
    {
        if (spriteRenderer == null || isFlashing) return;
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {
        isFlashing = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        isFlashing = false;
    }

    private void UpdateBloodBerserkerEffect()
    {
        var pm = PassiveItemManager.Instance;
        bool hasSynergy = pm != null && pm.HasBloodBerserkerSynergy();

        BloodBerserkerAura aura = GetComponent<BloodBerserkerAura>();
        if (aura != null)
            aura.SetActive(hasSynergy);

        if (spriteRenderer == null || isFlashing) return;

        if (!hasSynergy)
        {
            spriteRenderer.color = baseColor;
        }
    }

    void HandleMovement()
    {
        moveDirection = playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    void HandleLook()
    {
        // Получаем позицию мышки в мировых координатах
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // Получаем позицию персонажа
        Vector3 playerPos = transform.position;

        // Вычисляем направление от персонажа к мышке
        lookDirection = (mouseWorldPos - playerPos).normalized;

        // Разворачиваем персонажа в сторону мышки
        RotatePlayerTowardsMouse(lookDirection);
    }

    void RotatePlayerTowardsMouse(Vector2 direction)
    {
        if (spriteRenderer == null)
            return;

        // Если мышка слева от персонажа — переворачиваем спрайт
        if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        
       
    }

    void FixedUpdate()
    {
        if (playerDash != null && playerDash.IsDashing) return;
        float speed = moveSpeed * (PassiveItemManager.Instance?.speedMultiplier ?? 1f);
        rb.velocity = moveDirection * speed;
    }

    void UpdateAnimations()
    {
        if (animator == null)
            return;

        float currentSpeed = rb.velocity.magnitude;
        bool isRunning = currentSpeed > minSpeedForRun;

        animator.SetBool("Isrun", isRunning);
    }

    void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
            playerInputActions.Dispose();
        }
        if (Instance == this) Instance = null;
    }
}