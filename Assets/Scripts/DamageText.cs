using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private GameObject prefabDamageText;

    public IEnumerator DisplayFloatingText(Vector2 position, string damage)
    {
        if (prefabDamageText != null)
        {
            var floatText = Instantiate(prefabDamageText, position, Quaternion.identity);
            prefabDamageText.GetComponent<TextMeshPro>().text = damage;
            yield return new WaitForSeconds(1.2f);
            Destroy(floatText);
        }
    }
    
}
