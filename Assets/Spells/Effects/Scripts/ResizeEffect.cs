using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/ResizeEffect")]
public class ResizeEffect : ScriptableObject, ISpellEffect
{
    public Vector3 sizeChange;
    public float duration; 

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {

        Resizable resizable = target.GetComponentInParent<Resizable>();
        if (resizable == null)
            return;

        // Start resizing over time
        ResizeEffectHandler resizeHandler = target.gameObject.GetComponent<ResizeEffectHandler>();
        if (resizeHandler == null)
        {
            resizeHandler = target.gameObject.AddComponent<ResizeEffectHandler>();
        }

        resizeHandler.StartResizing(target, sizeChange, duration);
    }
}
