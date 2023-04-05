using UnityEngine;

public abstract class FoodEffect : ScriptableObject
{
    public abstract void ApplyEffect(GameObject target);
}