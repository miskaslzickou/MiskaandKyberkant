using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    private PlayerAction playerActions;
    private Rigidbody2D rb;
    public float speed;
    public float sprintSpeed = 10f;
    public float jumpSpeed;
    public bool jumped;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 15f;
    public float currentStamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerActions = new PlayerAction();
        rb = GetComponent<Rigidbody2D>();
        currentStamina = maxStamina;

        // Basically to funguje tak že když dáš bind na jump tak to skočí pomocí kinematiky a když pustíš tak to cutne speed aby lidi mohli dělat big i short jumps

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
    private void OnEnable()
    {
        playerActions.Enable();

    }
    private void OnDisable()
    {
        playerActions.Disable();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        float sideInput = playerActions.Player.Move.ReadValue<float>();
        
        bool isSprinting = playerActions.Player.Sprint.IsPressed() && currentStamina > 0;

        // Klesání nebo doplňování staminy podle toho, jestli opravdu běžíme dopředu/dozadu (a navíc držíme tlačítko sprintu)
        if (isSprinting && Mathf.Abs(sideInput) > 0)
        {
            currentStamina -= staminaDrainRate * Time.fixedDeltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.fixedDeltaTime;
        }

        // Omezení hodnoty staminy, aby nebyla mimo hranice 0 - maxStamina
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        float activeSpeed = isSprinting ? sprintSpeed : speed;
        rb.linearVelocityX = sideInput * activeSpeed;

    }
    //Pokud se dotýkáš nějakého colliderů můžeš skákat tbh takhle by jsme mohli udělat i parkur na walls, ale klidně můžu  přidat jen aby to fungovalo u spodní části
    private void OnCollisionEnter2D(Collision2D collision)
    {
        jumped = false;
    }
}
