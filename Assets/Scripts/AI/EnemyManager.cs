using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    
    [Header("Health System")] 
    [SerializeField] private float currentHealth;
    private const float MaxHealth = 100f;
    [Header("AI Settings")] 
    [SerializeField] private bool debug = true;
    [SerializeField] private float detectionRadius = 1f;
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask detectionLayer;
    [Header("Movement")]
    [SerializeField] private bool movingRight;
    [SerializeField] private float movementSpeed;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    [Header("Combat")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool targetFound;
    [SerializeField] private Transform target;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = MaxHealth;
    }

    private void Update()
    {
        FlipCharacter();
        HandleMovement();
        if (!targetFound) HandleDetection();
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
        switch (targetFound)
        {
            case false:
                HandlePatrol();
                break;
            case true when target != null:
            {
                float distanceToPlayer = Vector2.Distance(transform.position, target.position);
                HandlePlayerChase(distanceToPlayer, chaseRange);
                break;
            }
        }
    }
    
    private void HandlePatrol()
    {
        animator.SetBool("IsWalking", true);
        if (movingRight & transform.localPosition.x > rightEdge.position.x) movingRight = false;
        else if (!movingRight & transform.localPosition.x < leftEdge.position.x) movingRight = true;
        rb.velocity = movingRight 
            ? rb.velocity = new Vector3(movementSpeed, rb.velocity.y, 0.0f) 
            : rb.velocity = new Vector3(-movementSpeed, rb.velocity.y, 0.0f);
    }

    private void HandlePlayerChase(float distanceToPlayer, float range)
    {
        if (distanceToPlayer < range && !isAttacking)
        {
            rb.velocity = movingRight 
                ? rb.velocity = new Vector3(movementSpeed, rb.velocity.y, 0.0f) 
                : rb.velocity = new Vector3(-movementSpeed, rb.velocity.y, 0.0f);
        }
    }
    
    private void FlipCharacter()
    {
        spriteRenderer.flipX = movingRight switch
        {
            true => true,
            false => false
        };
    }
    // DEBUG ONLY
    private void OnDrawGizmosSelected()
    {
        if (debug)
        {
            Gizmos.color = Color.red; 
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
