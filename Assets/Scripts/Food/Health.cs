using UnityEngine;

[CreateAssetMenu(menuName = "Food/Health")]
public class Health : FoodEffect
{
    [SerializeField] private int amountToHeal;
    [SerializeField] private float interval;

    public override void ApplyEffect(GameObject target)
    {
        target.GetComponent<PlayerController>().StartCoroutine(target.GetComponent<PlayerController>().HealthOvertime(amountToHeal, interval));
    }
}