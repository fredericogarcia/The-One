using UnityEngine;


[CreateAssetMenu(menuName = "Food/Stamina")]
public class Stamina : FoodEffect
{
    [SerializeField] private int amountToIncreaseStamina;
    [SerializeField] private float interval;

    public override void ApplyEffect(GameObject target)
    {
        target.GetComponent<PlayerController>().StartCoroutine(target.GetComponent<PlayerController>().StaminaOvertime(amountToIncreaseStamina, interval));
 
    }
}