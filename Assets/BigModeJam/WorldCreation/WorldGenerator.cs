using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField]
    private Transform origin;
    [SerializeField]
    private Vector2 spacing;
    [SerializeField]
    private GameObject[] middlePrefabs;
    [SerializeField]
    private GameObject[] sidePrefabs;
    [SerializeField]
    private GameObject[] cornerPrefabs;

    [SerializeField]
    private Vector2 size;

    [SerializeField]
    private List<GameObject> objects;

    [Button("Create")]
    private void Create()
    {
        objects = new List<GameObject>();

        if (origin == null)
            return;
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++) {
                GameObject prefab = null;
                if (IsCorner(x, y)) {
                    prefab = ChainUtils.GetRandom<GameObject>(cornerPrefabs);
                } else if (IsEdge(x, y)) {
                    prefab = ChainUtils.GetRandom<GameObject>(sidePrefabs);
                } else { 
                    prefab = ChainUtils.GetRandom<GameObject>(middlePrefabs);
                }
                GameObject newObject = Instantiate(prefab, origin);
                newObject.transform.position = GetPosition(x, y);
                objects.Add( newObject );
            }
        }
    }
    [Button("Clear")]
    private void Clear()
    {
        if (objects == null)
            return;
        for (int i = objects.Count-1; i >= 0; i--) {
            GameObject.DestroyImmediate(objects[i]);
        }
        objects = new List<GameObject>();
    }
    private Vector3 GetPosition(int x, int y)
    {
        Vector3 startPosition = origin.position;
        startPosition.x += x * spacing.x;
        startPosition.z += y * spacing.y;
        return startPosition;
    }

    private bool IsEdge(int x, int y)
    {
        if (x == 0 || y == 0)
            return true;
        if (x == size.x - 1 || y == size.y - 1)
            return true;
        return false;
    }

    private bool IsCorner(int x, int y)
    {
        if (x == 0 && y == 0)
            return true;
        if (x == size.x - 1 && y == size.y - 1)
            return true;
        if (x == 0 && x == size.x - 1)
            return true;
        if (y == 0 && y == size.y - 1)
            return true;
        return false;
    }
}

[System.Serializable]
public class WorldLayer
{
    public Vector3 Origin => originTransform != null ? originTransform.position : origin;

    private Transform originTransform;
    private Vector3 origin;
}
