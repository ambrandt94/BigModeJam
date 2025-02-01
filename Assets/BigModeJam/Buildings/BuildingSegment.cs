using DestroyIt;
using NUnit.Framework;
using UnityEngine;

public class BuildingSegment : MonoBehaviour
{
    public Texture customTexture;

    private BuildingSegmentGroup parentGroup;
    private Destructible destructible;
    private Rigidbody body;

    public void ToggleConstraints(bool constrained)
    {
        body.constraints = constrained? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }

    private void Awake()
    {
        parentGroup = GetComponentInParent<BuildingSegmentGroup>();
        body = GetComponent<Rigidbody>();
        destructible = GetComponent<Destructible>();
        destructible.DestroyedEvent += () => { parentGroup.OnSegmentDestroyed(this); }; 
    }
}
