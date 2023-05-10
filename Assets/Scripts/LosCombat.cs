using UnityEngine;
public class LosCombat : MonoBehaviour
{
     [SerializeField] private float distance;
     public Collider2D LineOfSight()
     {
         Vector2 endPosition = transform.position + transform.localPosition * distance;
         RaycastHit2D hit = Physics2D.Linecast(transform.position, endPosition,
             1 << LayerMask.NameToLayer("Combat"));

         Debug.DrawLine(transform.position, endPosition, hit.collider != null ? Color.green : Color.blue);
        return hit.collider != null ? hit.collider : null;
     }
}
