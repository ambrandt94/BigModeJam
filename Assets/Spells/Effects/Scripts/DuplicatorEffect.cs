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
        if (target == null)
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

        Vector3 targetTotalScale = GetTotalScale(target);

        // Start at a smaller scale (relative to the target's total scale)
        clone.transform.localScale = new Vector3(
            targetTotalScale.x * 0.02f,
            targetTotalScale.y * 0.02f,
            targetTotalScale.z * 0.02f);

        CoroutineRunner.instance.StartCoroutine(ScaleUp(clone.transform, targetTotalScale, spawnScaleDuration));

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

    private IEnumerator ScaleUp(Transform obj, Vector3 targetTotalScale, float duration)
    {
        float time = 0f;
        Vector3 startScale = obj.localScale;
        Vector3 endScale = targetTotalScale; // Scale up to the target's total scale

        while (time < duration)
        {
            time += Time.deltaTime;
            obj.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            yield return null;
        }

        obj.localScale = endScale; // Ensure final size is exactly correct
    }


    // Recursive function to get the total scale of an object (including parents)
    private Vector3 GetTotalScale(Transform obj)
    {
        Vector3 totalScale = Vector3.one;
        Transform currentTransform = obj;

        while (currentTransform != null)
        {
            totalScale = new Vector3(
                totalScale.x * currentTransform.localScale.x,
                totalScale.y * currentTransform.localScale.y,
                totalScale.z * currentTransform.localScale.z);

            currentTransform = currentTransform.parent;
        }

        return totalScale;
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
