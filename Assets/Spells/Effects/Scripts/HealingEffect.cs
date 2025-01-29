using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/HealingEffect")]
public class HealingEffect : ScriptableObject, ISpellEffect
{
    public float healingAmount; 

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        throw new System.NotImplementedException();
    }
}
