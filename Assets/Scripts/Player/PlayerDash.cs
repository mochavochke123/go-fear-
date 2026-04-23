using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour {
    [Header("Рывок")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    [Header("Ссылки")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerHealth playerHealth;

    private Rigidbody2D rb;
    private PlayerinputActions inputActions;

    private bool isDashing;
    private float dashTimer;
    private float cooldownTimer;
    private Vector2 dashDirection;
    private int originalLayer;

    private const string DashLayer = "PlayerDashing";

    // PlayerMovement читает это свойство чтобы не перебивать velocity
    public bool IsDashing => isDashing;

    // ────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalLayer = gameObject.layer;

        if (mainCamera == null) mainCamera = Camera.main;
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        inputActions = new PlayerinputActions();
        inputActions.Enable();
        inputActions.Combat.Enable();
        inputActions.Combat.Dash.performed += OnDash;
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) EndDash();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing) return;

        // Каждый физический кадр принудительно выставляем скорость рывка
        rb.velocity = dashDirection * dashSpeed;
    }

    // ── Вход ────────────────────────────────────────────────────────────────
    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (isDashing || cooldownTimer > 0f) return;
        StartDash();
    }

    // ── Логика ──────────────────────────────────────────────────────────────
    private void StartDash()
    {
        // Считаем направление к курсору мыши
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        dashDirection = (mouseWorld - transform.position).normalized;

        // Если курсор прямо на персонаже — рывок вправо
        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = Vector2.right;

        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        rb.velocity = Vector2.zero;
        playerHealth?.SetInvulnerable(true);

        int dashLayerIndex = LayerMask.NameToLayer(DashLayer);
        if (dashLayerIndex >= 0)
            gameObject.layer = dashLayerIndex;
    }

    private void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;

        gameObject.layer = originalLayer;
        playerHealth?.SetInvulnerable(false);
    }

    private void OnDestroy()
    {
        if (inputActions == null) return;
        inputActions.Combat.Dash.performed -= OnDash;
        inputActions.Combat.Disable();
        inputActions.Player.Disable();
        inputActions.Dispose();
    }
}