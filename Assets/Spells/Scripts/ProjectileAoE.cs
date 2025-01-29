using UnityEngine;

public class ProjectileAoE : MonoBehaviour
{
    public float radius = 5f; // AoE radius
    public ScriptableObject[] spellEffects; // Effect to apply in the AoE
    public GameObject areaEffectPrefab; // Visual effect prefab for the AoE
    public float delayBeforeEffect = 0f; // Optional delay before applying the AoE

    private void OnTriggerEnter(Collider other)
    {
        // Determine the impact point
        Vector3 impactPoint = transform.position;

        // Trigger AoE after an optional delay
        if (delayBeforeEffect > 0)
        {
            Invoke(nameof(TriggerAoE), delayBeforeEffect);
        }
        else
        {
            TriggerAoE();
        }

        // Destroy the projectile immediately
        Destroy(gameObject);
    }

    private void TriggerAoE()
    {
        // Spawn the AoE visual effect at the impact point
        if (areaEffectPrefab)
        {
            Instantiate(areaEffectPrefab, transform.position, Quaternion.identity);
        }

        // Apply the spell effect to all targets within the AoE radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var collider in colliders)
        {
            Vector3 hitPoint = collider.ClosestPoint(transform.position); // Closest point to the AoE center


            foreach (var spellEffect in spellEffects)
            {
                if (spellEffect is ISpellEffect effect)
                {
                    effect.Apply(collider.transform, hitPoint, Time.deltaTime);
                }
            }
        }
    }
}
