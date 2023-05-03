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
    private LevelManager levelManager;
    private PlayerInput input;
    private DamageText floatingText;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private LosCombat combat;
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
    [SerializeField] private bool canDash = true;
    [SerializeField] private bool isDashing;
    [SerializeField] private float dashingPower;
    [SerializeField] private float dashingTime;
    [SerializeField] private float dashingCoolDown;
    [SerializeField] private int dashStaminaCost = 25;
    [Header("Health and Stamina")] 
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentStamina;
    private const float MaxHealth = 100f;
    private const float MaxStamina = 100f;
    private float HealthPercentage => currentHealth / MaxHealth;
    private float StaminaPercentage => currentStamina / MaxStamina;
    private bool isDead;
    [Header("Combat")] 
    [SerializeField] private bool debug;
    [SerializeField] private Transform aimingDirection;
    [SerializeField] private int attackMinDamage = 10;
    [SerializeField] private int attackMaxDamage = 30;
    [SerializeField] private float attackCost = 10f;
    [SerializeField] private Transform castPoint;
    public bool inCombat;
    private int damageToDeal;
    private float staminaToDecrease;
    private bool beenHit;
    [Header("Player HUD")] 
    [SerializeField] private Image healthBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private GameObject gotHit;
    [SerializeField] private GameObject drinkHUD;
    [SerializeField] private GameObject drinkText;
    [SerializeField] private GameObject chocHUD;
    [SerializeField] private GameObject chocText;
    [SerializeField] private GameObject staminaCheckWarningText;
    
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int LightAttack = Animator.StringToHash("LightAttack");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    public float enemyCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        levelManager = FindObjectOfType<LevelManager>();
        input = GetComponent<PlayerInput>();
        floatingText = GetComponent<DamageText>();
        currentHealth = MaxHealth;
        currentStamina = MaxStamina;
    }

    private void Update()
    {
        if (enemyCount == 0) StartCoroutine(Win());
        
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
        } else
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

        if (debug) combat.LineOfSight();
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
        animator.SetBool(IsWalking, isMoving);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
   
            if (StaminaCheck())
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
        damageToDeal = Random.Range(attackMinDamage, attackMaxDamage);
        staminaToDecrease = attackCost;
        if (StaminaCheck() && damageToDeal != 0f) animator.SetTrigger(LightAttack);
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (StaminaCheck()) StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        
        if (isMoving && canDash)
        {
            animator.SetTrigger(IsDashing);
            isDashing = true;
            canDash = false;
            UpdateStamina(-dashStaminaCost);
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(playerInput.x * dashingPower, 0f);
            yield return new WaitForSeconds(dashingTime);
            rb.gravityScale = originalGravity;
            isDashing = false;
            yield return new WaitForSeconds(dashingCoolDown);
            canDash = true;
            animator.ResetTrigger(IsDashing);
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
        if (currentHealth > 100f) currentHealth = MaxHealth;
        if (currentHealth <= 0) isDead = true;
    }
    
    private void UpdateStamina(float value)
    {
        currentStamina += value;
        if (currentStamina > 100f) currentStamina = MaxStamina;
    }

    public IEnumerator StaminaOvertime(int amountToIncrease, float interval)
    {
        drinkHUD.SetActive(true);
        drinkText.SetActive(true);
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < amountToIncrease; i++)
        {
            UpdateStamina(1f);
            yield return new WaitForSeconds(interval);
        }
        drinkHUD.SetActive(false);
        drinkText.SetActive(false);

    }
    
    public IEnumerator HealthOvertime(int amountToIncrease, float interval)
    {
        chocHUD.SetActive(true);
        chocText.SetActive(true);
        for (int i = 0; i < amountToIncrease; i++)
        {
            UpdateHealth(1f);
            yield return new WaitForSeconds(interval);
        }
        chocHUD.SetActive(false);
        chocText.SetActive(false);
    }

    private IEnumerator ResetCombat()
    {
        yield return new WaitForSeconds(2f);
        inCombat = false;
    }
    
    private IEnumerator Attack()
    {
        UpdateStamina(-staminaToDecrease);
        if (combat.LineOfSight())
        {
            inCombat = true;
            combat.LineOfSight().gameObject.GetComponent<EnemyController>().UpdateHealth(-damageToDeal);
            StartCoroutine(floatingText.DisplayFloatingText(combat.LineOfSight().transform.localPosition, damageToDeal.ToString()));
            yield return new WaitForSeconds(0.1f);
            damageToDeal = 0;
        }
        yield return new WaitForSeconds(0.45f);
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
        if (col.gameObject.CompareTag("Obstacle")) StartCoroutine(ResetCombat());
        if (col.gameObject.CompareTag("Enemy")) StartCoroutine(ResetCombat());
    }

    private bool StaminaCheck()
    {
        if (currentStamina >= staminaToDecrease)
        {
            staminaCheckWarningText.SetActive(false);
            return true;
        }
        else
        {
            staminaCheckWarningText.SetActive(true);
            return false;
        }
    }
    private IEnumerator Death()
    {
        animator.SetBool(IsDead, isDead);
        dust.Stop();
        DisablePlayerInput();
        yield return new WaitForSeconds(2.5f);
        levelManager.LoadGameOver();
    }
    
    private void DisablePlayerInput()
    {
        input.actions.Disable();
    }
}