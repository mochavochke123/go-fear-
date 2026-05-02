using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour {
    // Singleton
    public static Player Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float minSpeedForRun = 0.1f;
    [SerializeField] private GameObject playerVisual; // PlayerVisio

    [Header("Turn")]
    [SerializeField] private float turnSmoothSpeed = 12f;
    private float currentFlipX = 1f;

    [Header("Tilt")]
    [SerializeField] private float tiltAmount = 8f;
    [SerializeField] private float tiltSmoothSpeed = 10f;
    private float currentTilt = 0f;

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
        
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (playerInputActions == null)
        {
            playerInputActions = new PlayerinputActions();
            playerInputActions.Enable();
        }
        
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
            }
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
        UpdateTilt();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (ShopUI.Instance != null && ShopUI.Instance.shopPanel != null)
            {
                ShopUI.Instance.ToggleShop();
            }
        }
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
        if (playerInputActions == null) return;
        moveDirection = playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    void HandleLook()
    {
        if (Camera.main == null) return;
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3 playerPos = transform.position;

        lookDirection = (mouseWorldPos - playerPos).normalized;

        RotatePlayerTowardsMouse(lookDirection);
    }

void RotatePlayerTowardsMouse(Vector2 direction)
    {
        if (spriteRenderer == null)
            return;

        float targetFlipX = direction.x >= 0 ? 1f : 0f;
        float startFlipX = currentFlipX;
        currentFlipX = Mathf.MoveTowardsAngle(startFlipX, targetFlipX, turnSmoothSpeed * Time.deltaTime * 100f);
        spriteRenderer.flipX = currentFlipX < 0.5f;
    }

    private void UpdateTilt()
    {
        float speed = rb.velocity.magnitude;
        bool isMoving = speed > minSpeedForRun;

        float targetTilt = 0f;
        if (isMoving)
        {
            Vector2 dir = rb.velocity.normalized;
            targetTilt = -dir.y * tiltAmount;
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        if (playerVisual != null)
        {
            playerVisual.transform.rotation = Quaternion.Euler(0f, 0f, currentTilt);
        }
    }

    void FixedUpdate()
    {
        if (playerDash != null && playerDash.IsDashing) return;
        float speed = moveSpeed * (PassiveItemManager.Instance?.GetTotalSpeedMultiplier() ?? 1f);
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

    public void CleanupInput()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
            playerInputActions.Dispose();
            playerInputActions = null;
        }

        PlayerDash dash = GetComponent<PlayerDash>();
        dash?.CleanupInput();

        SwordWeapon sword = GetComponentInChildren<SwordWeapon>();
        sword?.CleanupInput();
    }
}