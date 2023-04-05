using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private LOSCombat combat;
    [Header("AI Settings")] 
    [SerializeField] private State state;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private float detectionRadius = 1f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float distanceToPlayer; 
    [SerializeField] private bool debug = true;
    private enum State
    {
        Patrol,
        ChasingPlayer,
        Attacking,
        Dead
    }
    [Header("Health System")] 
    [SerializeField] private float currentHealth;
    private const float MaxHealth = 100f;
    [Header("Movement")]
    [SerializeField] private Vector2 originalPosition;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float patrolDistance;
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool movingRight;
    [Header("Combat")] 
    [SerializeField] private Transform target;
    [SerializeField] private Transform castPoint;
    [SerializeField] private bool isAttacking;
    [SerializeField] private float attackDamage;

    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        combat = GetComponentInChildren<LOSCombat>();
        currentHealth = MaxHealth;
        originalPosition = transform.localPosition;
        state = State.Patrol;
    }
    
    private void Update()
    {
        FlipCharacter();
        if (target != null) distanceToPlayer = Vector2.Distance(transform.position, target.position);
        
        switch (state)
        {
            case State.Patrol:
            {
                if (movingRight & transform.localPosition.x > originalPosition.x + patrolDistance) movingRight = false;
                else if (!movingRight & transform.localPosition.x < originalPosition.x - patrolDistance) movingRight = true;
                HandleDetection();
                HandlePatrol();
                break;
            }
            case State.ChasingPlayer:
            {
                player.inCombat = true;
                if (movingRight & transform.localPosition.x > target.position.x) movingRight = false;
                else if (!movingRight & transform.localPosition.x < target.position.x) movingRight = true;
                HandlePlayerChase();
                break;
            }
            case State.Attacking:
            {
                if (distanceToPlayer > attackRange) state = State.ChasingPlayer;
                canMove = false;
                animator.SetTrigger("Attack");
                break;
            }
            case State.Dead:
            {
                canMove = false;
                StartCoroutine(Death());
                break;
            }
            default:
                state = State.Patrol;
                break;
        }
  
    }
    
    private void FlipCharacter()
    {
        animator.SetBool("IsWalking", true);
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

        foreach (var result in results)
        {
            var hit = result.transform.GetComponent<PlayerController>();
            if (hit != null)
            {
                target = hit.gameObject.transform;
                state = State.ChasingPlayer;
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

    private void HandlePlayerChase()
    {
        if (distanceToPlayer > chaseRange)
        {
            state = State.Patrol;
            player.inCombat = false;
        }
        if (distanceToPlayer <= attackRange) state = State.Attacking;
        if (canMove)
        {
            if (distanceToPlayer < chaseRange && distanceToPlayer > attackRange && !isAttacking)
            {
                rb.velocity = movingRight 
                    ? rb.velocity = new Vector3(movementSpeed, 0.0f, 0.0f) 
                    : rb.velocity = new Vector3(-movementSpeed, 0.0f, 0.0f);
            }
        }
    }
  
    public void UpdateHealth(float value)
    {
        currentHealth += value;
        if (currentHealth > 100f) currentHealth = MaxHealth;
        if (currentHealth <= 0) state = State.Dead;
    }
    
    public IEnumerator Attack()
    {
        isAttacking = true;
        if (combat.LineOfSight())
        {
            yield return new WaitForSeconds(0.45f);
            player.showDamageOnHUD();
            player.UpdateHealth(-attackDamage);
        }
        isAttacking = false;
        yield return new WaitForSeconds(0.55f);
        canMove = true;
    }
    
    private IEnumerator Death()
    {
        player.inCombat = false;
        animator.SetBool("IsDead", true);
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