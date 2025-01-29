using UnityEngine;

[CreateAssetMenu(menuName = "Spells/BeamShape")]
public class BeamShape : BaseSpell
{
    public float range;   

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            foreach (var spellEffect in spellEffects)
            {
               if (spellEffect is ISpellEffect effect)
                {
                    effect.Apply(hit.transform, hit.point, Time.deltaTime);
                }
            }
           
            if (effectPrefab)
            {
                var beam = Instantiate(effectPrefab, origin, Quaternion.identity);
                beam.transform.LookAt(hit.point);
                Destroy(beam, 0.1f);
            }
        }
    }
}
