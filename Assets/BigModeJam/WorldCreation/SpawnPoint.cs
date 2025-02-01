using ChainLink.Core;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public bool IsOccupied;

    [SerializeField]
    private bool spawnRandomOnStart;
    [SerializeField]
    private bool allowNPCs;
    [SerializeField]
    private bool allowProps;
    [SerializeField]
    private List<GameObject> customPrefabList;

    public void Spawn(GameObject prefab, float radiusVariance, bool setOccupied)
    {
        if (IsOccupied)
            return;

        if (setOccupied)
            IsOccupied = true;
        Vector3 pos = Random.onUnitSphere * radiusVariance;
        Instantiate(prefab, transform.position + pos, Quaternion.identity);
    }

    private void Start()
    {
        if (spawnRandomOnStart) {
            if (Random.Range(0, 100) < 50)
                return;
            GameObject prefab = ChainUtils.GetRandom(customPrefabList);
            if (prefab != null) {
                Spawn(prefab, 1.2f, true);
            }
        }
    }
}
