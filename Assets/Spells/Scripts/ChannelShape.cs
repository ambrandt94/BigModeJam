using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

[CreateAssetMenu(menuName = "Spells/ChannelShape")]
public class ChannelShape : BaseSpell
{
    public GameObject Channel;
    public float range;
    public float duration;

    BeamVisual visual;

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        var startTime = Time.time;

        if (effectPrefab) {
            Channel = Instantiate(effectPrefab, origin, Quaternion.identity);
            if (effectPrefabRemainsChild)
                Channel.transform.SetParent(caster.spellOrigin, true);
            visual = Channel.GetComponent<BeamVisual>();
            if (visual) {
                visual.Apply(origin, origin + direction * range);
            }
            Channel.transform.LookAt(origin + direction * range);
            Destroy(Channel, duration);
        }

        CoroutineRunner.instance.StartCoroutine(CastRoutine(direction));
    }

    public override void CastingUpdate(SpellCaster caster)
    {
        if (visual) {
            visual.Apply(Channel.transform.position, Channel.transform.position + caster.Direction * range);
        }
    }

    public IEnumerator CastRoutine(Vector3 direction)
    {
        float time = duration;

        while (time > 0) {
            Gizmos.color = Color.red;
            Debug.DrawLine(Channel.transform.position, Channel.transform.position + direction * range);
            if (Physics.Raycast(Channel.transform.position, direction, out RaycastHit hit, range)) {
                foreach (var spellEffect in spellEffects) {
                    if (spellEffect is ISpellEffect effect) {
                        effect.Apply(hit.transform, hit.point, Time.deltaTime);
                    }
                }
            }
            time -= Time.deltaTime;
            
            yield return null;
        }
    }
}
