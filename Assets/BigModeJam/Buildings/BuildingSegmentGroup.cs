using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSegmentGroup : MonoBehaviour
{
    private List<BuildingSegment> segments;

    [Button("Set Building Texture")]
    public void SetBaseBuildingTexture(Texture texture)
    {
        foreach (MaterialUtility mat in MaterialUtility.GetUtilitiesWithId("base", transform)) {
            mat.SetTexture(texture);
        }
    }

    private void Awake()
    {
        segments = new List<BuildingSegment>(GetComponentsInChildren<BuildingSegment>());
    }
}
