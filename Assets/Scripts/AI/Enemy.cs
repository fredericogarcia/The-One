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
        private EnemyPatrol patrol;
        [Header("Health")]
        [SerializeField] private float currentHealth;
        private const float maxHealth = 100f;
        private bool isDead;
        public bool isAttacking;
 


        private void Awake()
        {
                currentHealth = maxHealth;
                rb = GetComponent<Rigidbody2D>();
                animator = GetComponent<Animator>();
                spriteRenderer = GetComponent<SpriteRenderer>();
                patrol = GetComponent<EnemyPatrol>();
                FindObjectOfType<PlayerController>().enemyCount++;
        }

        private void FixedUpdate()
        {
                if (isDead) StartCoroutine(Death());
                FlipCharacter();
                if (combat.LineOfSight()) patrol.canPatrol = false;
                if (!patrol.canPatrol) patrol.ChasePlayer();
                if (!isAttacking && !patrol.canPatrol && combat.LineOfSight()) animator.SetTrigger("Slam");
        }
        
        private void FlipCharacter()
        {
                switch (patrol.movingRight)
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
                if (combat.LineOfSight())
                {
                        yield return new WaitForSeconds(0.45f);
                        FindObjectOfType<PlayerController>().showDamageOnHUD();
                        FindObjectOfType<PlayerController>().UpdateHealth(-50f);
                        FindObjectOfType<PlayerController>().inCombat = true;
                }
                yield return new WaitForSeconds(0.2f);
                animator.ResetTrigger("Slam");
        }

        public void IsAttacking()
        {
                isAttacking = !isAttacking;
        }
        private IEnumerator Death()
        {
                animator.SetBool("IsDead", true);
                yield return new WaitForSeconds(1f);
                FindObjectOfType<PlayerController>().inCombat = false;
                FindObjectOfType<PlayerController>().enemyCount--;
                Destroy(gameObject);
        }
}
