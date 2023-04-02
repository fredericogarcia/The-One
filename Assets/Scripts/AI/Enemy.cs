using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
        [Header("Components")] 
        [SerializeField] private RayCastCombat combat;
        [SerializeField] private Transform castPoint;
        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private PlayerController player;
        [Header("Health")]
        [SerializeField] private float currentHealth;
        private const float maxHealth = 100f;
        private bool isDead;
        [Header ("Patrol Points")]
        [SerializeField] private Transform leftEdge;
        [SerializeField] private Transform rightEdge;
        [SerializeField] private bool canPatrol = true;
        [SerializeField] private bool playerAboveEnemy;
        [Header("Movement")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private bool movingRight;
        [Header("Combat")] 
        [SerializeField] private Transform target;
        [SerializeField] private float range = 0.2f;
        [SerializeField] private bool isAttacking;

        private void Awake()
        {
                currentHealth = maxHealth;
                rb = GetComponent<Rigidbody2D>();
                animator = GetComponent<Animator>();
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
                FlipCharacter();
                if (isDead) StartCoroutine(Death());
                if (combat.LineOfSight()) canPatrol = false;
                if (!canPatrol && !isAttacking) ChasePlayer();
                if (!isAttacking && !canPatrol && combat.LineOfSight()) animator.SetTrigger("Slam");
                if (!isAttacking && canPatrol)
                { 
                        animator.SetBool("IsWalking", true);
                        if (movingRight & transform.localPosition.x > rightEdge.position.x) movingRight = false;
                        else if (!movingRight & transform.localPosition.x < leftEdge.position.x) movingRight = true;
                        rb.velocity = movingRight 
                                ? rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0.0f) 
                                : rb.velocity = new Vector3(-moveSpeed, rb.velocity.y, 0.0f);
                } else if (!canPatrol && !playerAboveEnemy)
                {
                        if (movingRight & transform.localPosition.x > target.position.x) movingRight = false;
                        else if (!movingRight & transform.localPosition.x < target.position.x) movingRight = true;
                }
        }
        
        private void ChasePlayer()
        {
                float distanceToPlayer = Vector2.Distance(transform.position, target.position);
                if (distanceToPlayer > range)
                {
                        canPatrol = true;
                        player.inCombat = false;
                        isAttacking = false;
                        return;
                }
                if (distanceToPlayer < 0.16f) rb.velocity = Vector2.zero;
                if (distanceToPlayer < range && !isAttacking)
                {
                        rb.velocity = movingRight 
                                ? rb.velocity = new Vector3(moveSpeed, rb.velocity.y, 0.0f) 
                                : rb.velocity = new Vector3(-moveSpeed, rb.velocity.y, 0.0f);
                }
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
        
        public void UpdateHealth(float value)
        {
                currentHealth += value;
                if (currentHealth > 100f) currentHealth = maxHealth;
                if (currentHealth <= 0) isDead = true;
        }

        public IEnumerator EnemyAttack()
        {
                rb.velocity = Vector2.zero;
                isAttacking = true;
                if (combat.LineOfSight())
                {
                        yield return new WaitForSeconds(0.45f);
                        player.showDamageOnHUD();
                        player.UpdateHealth(-50f);
                        player.inCombat = true;
                }
                isAttacking = false;
                yield return new WaitForSeconds(0.2f);
                animator.ResetTrigger("Slam");
        }
        
        private IEnumerator Death()
        {
                animator.SetBool("IsDead", true);
                yield return new WaitForSeconds(1f);
                player.inCombat = false;
                Destroy(gameObject);
        }
}
