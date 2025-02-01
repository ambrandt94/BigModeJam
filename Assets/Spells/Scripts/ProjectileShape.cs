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

            // 1. Calculate the base rotation (same as before)
            Quaternion baseRotation = Quaternion.LookRotation(direction);

            // 2. Apply the capsule's initial rotation offset
            //    This assumes your capsule's long axis is along the Z-axis in its prefab.
            //    Adjust the rotation if your capsule's long axis is different.

            Quaternion capsuleRotationOffset = Quaternion.Euler(90f, 0f, 0f); // Example: 90-degree rotation around X

            Quaternion finalRotation = baseRotation * capsuleRotationOffset;
            var projectile = Instantiate(effectPrefab, origin, finalRotation);

            var projectileCollider = projectile.GetComponent<Collider>();
            var casterCollider = caster.GetComponent<Collider>();

            if (projectileCollider && casterCollider)
            {
                Physics.IgnoreCollision(projectileCollider, casterCollider); // Ignore collision
            }

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
