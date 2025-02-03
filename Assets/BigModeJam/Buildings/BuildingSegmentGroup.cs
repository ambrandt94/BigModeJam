using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSegmentGroup : MonoBehaviour
{
    [SerializeField]
    private GameObject segmentPrefab;
    [SerializeField]
    private Transform spawnStartTransform;
    [SerializeField]
    private float SegmentHeight;
    [SerializeField]
    GenericAssetPool buildingAssetPool;
    [SerializeField]
    private bool createSegmentsOnStart;

    private List<BuildingSegment> segments;

    [Button("Set Building Texture")]
    public void SetBaseBuildingTexture(Texture texture)
    {
        foreach (var segment in segments) {
            segment.customTexture = texture;
        }
        foreach (MaterialUtility mat in MaterialUtility.GetUtilitiesWithId("base", transform)) {
            mat.SetTexture(texture);
        }
    }

    public void OnSegmentDestroyed(BuildingSegment segment)
    {
        bool hitSegment = false;
        for (int i = 0; i < segments.Count; i++) {
            if (!hitSegment && segment == segments[i]) {
                Debug.Log($"Hit segment {i}");
                hitSegment = true;
            }
            if (hitSegment) {
                Debug.Log($"Turn on Physics on segment {i}");
                segments[i].TriggerDestruction();
                //segments[i].ToggleConstraints(false);
            }
        }
    }

    private void InitializeSegments()
    {
        if (segmentPrefab == null)
            return;
        int num = Random.Range(1, 14);
        Vector3 spawnPos = Vector3.zero;
        for (int i = 0; i < num; i++) {
            BuildingSegment segment = Instantiate(segmentPrefab, spawnPos, Quaternion.identity).GetComponent<BuildingSegment>();
            segment.transform.SetParent(spawnStartTransform, true);
            segment.transform.localPosition = spawnPos;
            segment.transform.localScale = Vector3.one;
            segment.Initialize(this);
            segments.Add(segment);
            spawnPos.y += SegmentHeight;
        }
    }

    private void Awake()
    {
        segments = new List<BuildingSegment>(GetComponentsInChildren<BuildingSegment>());
        if (createSegmentsOnStart)
            InitializeSegments();
        SetBaseBuildingTexture(buildingAssetPool.GetRandom("base") as Texture);
    }
}
