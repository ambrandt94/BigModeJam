using UnityEngine;

[CreateAssetMenu(menuName = "Spells/AreaShape")]
public class AreaShape : BaseSpell
{
    public float radius;
    public ISpellEffect spellEffect;
    public LayerMask excludeLayers;

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        Vector3 targetPoint = origin + Camera.main.transform.forward * 10f;
        if (Physics.Raycast(origin, direction, out RaycastHit hit))
        {
            targetPoint = hit.point;
        }

        var colliders = Physics.OverlapSphere(targetPoint, radius, ~excludeLayers);
        foreach (var col in colliders)
        {
            Vector3 hitPoint = col.ClosestPoint(targetPoint); // Closest point on the collider to the area center
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
            var instance = Instantiate(effectPrefab, targetPoint, Quaternion.identity);
            Destroy(instance, 2f);
        }
    }
}
 