using DestroyIt;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class BuildingSegment : MonoBehaviour
{
    public Texture customTexture;

    private BuildingSegmentGroup parentGroup;
    private Destructible destructible;
    private Rigidbody body;

    private bool isDestroyed;

    public void Initialize(BuildingSegmentGroup parent)
    {
        parentGroup = parent;
    }

    public void TriggerDestruction()
    {
        if (isDestroyed)
            return;
        isDestroyed = true;
        StartCoroutine(ConstructionDestructionRoutine());
    }

    private IEnumerator ConstructionDestructionRoutine()
    {
        ToggleConstraints(false);
        yield return new WaitForEndOfFrame();
        //yield return new WaitForSeconds(Random.Range(.5f, 2f));
        destructible.Destroy();
    }

    public void ToggleConstraints(bool constrained)
    {
        body.constraints = constrained ? RigidbodyConstraints.FreezeAll : RigidbodyConstraints.None;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        destructible = GetComponent<Destructible>();
        destructible.DestroyedEvent += () => { if (!isDestroyed && parentGroup != null) parentGroup.OnSegmentDestroyed(this); };
    }
}
