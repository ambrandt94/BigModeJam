using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/VisualEffect")]
public class VisualEffect : ScriptableObject, ISpellEffect
{
    public GameObject visualEffectPrefab;   

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        GameObject.Instantiate(visualEffectPrefab, hitPoint, target.rotation);
    }
}
