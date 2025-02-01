using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class DestroyedBuildingSegment : MonoBehaviour
{
    [SerializeField, ReadOnly]
    BuildingSegment parentSegment;
    [SerializeField]
    private List<MaterialUtility> fragments;

    private void Start()
    {
        return;
        foreach (var fragment in fragments) {
            fragment.SetTexture(parentSegment.customTexture);
        }
    }
}
