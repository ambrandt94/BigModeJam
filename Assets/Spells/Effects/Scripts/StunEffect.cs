using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/StunEffect")]
public class StunEffect : ScriptableObject, ISpellEffect
{
    public float stunDuration = 2f;

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        if (target == null) return;

        // 1. Get the StunComponent (see next step)
        StunComponent stunComponent = target.GetComponent<StunComponent>();
        if (stunComponent == null)
        {
            stunComponent = target.gameObject.AddComponent<StunComponent>();
        }

        // 2. Apply the stun
        stunComponent.Stun(stunDuration);
    }
}