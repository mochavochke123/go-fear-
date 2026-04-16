using UnityEngine;
using UnityEngine.InputSystem;

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
            Debug.Log("✓ PlayerVisio найден");
        }
        else
        {
            Debug.LogError("✗ PlayerVisio не найден!");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        UpdateAnimations();
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
        playerInputActions.Disable();
    }
}