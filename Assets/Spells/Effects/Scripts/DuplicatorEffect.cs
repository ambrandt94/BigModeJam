using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[CreateAssetMenu(menuName = "SpellEffects/DuplicatorEffect")]
public class DuplicatorEffect : ScriptableObject, ISpellEffect
{
    public float stackingBuffer = 0.05f; // Tiny buffer to prevent z-fighting
    public bool inheritVelocity = false; // Should the duplicated object keep velocity?
    public bool enableGravity = true;    // Should the duplicated object fall?
    public float spawnScaleDuration = 0.25f; // Time it takes to scale up

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        if (target == null || target.GetComponent<Renderer>() == null)
        {
            Debug.LogWarning("DuplicatorEffect: Target is not a valid object for duplication.");
            return;
        }

        // Get accurate top position of the object
        Bounds objectBounds = GetObjectBounds(target);
        Vector3 highestPoint = objectBounds.center + Vector3.up * objectBounds.extents.y;

        // Calculate the exact spawn position for the duplicate
        Vector3 spawnPosition = highestPoint + Vector3.up * stackingBuffer;

        // Clone the object
        GameObject clone = Instantiate(target.gameObject, spawnPosition, target.rotation);
        clone.name = target.name + "_Duplicate";

        // Start at a smaller scale and animate to full scale
        clone.transform.localScale *= 0.02f;
        CoroutineRunner.instance.StartCoroutine(ScaleUp(clone.transform, 1f, spawnScaleDuration));

        // Handle Rigidbody properties
        Rigidbody cloneRb = clone.GetComponent<Rigidbody>();
        if (cloneRb)
        {
            cloneRb.useGravity = enableGravity;

            if (inheritVelocity && target.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
            {
                cloneRb.linearVelocity = targetRb.linearVelocity; // Copy velocity
            }
            else
            {
                cloneRb.linearVelocity = Vector3.zero; // No unwanted movement
            }
        }

        Debug.Log($"DuplicatorEffect: Cloned '{target.name}' at {spawnPosition}");
    }

    // Smoothly scale the clone from 0.1x to full size
    private IEnumerator ScaleUp(Transform obj, float targetScale, float duration)
    {
        float time = 0f;
        Vector3 startScale = obj.localScale;
        Vector3 endScale = Vector3.one * targetScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            obj.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            yield return null;
        }

        obj.localScale = endScale; // Ensure final size is exactly 1
    }

    // Helper method to calculate the object's total bounds (handles complex objects)
    private Bounds GetObjectBounds(Transform obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.position, Vector3.one * 0.5f); // Default small size

        Bounds combinedBounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            combinedBounds.Encapsulate(renderer.bounds);
        }
        return combinedBounds;
    }
}
