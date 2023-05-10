using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float enemyCount;
    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private LevelManager levelManager;
    private PlayerInput input;
    private DamageText floatingText;
    private EventSystem eSystem;

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
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;

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
    [SerializeField] private int attackMinDamage = 10;
    [SerializeField] private int attackMaxDamage = 30;
    [SerializeField] private float attackCost = 10f;
    [SerializeField] private Transform castPoint;
    [SerializeField] private bool attacking;
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
    private bool isGamePaused;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject pauseScreen;

    [Header("Background changer")]
    [SerializeField] private GameObject dayBackground;
    [SerializeField] private GameObject dayBackgroundExtension;
    [SerializeField] private GameObject nightBackground;
    [SerializeField] private GameObject nightBackgroundExtension;
    private Color dayOpacity;
    private Color nightOpacity;

    //animations
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int LightAttack = Animator.StringToHash("LightAttack");
    private static readonly int IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int IsDead = Animator.StringToHash("IsDead");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        levelManager = FindObjectOfType<LevelManager>();
        input = GetComponent<PlayerInput>();
        floatingText = GetComponent<DamageText>();
        eSystem = EventSystem.current;
        currentHealth = MaxHealth;
        currentStamina = MaxStamina;
    }

    private void Update()
    {
        if (enemyCount == 0) StartCoroutine(Win());
        if (isDead) StartCoroutine(Death());
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
        
        UpdateStamina(10f * Time.deltaTime);
        
        StartCoroutine(resetShowDamageOnHUD());

        if (!inCombat) UpdateHealth(10f * Time.deltaTime);

        if (debug) combat.LineOfSight();
    }

    private IEnumerator Win()
    {
        yield return new WaitForSeconds(5f);
        levelManager.LoadVictory();
    }
    
    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.velocity = new Vector2(playerInput.x * movementSpeed, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        
        if (canMove)
        {
            attacking = false;
            playerInput = context.ReadValue<Vector2>();
            isMoving = playerInput != Vector2.zero;
            animator.SetBool(IsWalking, isMoving);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (canJump)
        {
            if (StaminaCheck() && isGrounded)
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
    }

    public void OnLight(InputAction.CallbackContext context)
    {
        if (attacking) return;
        if (StaminaCheck()) {
            damageToDeal = Random.Range(attackMinDamage, attackMaxDamage);
            if (damageToDeal == 0) damageToDeal = attackMinDamage;
            staminaToDecrease = attackCost;
            attacking = true;
            animator.SetTrigger(LightAttack);
        }
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        attacking = false;
        if (StaminaCheck()) StartCoroutine(Dash());
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        // If the game is not already paused, pause the game by disabling the player HUD and showing the pause screen.
        if (!isGamePaused)
        {
            eSystem.SetSelectedGameObject(null);
            eSystem.SetSelectedGameObject(pauseButton);
            isGamePaused = true;
            playerHUD.SetActive(!isGamePaused);
            pauseScreen.SetActive(isGamePaused);
            Time.timeScale = 0f;
            canMove = false;
            canJump = false;
        }
        // If the game is already paused, continue the game by enabling the player HUD and hiding the pause screen.
        else
        {
            Continue();
            
        }
    }

    public void Continue()
    {
    // This function continues the game by enabling the player HUD and hiding the pause screen.
        if (isGamePaused)
        {
            isGamePaused = false;
            playerHUD.SetActive(!isGamePaused);
            pauseScreen.SetActive(isGamePaused);
            Time.timeScale = 1f;
            canMove = true;
            canJump = true;
        }
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
                castPoint.localPosition = new Vector3(0.072f,castPoint.localPosition.y,castPoint.localPosition.z);
                dust.Play();
                break;
            case < 0:
                spriteRenderer.flipX = true;
                castPoint.localPosition = new Vector3(-0.072f,castPoint.localPosition.y,castPoint.localPosition.z);
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
        attacking = false;
    }
    
    private IEnumerator Attack()
    {
        UpdateStamina(-staminaToDecrease);
        if (attacking) attacking = false;
        if (combat.LineOfSight())
        {
            if (combat.LineOfSight().CompareTag("Enemy") &&
                !combat.LineOfSight().gameObject.GetComponent<EnemyController>().isDead)
            {
                StartCoroutine(floatingText.DisplayFloatingText(combat.LineOfSight().transform.position,
                    damageToDeal.ToString()));
                inCombat = true;
                combat.LineOfSight().gameObject.GetComponent<EnemyController>().UpdateHealth(-damageToDeal);
                yield return new WaitForSeconds(0.25f);
            }
            else StartCoroutine(ResetCombat());
        }
        yield return new WaitForSeconds(0.45f);
        attacking = false;
    }
    
    public void showDamageOnHUD()
    {
        var color = gotHit.GetComponent<Image>().color;
        color.a = 1f;
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
        UpdateHealth(-5f);
        showDamageOnHUD();
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Obstacle")) ObstacleDamage();
        if (col.gameObject.CompareTag("BackgroundChanger")) StartCoroutine(ChangeBackground());
    }

    private IEnumerator ChangeBackground()
    {
        nightBackground.SetActive(true);
        nightBackgroundExtension.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        dayBackgroundExtension.SetActive(false);
        dayBackground.SetActive(false);
    }
    
    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Obstacle"))
        {
            inCombat = true;
            UpdateHealth(-0.05f);
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
        staminaCheckWarningText.SetActive(true);
        return false;
    }
   
    private IEnumerator Death()
    {
        dust.Stop();
        DisablePlayerInput();
        animator.SetBool(IsDead, isDead);
        yield return new WaitForSeconds(2f);
        levelManager.LoadGameOver();
    }
    
    private void DisablePlayerInput()
    {
        input.actions.Disable();
        canMove = false;
    }

    private void EnablePlayerInput()
    {
        input.actions.Enable();
    }

}