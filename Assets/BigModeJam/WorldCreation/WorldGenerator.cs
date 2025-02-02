using BigModeJam;
using ChainLink.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Action OnFinished;

    private WorldTileGenerator tileMap;

    public void Generate()
    {
        tileMap.Generate();
    }

    private void FinishGeneration()
    {
        OnFinished?.Invoke();
        NPCManager.Instance.FindCharacterTravelPoints();
        NPCManager.Instance.AllRandom();
    }

    private void Awake()
    {
        tileMap = GetComponent<WorldTileGenerator>();
        tileMap.OnFinished += FinishGeneration;
    }
}

[System.Serializable]
public class WorldLayer
{
    public Vector3 Origin => originTransform != null ? originTransform.position : origin;

    private Transform originTransform;
    private Vector3 origin;
}
