using UnityEngine;


public class LOSCombat : MonoBehaviour
{
     [SerializeField] private float distance;
     public Collider2D LineOfSight()
     {
         Vector2 endPosition = transform.position + transform.localPosition * distance;
         RaycastHit2D hit = Physics2D.Linecast(transform.position, endPosition,
             1 << LayerMask.NameToLayer("Combat"));


         if (hit.collider != null)
         {
             Debug.DrawLine(transform.position, endPosition, Color.green);
         }
         else
         {
             Debug.DrawLine(transform.position, endPosition, Color.blue);
         }
         return hit.collider;
     }
}
