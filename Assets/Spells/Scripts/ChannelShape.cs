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
    private bool hasAppliedTelekinesis = false; // Flag to track if Telekinesis has been applied
    private Coroutine castCoroutine; // Store the coroutine

    BeamVisual visual;
    SpellCaster spellCaster;

    public override void Cast(SpellCaster caster, Vector3 origin, Vector3 direction)
    {
        hasAppliedTelekinesis = false; // Reset at the start of each cast
        this.spellCaster = caster;
        var startTime = Time.time;
       

        if (effectPrefab)
        {
            Channel = Instantiate(effectPrefab, origin, Quaternion.identity);

            Transform beamParent = new GameObject("BeamParent").transform; // Create a parent
            Channel.transform.SetParent(beamParent, false); // Parent to it (no world position reset)

            visual = Channel.GetComponent<BeamVisual>();
            if (visual) {
                visual.Apply(origin, origin + direction * range);
            }
            Channel.transform.LookAt(origin + direction * range);
            

            isChanneling = true;
            Debug.Log("Starting channeling cast");
            castCoroutine = CoroutineRunner.instance.StartCoroutine(CastRoutine(direction)); // Store the coroutine
        }
    }

    public override void CastingUpdate(SpellCaster caster)
    {
        if (Input.GetMouseButtonUp(0))
        {
            StopChanneling(); // Call the dedicated stop function

        }

       
        if (visual && Channel != null)
        {
            // 1. Get the telekinesis target's position
            Transform telekinesisTarget = null;
            foreach (var effect in spellEffects)
            {
                if (effect is TelekinesisEffect tkEffect)
                {
                    telekinesisTarget = tkEffect.telekinesisTarget;
                    break; // Found the target, exit the loop
                }
            }

            if (telekinesisTarget != null) // Check if we have a target
            {
                // 2. Update the beam's end point
                visual.Apply(spellCaster.spellOrigin.transform.position, telekinesisTarget.position); // Update the beam!
                Debug.Log("Updating beat to tk pos");
            }
            else
            {
                // If no target, beam should go to max range
                visual.Apply(spellCaster.spellOrigin.transform.position, Channel.transform.position + caster.Direction * range);
            }
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

    private void StopChanneling() // Dedicated stop function
    {
        if (isChanneling)
        {
            isChanneling = false;

            if (castCoroutine != null) // Stop the coroutine if it's running
            {
                CoroutineRunner.instance.StopCoroutine(castCoroutine);
                castCoroutine = null;
            }

            // Release telekinesis 
            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is TelekinesisEffect telekinesis)
                {
                    telekinesis.Release();
                }
            }

            if (Channel != null)
                Destroy(Channel);

            visual = null; // Important: Clear the visual reference!
        }
    }

    public IEnumerator CastRoutine(Vector3 direction)
    {
        float time = duration;
        isChanneling = true;
        while (isChanneling && time > 0)
        {
            if (Input.GetMouseButtonUp(0))
            {
                StopChanneling();
            }

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
                        hasAppliedTelekinesis = true; // Set the flag after applying
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

        StopChanneling();
    }



}
