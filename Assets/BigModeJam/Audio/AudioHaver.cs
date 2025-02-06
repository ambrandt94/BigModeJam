using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AudioHaver : MonoBehaviour
{
    private List<AssetEntry> individualAssets;
    private List<VoicePool> voicePools;
    private AudioSource source;

    private void Play(string id, float volumeMod = 1)
    {
        if (individualAssets == null || source == null)
            return;
        foreach (var asset in individualAssets) {
            if (asset.Id == id)
                source.PlayOneShot((AudioClip)asset.Asset, volumeMod);
        }
    }

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }
}

[System.Serializable]
public class AssetEntry
{
    public string Id;
    public Object Asset;
}
