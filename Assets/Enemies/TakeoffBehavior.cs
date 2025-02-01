using UnityEngine;

public class TakeoffBehavior : MonoBehaviour
{
    public float minHeight = 5f;
    public float takeoffSpeed = 3f;
    public Transform takeoffRaycastOrigin;
    public bool HasTakenOff { get; private set; } = true;

    private FlyingEnemy enemy;

    public bool CheckTakeoff(FlyingEnemy _enemy)
    {
        if (enemy == null)
            this.enemy = _enemy;
        if (Physics.Raycast(takeoffRaycastOrigin.position, Vector3.down, out RaycastHit hit) && hit.distance < minHeight)
        {
            HasTakenOff = false;
        }
        else
        {
            HasTakenOff = true;
        }
        return HasTakenOff;
    }

    public void PerformTakeoff()
    {
        if (HasTakenOff) return;

        transform.position += Vector3.up * takeoffSpeed * Time.deltaTime;
        CheckTakeoff(enemy);
    }
}
