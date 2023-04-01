using UnityEngine;

public class FoodGameObject : MonoBehaviour
{
    [SerializeField] private FoodEffect typeOfFood;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            typeOfFood.ApplyEffect(col.gameObject);
        }
    }
}