using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private TrailRenderer trail;
    private LevelManager levelManager;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private RayCastCombat combat;
    [Header("Player Movement")]
    [SerializeField] private float movementSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 3.75f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isGrounded;
    private bool IsGrounded => Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundLayer);
    private Vector2 playerInput;
    [SerializeField, Range(0, 1)] private float coyoteTime = 0.2f;
    [SerializeField, Range(0, 1)] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTimeCounter;
    [SerializeField] private int jumpStaminaCost = 15;
    [Header("Dash")] 
    [SerializeField] private bool canDash;
    [SerializeField] private bool isDashing;
    [SerializeField] private float dashingPower;
    [SerializeField] private float dashingTime;
    [SerializeField] private float dashingCoolDown;
    [SerializeField] private int dashStaminaCost = 25;
    [Header("Health and Stamina")] 
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentStamina;
    private const float maxHealth = 100f;
    private const float maxStamina = 100f;
    private float HealthPercentage => currentHealth / maxHealth;
    private float StaminaPercentage => currentStamina / maxStamina;
    private bool isDead;
    [Header("Combat")] 
    [SerializeField] private Transform aimingDirection;
    [SerializeField] private float lightAttackDamage = 10f;
    [SerializeField] private float lightAttackCost = 10f;
    [SerializeField] private float heavyAttackDamage = 35f;
    [SerializeField] private float heavyAttackCost = 50f;
    [SerializeField] private Transform castPoint;
    public bool inCombat;
    private float damageToDeal;
    private float staminaToDecrease;
    private bool beenHit;
    [Header("Player HUD")] 
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private GameObject gotHit;

    public float enemyCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>();
        levelManager = FindObjectOfType<LevelManager>();
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        canDash = true;
    }

    private void Update()
    {
        if (isDashing) return;
        if (isJumping) dust.Play();
        FlipCharacter();

        healthBar.fillAmount = HealthPercentage;
        staminaBar.fillAmount = StaminaPercentage;
        
        isGrounded = IsGrounded;
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpBufferTimeCounter = jumpBufferTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            jumpBufferTimeCounter -= Time.deltaTime;
        }

        if (isDead)
        {
            StartCoroutine(Death());
        } 
        UpdateStamina(10f * Time.deltaTime);
        if (!inCombat) UpdateHealth(10f * Time.deltaTime);

        StartCoroutine(resetShowDamageOnHUD());

        if (enemyCount == 0) StartCoroutine(Win());
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(2f);
        levelManager.LoadMainMenu();
    }
    
    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.velocity = new Vector2(playerInput.x * movementSpeed, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        playerInput = context.ReadValue<Vector2>();
        isMoving = playerInput != Vector2.zero;
        animator.SetBool("IsWalking", isMoving);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (currentStamina >= jumpStaminaCost)
        {
            if (context.performed)
            {
                UpdateStamina(-jumpStaminaCost);
                if (coyoteTimeCounter >= 0f && jumpBufferTimeCounter >= 0f)
                {
                    isJumping = true;
                    rb.velocity = new Vector2(playerInput.x, jumpHeight);
                }
            }
            else
            {
                isJumping = false;
            }

            if (context.canceled && rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x + 0.1f, rb.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
                jumpBufferTimeCounter = 0f;
            }
        }
    }

    public void OnLight(InputAction.CallbackContext context)
    {
        animator.SetTrigger("LightAttack");
        damageToDeal = lightAttackDamage;
        staminaToDecrease = lightAttackCost;

    }
    public void OnHeavy(InputAction.CallbackContext context)
    {
        animator.SetTrigger("HeavyAttack");
        damageToDeal = heavyAttackDamage;
        staminaToDecrease = heavyAttackCost;
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (canDash) StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        
        if (isMoving && currentStamina >= dashStaminaCost && !isJumping)
        {
            animator.SetTrigger("IsDashing");
            UpdateStamina(-dashStaminaCost);
            canDash = false;
            isDashing = true;
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(playerInput.x * dashingPower, 0f);
            //trail.emitting = true;
            yield return new WaitForSeconds(dashingTime);
            //trail.emitting = false;
            rb.gravityScale = originalGravity;
            isDashing = false;
            yield return new WaitForSeconds(dashingCoolDown);
            canDash = true;
            animator.ResetTrigger("IsDashing");

        }

    }
   
    private void FlipCharacter()
    {
        switch (playerInput.x)
        {
            case > 0:
                spriteRenderer.flipX = false;
                aimingDirection.localPosition = new Vector3(0.05f,aimingDirection.localPosition.y,aimingDirection.localPosition.z);
                castPoint.localPosition = new Vector3(0.14f,castPoint.localPosition.y,castPoint.localPosition.z);
                dust.Play();
                break;
            case < 0:
                spriteRenderer.flipX = true;
                aimingDirection.localPosition = new Vector3(-0.05f, aimingDirection.localPosition.y,aimingDirection.localPosition.z);
                castPoint.localPosition = new Vector3(-0.14f,castPoint.localPosition.y,castPoint.localPosition.z);
                dust.Play();
                break;
        }
    }
    public void UpdateHealth(float value)
    {
        currentHealth += value;
        if (currentHealth > 100f) currentHealth = maxHealth;
        if (currentHealth <= 0) isDead = true;
    }
    
    private void UpdateStamina(float value)
    {
        currentStamina += value;
        if (currentStamina > 100f) currentStamina = maxStamina;
    }

    public IEnumerator StaminaOvertime(int amountToIncrease)
    {
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < amountToIncrease; i++)
        {
            UpdateStamina(1f);
            yield return new WaitForSeconds(0.035f);
        }
    }
    
    public IEnumerator HealthOvertime(int amountToIncrease)
    {
        for (int i = 0; i < amountToIncrease; i++)
        {
            UpdateHealth(1f);
            yield return new WaitForSeconds(0.035f);
        }
    }

    private IEnumerator resetCombat()
    {
        yield return new WaitForSeconds(2f);
        inCombat = false;
    }
    
    private IEnumerator Attack()
    {

            if (currentStamina >= staminaToDecrease && damageToDeal != 0f)
            {
                UpdateStamina(-staminaToDecrease);
                if (combat.LineOfSight())
                {
                    inCombat = true;
                    combat.LineOfSight().gameObject.GetComponent<Enemy>().UpdateHealth(-damageToDeal);
                    yield return new WaitForSeconds(0.1f);
                    damageToDeal = 0;
                }
            }
    }

    public void showDamageOnHUD()
    {
        var color = gotHit.GetComponent<Image>().color;
        color.a = 0.75f;
        gotHit.GetComponent<Image>().color = color;
        beenHit = true;
    }

    private IEnumerator resetShowDamageOnHUD()
    {
        if (beenHit && currentHealth > 40)
        {
            var color = gotHit.GetComponent<Image>().color;
            color.a -= 0.01f;
            gotHit.GetComponent<Image>().color = color;
        } else if (gotHit.GetComponent<Image>().color.a >= 1)
        {
            yield return new WaitForSeconds(0.1f);
            beenHit = false;
        }
    }
    
    private void ObstacleDamage()
    {
        inCombat = true;
        UpdateHealth(-2.5f);
        showDamageOnHUD();
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Obstacle")) ObstacleDamage();
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Obstacle"))
        {
            inCombat = true;
            UpdateHealth(-0.1f);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Obstacle")) StartCoroutine(resetCombat());
        if (col.gameObject.CompareTag("Enemy")) StartCoroutine(resetCombat());
    }


    private IEnumerator Death()
    {
        animator.SetBool("IsDead", isDead);
        PlayerInput input = GetComponent<PlayerInput>();
        input.actions.Disable();
        yield return new WaitForSeconds(2.5f);
        levelManager.LoadGameOver();
    }
}
