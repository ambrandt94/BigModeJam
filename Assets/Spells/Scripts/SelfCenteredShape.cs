using UnityEngine;

[CreateAssetMenu(menuName = "Spells/SelfCenteredShape")]
public class SelfCenteredShape : BaseSpell
{
    public float radius;   
    public bool canHitSelf = false;
    public bool canHitOthers = true;
    public Vector3 originOffSet = Vector3.zero;

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {

        foreach (var spellEffect in spellEffects)
        {
            if (spellEffect is ISpellEffect effect)
            {
                effect.Apply(caster.transform, caster.transform.position, Time.deltaTime);
            }
        }

        var colliders = Physics.OverlapSphere(origin + originOffSet, radius);
        foreach (var col in colliders)
        {
            Vector3 hitPoint = col.ClosestPoint(origin); // Closest point on the collider to the area center
            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is ISpellEffect effect)
                {
                    effect.Apply(col.transform, hitPoint, Time.deltaTime);
                }
            }
        }



        if (effectPrefab)
        {
            Instantiate(effectPrefab, origin, Quaternion.identity);
        }
    }
}
