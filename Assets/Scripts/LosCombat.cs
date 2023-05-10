using System;
using UnityEngine;
public class LosCombat : MonoBehaviour
{
     [SerializeField] private float distance;
     private RaycastHit2D hit;
     private Vector2 endPosition;
     
     public Collider2D LineOfSight()
     {
         endPosition = transform.position + transform.localPosition * distance;
         hit = Physics2D.Linecast(transform.position, endPosition,
             1 << LayerMask.NameToLayer("Combat"));

         Debug.DrawLine(transform.position, endPosition, hit.collider != null ? Color.green : Color.blue);
        return hit.collider != null ? hit.collider : null;
     }

     private void Update()
     {
         LineOfSight();
     }
}
