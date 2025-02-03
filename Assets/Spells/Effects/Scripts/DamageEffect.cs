using DestroyIt;
using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/DamageEffect")]
public class DamageEffect : ScriptableObject, ISpellEffect
{
    public float damageAmount = 10f;

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        if (target == null) return;

        Destructible destructible = target.GetComponent<Destructible>();
        if (destructible != null)
        {
            // Apply the damage
            destructible.ApplyDamage(new DirectDamage { DamageAmount = damageAmount });
        }        
        else
        {
             Health health = target.GetComponent<Health>();
             if (health != null)
             {
                 health.TakeDamage(damageAmount);
             }
        }
    }
}
