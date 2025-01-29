using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/KnockbackEffect")]
public class KnockbackEffect : ScriptableObject, ISpellEffect
{
    public float knockbackForce = 10f; // Force applied to the rigidbodies
    public LayerMask targetLayer; // Define which layers can be affected (e.g., exclude the player)

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        // Ensure the target is on the correct layer
        if (((1 << target.gameObject.layer) & targetLayer) == 0)
            return;

        // Apply force to rigidbodies only
        if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 knockbackDirection = (target.position - hitPoint).normalized;
            rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }
}
