using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/ProjectileShape")]
public class ProjectileShape : BaseSpell
{
    public float speed;   

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        if (effectPrefab)
        {
            var projectile = Instantiate(effectPrefab, origin, Quaternion.LookRotation(direction));
            var rb = projectile.GetComponent<Rigidbody>();

            if (rb)
            {
                rb.linearVelocity = direction * speed;
            }

            var spellEffectComponent = projectile.AddComponent<SpellEffectHandler>();
            spellEffectComponent.Initialize(spellEffects);
        }
    }
}

public class SpellEffectHandler : MonoBehaviour
{
    private ScriptableObject[] effects;

    public void Initialize(ScriptableObject[] spellEffects)
    {
        effects = spellEffects;
    }

    private void OnTriggerEnter(Collider other)
    {
        Vector3 hitPoint = transform.position; // Use the projectile's position as the impact point
        foreach (var spellEffect in effects)
        {
            if (spellEffect is ISpellEffect effect)
            {
                effect.Apply(other.transform, hitPoint, Time.deltaTime);
            }
        }
        Destroy(gameObject); // Destroy projectile on impact
    }
}
