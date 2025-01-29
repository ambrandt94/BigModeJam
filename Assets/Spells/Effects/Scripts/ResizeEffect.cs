using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/ResizeEffect")]
public class ResizeEffect : ScriptableObject, ISpellEffect
{
    public Vector3 sizeChangePerSecond;
    private bool isResizable = false;

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {

        if (target.GetComponentInParent<Resizable>() == null)
            return;
            

       

        if (target.localScale.magnitude > 0.1f)
        {
            target.localScale += sizeChangePerSecond * deltaTime;
        }
    }
}
