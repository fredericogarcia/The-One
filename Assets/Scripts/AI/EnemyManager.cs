using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private LOSCombat combat;
    
    [Header("Health System")] 
    [SerializeField] private float currentHealth;
    private bool isDead;
    private const float MaxHealth = 100f;
    [Header("AI Settings")] 
    [SerializeField] private bool debug = true;
    [SerializeField] private float detectionRadius = 1f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask detectionLayer;
    [Header("Movement")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool movingRight;
    [SerializeField] private float movementSpeed;
    [SerializeField] private Vector2 originalPosition;
    [SerializeField] private float patrolDistance;
    [Header("Combat")] 
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool targetFound;
    [SerializeField] private Transform target;
    [SerializeField] private Transform castPoint;
    [SerializeField] private float slamDamage;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        combat = GetComponentInChildren<LOSCombat>();
        currentHealth = MaxHealth;
        originalPosition = transform.localPosition;

    }

    private void Update()
    {
        if (isDead)
        {
            StartCoroutine(Death());
            return;
        }
        FlipCharacter();
        HandleMovement();
        if (!targetFound) HandleDetection();
    }
    
    private void FlipCharacter()
    {
        switch (movingRight)
        {
            case true:
                spriteRenderer.flipX = true;
                castPoint.localPosition = new Vector3(0.2f,castPoint.localPosition.y,castPoint.localPosition.z);
                break;
            case false:
                spriteRenderer.flipX = false;
                castPoint.localPosition = new Vector3(-0.2f,castPoint.localPosition.y,-castPoint.localPosition.z);
                break;
        }
    }
    
    private void HandleDetection()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);

        for (int i = 0; i < results.Length; i++)
        {
            var hit = results[i].transform.GetComponent<PlayerController>();
            if (hit != null)
            {
                targetFound = true;
                target = hit.gameObject.transform;
            }
        }
   
    }

    private void HandleMovement()
    {
        animator.SetBool("IsWalking", true);
        switch (targetFound)
        {
            case false:
                if (movingRight & transform.localPosition.x > originalPosition.x + patrolDistance) movingRight = false;
                else if (!movingRight & transform.localPosition.x < originalPosition.x - patrolDistance) movingRight = true;
                HandlePatrol();
                break;
            case true when target != null:
            {
                if (transform.localPosition.y < target.position.y)
                {
                    HandlePatrol();
                    return;
                }
                float distanceToPlayer = Vector2.Distance(transform.position, target.position);
                if (movingRight & transform.localPosition.x > target.position.x) movingRight = false;
                else if (!movingRight & transform.localPosition.x < target.position.x) movingRight = true;
                HandlePlayerChaseAndAttackTriggers(distanceToPlayer);
                break;
            }
        }
    }
    
    private void HandlePatrol()
    {
        if (canMove)
        {
            rb.velocity = movingRight 
                ? rb.velocity = new Vector3(movementSpeed, 0.0f, 0.0f) 
                : rb.velocity = new Vector3(-movementSpeed, 0.0f, 0.0f);
        }
    }

    private void HandlePlayerChaseAndAttackTriggers(float distanceToPlayer)
    {
        if (canMove)
        {
            if (distanceToPlayer < chaseRange && distanceToPlayer > attackRange && !isAttacking)
            {
                rb.velocity = movingRight 
                    ? rb.velocity = new Vector3(movementSpeed, 0.0f, 0.0f) 
                    : rb.velocity = new Vector3(-movementSpeed, 0.0f, 0.0f);
            }
        }

        if (distanceToPlayer <= attackRange)
        {
            canMove = false;
            animator.SetTrigger("Slam");
        }
    }
    
    public IEnumerator SlamAttack()
    {
        isAttacking = true;
        if (combat.LineOfSight())
        {
            yield return new WaitForSeconds(0.45f);
            player.showDamageOnHUD();
            player.UpdateHealth(-slamDamage);
            player.inCombat = true;
        }
        isAttacking = false;
        canMove = true;
        yield return new WaitForSeconds(0.2f);
        animator.ResetTrigger("Slam");
    }

    public void UpdateHealth(float value)
    {
        currentHealth += value;
        if (currentHealth > 100f) currentHealth = MaxHealth;
        if (currentHealth <= 0) isDead = true;
    }
    
    private IEnumerator Death()
    {
        canMove = false;
        animator.SetBool("IsDead", true);
        player.inCombat = false;
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    // DEBUG ONLY
    private void OnDrawGizmosSelected()
    {
        if (debug)
        {
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            combat.LineOfSight();
        }
    }
}
