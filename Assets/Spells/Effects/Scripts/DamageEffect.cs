using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/DamageEffect")]
public class DamageEffect : ScriptableObject, ISpellEffect
{
    public float damageAmount;
       

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        //throw new System.NotImplementedException();
    }
}
