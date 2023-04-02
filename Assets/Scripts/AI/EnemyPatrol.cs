using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Components")] 
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;
    private Enemy enemy;
    [Header ("Patrol Points")]
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform rightEdge;
    public bool canPatrol = true;
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    public bool movingRight;
    [Header("Combat")] 
    [SerializeField] private Transform target;
    [SerializeField] private float range = 0.2f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
    }
 
    private void Update()
    {
        if (target.position.y > transform.position.y) return;
 
        if (!enemy.isAttacking)
        {
            animator.SetBool("IsWalking", true);
            if (canPatrol)
            {
                if (movingRight & transform.localPosition.x > rightEdge.position.x) movingRight = false;
                else if (!movingRight & transform.localPosition.x < leftEdge.position.x) movingRight = true;
       
                rb.velocity = movingRight ? rb.velocity 
                        = new Vector3(moveSpeed, rb.velocity.y, 0.0f) 
                    : rb.velocity = new Vector3(-moveSpeed, rb.velocity.y, 0.0f);

            } else if (!canPatrol)
            {
                if (movingRight & transform.localPosition.x > target.position.x) movingRight = false;
                else if (!movingRight & transform.localPosition.x < target.position.x) movingRight = true;
            }
        }
        

    }
    
    public void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (distanceToPlayer < 0.16f || enemy.isAttacking) StartCoroutine(idleWalking());
    
        if (distanceToPlayer < range && !enemy.isAttacking)
        {
            rb.velocity = movingRight ? rb.velocity 
                    = new Vector3(moveSpeed, rb.velocity.y, 0.0f) 
                : rb.velocity = new Vector3(-moveSpeed, rb.velocity.y, 0.0f);
        }
    }

    private IEnumerator idleWalking()
    {
        animator.SetBool("IsWalking", false);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("IsWalking", true);
        enemy.isAttacking = false;
    }
}