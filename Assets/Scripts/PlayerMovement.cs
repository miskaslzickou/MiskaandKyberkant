using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerAction playerActions;
    private Rigidbody2D rb;
    public PlayerStats playerStats;

    private float speed;
    private float sprintSpeed;
    public float jumpSpeed;
    public bool jumped;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float currentStamina;

    void Awake()
    {
        playerActions = new PlayerAction();
        rb = GetComponent<Rigidbody2D>();
        currentStamina = maxStamina;

        playerActions.Player.Jump.started += ctx =>
        {
            if (jumped) return;
            jumped = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
        };
        playerActions.Player.Jump.canceled += ctx =>
        {
            if (rb.linearVelocityY > 0) rb.linearVelocityY *= 0.5f;
        };
    }

    void Start()
    {
        // Přihlásíme se k odběru eventu OnStatsChanged z PlayerStats
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += UpdateStatsFromManager;
            // Rovnou si načteme aktuální rychlost
            UpdateStatsFromManager();
        }
    }

    void OnDestroy()
    {
        // Odhlášení eventu při zničení
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateStatsFromManager;
        }
    }

    // Tato metoda se zavolá automaticky pokaždé, když PlayerStats přepočítá staty
    void UpdateStatsFromManager()
    {
        speed = playerStats.GetStat(StatType.Speed);
        sprintSpeed = speed * 1.5f;
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    void FixedUpdate()
    {
        float sideInput = playerActions.Player.Move.ReadValue<float>();
        bool isSprinting = playerActions.Player.Sprint.IsPressed() && currentStamina > 0;

        if (isSprinting && Mathf.Abs(sideInput) > 0)
        {
            currentStamina -= staminaDrainRate * Time.fixedDeltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.fixedDeltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        float activeSpeed = isSprinting ? sprintSpeed : speed;
        rb.linearVelocityX = sideInput * activeSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        jumped = false;
    }
}