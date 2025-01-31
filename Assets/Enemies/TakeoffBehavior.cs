using UnityEngine;

public class TakeoffBehavior : MonoBehaviour
{
    public float minHeight = 5f;
    public float takeoffSpeed = 3f;
    public Transform takeoffRaycastOrigin;
    public bool HasTakenOff { get; private set; } = true;

    public bool CheckTakeoff(FlyingEnemy enemy)
    {
        if (Physics.Raycast(takeoffRaycastOrigin.position, Vector3.down, out RaycastHit hit) && hit.distance < minHeight)
        {
            HasTakenOff = false;
        }
        return HasTakenOff;
    }

    public void PerformTakeoff()
    {
        if (HasTakenOff) return;

        transform.position += Vector3.up * takeoffSpeed * Time.deltaTime;
        if (transform.position.y >= minHeight)
        {
            HasTakenOff = true;
        }
    }
}
