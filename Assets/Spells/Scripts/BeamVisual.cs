using UnityEngine;

public class BeamVisual : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public void Apply(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
}
