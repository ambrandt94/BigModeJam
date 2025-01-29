using UnityEngine;
using System.Collections;

public class ResizeEffectHandler : MonoBehaviour
{
    public void StartResizing(Transform target, Vector3 totalSizeChange, float duration)
    {
        StartCoroutine(ResizeOverTime(target, totalSizeChange, duration));
    }

    private IEnumerator ResizeOverTime(Transform target, Vector3 totalSizeChange, float duration)
    {
        Vector3 initialScale = target.localScale;
        Vector3 targetScale = initialScale + totalSizeChange;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            target.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is set precisely
        target.localScale = targetScale;

        // Cleanup: Remove this component if it's no longer needed
        Destroy(this);
    }
}
