using UnityEngine;

[CreateAssetMenu(menuName = "Spells/ConeShape")]
public class ConeShape : BaseSpell
{
    public float angle;
    public float range;
    public ISpellEffect spellEffect;

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        var colliders = Physics.OverlapSphere(origin, range);
        foreach (var col in colliders)
        {
            Vector3 toTarget = (col.transform.position - origin).normalized;
            Vector3 hitPoint = col.ClosestPoint(toTarget); // Closest point on the collider to the area center
            if (Vector3.Angle(direction, toTarget) <= angle / 2)
            {
                foreach (var spellEffect in spellEffects)
                {
                    if (spellEffect is ISpellEffect effect)
                    {
                        effect.Apply(col.transform, hitPoint, Time.deltaTime);
                    }
                }
            }
        }

        if (effectPrefab)
        {
            Instantiate(effectPrefab, origin, Quaternion.LookRotation(direction));
        }
    }
}
