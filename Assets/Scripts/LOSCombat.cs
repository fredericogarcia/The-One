using UnityEngine;


public class LOSCombat : MonoBehaviour
{
     [SerializeField] private float distance;
     public Collider2D LineOfSight()
     {
         Vector2 endPosition = transform.position + transform.localPosition * distance;
         RaycastHit2D hit = Physics2D.Linecast(transform.position, endPosition,
             1 << LayerMask.NameToLayer("Combat"));

         Debug.DrawLine(transform.position, endPosition, hit.collider != null ? Color.green : Color.blue);
         return hit.collider;
     }
}
