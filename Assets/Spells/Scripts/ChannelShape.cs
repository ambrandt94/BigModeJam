using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

[CreateAssetMenu(menuName = "Spells/ChannelShape")]
public class ChannelShape : BaseSpell
{
    private GameObject Channel;
    public float range;
    public float duration;
    private float initialHitDistance;
    private float currentDistance;

    private bool isChanneling = false;

    BeamVisual visual;
    SpellCaster spellCaster;

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        this.spellCaster = caster;
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
            
        }

        isChanneling = true;
        Debug.Log("Starting channeling cast");
        CoroutineRunner.instance.StartCoroutine(CastRoutine(direction));
    }

    public override void CastingUpdate(SpellCaster caster)
    {
        if (Input.GetMouseButtonUp(0))
        {
            TryToStopChanneling();
        }

        if (visual)
        {
            visual.Apply(Channel.transform.position, Channel.transform.position + caster.Direction * range);
        }       
    }

    private void TryToStopChanneling()
    {
        if (isChanneling == true)
        {
            Debug.Log("Stopping channeling cast");
            isChanneling = false;
            // Release telekinesis when the spell ends
            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is TelekinesisEffect telekinesis)
                {
                    telekinesis.Release();
                }
            }
            if(Channel != null)
                Destroy(Channel);
        }
    }

    public IEnumerator CastRoutine(Vector3 direction)
    {
        float time = duration;

        while (time > 0)
        {
            bool hasValidRaycast = false;
            Debug.Log("channeling CastRoutine");

            // Debug draw: Show the ray being cast
            Debug.DrawRay(Channel.transform.position, direction * range, Color.red);

            if (Physics.Raycast(Channel.transform.position, direction, out RaycastHit hit, range))
            {
                hasValidRaycast = true;
                initialHitDistance = hit.distance;

                // Debug draw: Show hit point
                Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.5f, Color.green, 0.1f);
                Debug.Log($"Raycast Hit: {hit.transform.name} at {hit.point}");

                foreach (var spellEffect in spellEffects)
                {
                    if (spellEffect is TelekinesisEffect telekinesis)
                    {
                        // Apply effect and also Debug log
                        Debug.Log($"Applying Telekinesis to {hit.transform.name}");
                        telekinesis.SetCaster(this.spellCaster);
                        telekinesis.Apply(hit.transform, hit.point, Time.deltaTime);
                    }
                    else if (spellEffect is ISpellEffect effect)
                    {
                        effect.Apply(hit.transform, hit.point, Time.deltaTime);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Raycast missed!");
            }

            // Debug draw: Show object tracking position
            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is TelekinesisEffect telekinesis && telekinesis.HasTarget())
                {
                    Debug.DrawLine(telekinesis.GetTargetPosition(), telekinesis.GetTargetPosition() + Vector3.up * 0.5f, Color.blue, 0.1f);
                    Debug.Log($"Object Position: {telekinesis.GetTargetPosition()}");
                }
            }

            // Ensure telekinesis keeps updating even if raycast misses
            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is TelekinesisEffect telekinesis)
                {
                    telekinesis.UpdateEffect(hasValidRaycast);
                }
            }

            time -= Time.deltaTime;
            yield return null;
        }

        TryToStopChanneling();
    }



}
