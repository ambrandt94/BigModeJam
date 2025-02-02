using ChainLink.Core;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagdollMaker : MonoBehaviour
{
    [System.Serializable]
    public struct Bone
    {
        [FoldoutGroup("@boneTransformString")]
        public HumanBodyBones boneTransformString;
        [FoldoutGroup("@boneTransformString")]
        public HumanBodyBones connectedToString;
        [FoldoutGroup("@boneTransformString")]
        public bool useCapsuleCollider;
        [FoldoutGroup("@boneTransformString")]
        public int capsuleDirection;
        [FoldoutGroup("@boneTransformString")]
        public Vector2 capsuleSize;
        [FoldoutGroup("@boneTransformString")]
        public Vector3 colliderSize;
        [FoldoutGroup("@boneTransformString")]
        public Vector3 colliderOffset;
        [FoldoutGroup("@boneTransformString")]
        public float jointSwingLimit;
        [FoldoutGroup("@boneTransformString")]
        public float jointTwistLimit;
    }

    public Animator Animator {
        get {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            return animator;
        }
    }

    public Bone[] bones;
    public float totalMass = 10f;

    [SerializeField]
    private Rigidbody mainBody;
    [SerializeField]
    private List<Collider> colliders;
    [SerializeField]
    private List<CharacterJoint> joints;
    [SerializeField]
    private List<Rigidbody> rigidbodies;

    private Animator animator;

    void Start()
    {
        CreateRagdoll();
    }

    public void TransferVelocity(Rigidbody targetBody, float modifier = 1)
    {
        Vector3 forcePerBody = (targetBody.linearVelocity / rigidbodies.Count) * modifier;
        foreach (Rigidbody body in rigidbodies) {
            body.linearVelocity = forcePerBody;
        }
    }

    [Button("Toggle Active")]
    public void ToggleActive(bool active)
    {
        Vector3 pos = Animator.GetBoneTransform(HumanBodyBones.Chest).position;
        if (rigidbodies != null) {
            foreach (Rigidbody body in rigidbodies) {
                if (!active) { 
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                }
                body.isKinematic = !active;
            }
        }
        if (colliders != null) { 
        foreach(Collider collider in colliders) {
                collider.isTrigger = !active;
            }
        }
        Animator.enabled = !active;
        transform.position = pos;
    }

    [Button("Create")]
    public void CreateRagdoll()
    {
        if (bones.Length == 0) {
            Debug.LogError("No bones assigned!");
            return;
        }
        joints = new List<CharacterJoint>();
        rigidbodies = new List<Rigidbody>();
        colliders = new List<Collider>();

        float massPerBone = totalMass / bones.Length;

        foreach (var bone in bones) {
            Transform boneTransform = Animator.GetBoneTransform(bone.boneTransformString);
            Transform connectedTo = Animator.GetBoneTransform(bone.connectedToString);
            if (connectedTo == boneTransform)
                connectedTo = null;
            if (!boneTransform) {
                Debug.LogError("Couldn't find bone");
                continue;
            }

            // Add Rigidbody
            Rigidbody rb = boneTransform.gameObject.GetOrAddComponent<Rigidbody>();
            rigidbodies.Add(rb);
            rb.mass = massPerBone;

            // Add Collider
            if (bone.useCapsuleCollider) {
                CapsuleCollider capsule = boneTransform.gameObject.GetOrAddComponent<CapsuleCollider>();
                colliders.Add(capsule);
                capsule.radius = bone.capsuleSize.x;
                capsule.height = bone.capsuleSize.y;
                capsule.direction = bone.capsuleDirection;
                capsule.center = bone.colliderOffset;
            } else {
                BoxCollider box = boneTransform.gameObject.GetOrAddComponent<BoxCollider>();
                colliders.Add(box);
                box.size = bone.colliderSize;
                box.center = bone.colliderOffset;
            }

            // Add CharacterJoint if connected to another bone
            if (connectedTo) {
                CharacterJoint joint = boneTransform.gameObject.GetOrAddComponent<CharacterJoint>();
                joints.Add(joint);
                joint.connectedBody = connectedTo.GetComponent<Rigidbody>();

                // Configure Joint Limits
                SoftJointLimit swingLimit = new SoftJointLimit { limit = bone.jointSwingLimit };
                joint.swing1Limit = swingLimit;
                joint.swing2Limit = swingLimit;

                SoftJointLimit twistLimit = new SoftJointLimit { limit = bone.jointTwistLimit };
                joint.lowTwistLimit = twistLimit;
                joint.highTwistLimit = twistLimit;
            }
        }
        foreach (var bone in bones) {
            Transform boneTransform = Animator.GetBoneTransform(bone.boneTransformString);
            Transform connectedTo = Animator.GetBoneTransform(bone.connectedToString);
            if (connectedTo == boneTransform)
                connectedTo = null;

            // Add CharacterJoint if connected to another bone
            if (connectedTo) {
                CharacterJoint joint = boneTransform.gameObject.GetOrAddComponent<CharacterJoint>();
                joint.connectedBody = connectedTo.GetComponent<Rigidbody>();

                // Configure Joint Limits
                SoftJointLimit swingLimit = new SoftJointLimit { limit = bone.jointSwingLimit };
                joint.swing1Limit = swingLimit;
                joint.swing2Limit = swingLimit;

                SoftJointLimit twistLimit = new SoftJointLimit { limit = bone.jointTwistLimit };
                joint.lowTwistLimit = twistLimit;
                joint.highTwistLimit = twistLimit;
            } else {
            }
        }
    }

    [Button("Copy Values")]
    void CoyCapsuleValues()
    {
        if (bones.Length == 0) {
            Debug.LogError("No bones assigned!");
            return;
        }

        for (int i = 0; i < bones.Length; i++) {
            Bone bone = bones[i];
            Transform boneTransform = Animator.GetBoneTransform(bone.boneTransformString);
            if (bone.useCapsuleCollider) {
                Debug.Log($"Copying capsule on {boneTransform.gameObject.name}");
                CapsuleCollider col = boneTransform.GetComponent<CapsuleCollider>();
                bone.capsuleDirection = col.direction;
                bone.capsuleSize = new Vector2(col.radius, col.height);
                bone.colliderOffset = col.center;
            } else {
                Debug.Log($"Copying box on {boneTransform.gameObject.name}");
                BoxCollider col = boneTransform.GetComponent<BoxCollider>();
                bone.colliderSize = col.size;
                bone.colliderOffset = col.center;
            }
            bones[i] = bone;
        }

        Debug.Log("Ragdoll copied");
    }
}