using BigModeJam;
using ChainLink.Core;
using DestroyIt;
using System.Collections.Generic;
using UnityEngine;

public class NPCharacter : MonoBehaviour
{
    [SerializeField]
    private float knockbackMultiplier;
    [SerializeField]
    private GenericAssetPool voicePool;

    private AudioSource audioSource;
    private CharacterPather pather;
    private Destructible destructible;
    private RagdollMaker ragDoll;

    private void SetupRagdoll()
    {
        ragDoll = transform.GetOrAddComponent<RagdollMaker>();
        ragDoll.totalMass = 80;
        ragDoll.bones = new List<RagdollMaker.Bone>(NPCManager.Instance.DefaultBones).ToArray();
        ragDoll.CreateRagdoll();
        ragDoll.ToggleActive(false);
    }

    private void OnDestroyed()
    {
        transform.GetChild(0).transform.parent = null;
        ragDoll.ToggleActive(true);
        if (voicePool != null) {
            AudioClip clip = (AudioClip)voicePool.GetRandom("Pain");
            audioSource.PlayOneShot(clip);
        }
    }

    private void Start()
    {
        pather = GetComponent<CharacterPather>();
        GameObject obj = Instantiate(NPCManager.Instance.GetCharacterPrefab("modern"), transform);
        audioSource = obj.GetOrAddComponent<AudioSource>();
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        SetupRagdoll();

        destructible = GetComponentInChildren<Destructible>();

        if (destructible) {
            destructible.DestroyedByRigidBodyImpactEvent += (body) => {
                Debug.Log($"Transfering velocity {body.linearVelocity}");
                ragDoll.TransferVelocity(body, knockbackMultiplier);
            };
            destructible.DestroyedEvent += () => {
                OnDestroyed();
            };
        }
        pather.Initialize();
    }
}
