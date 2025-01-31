using UnityEngine;
using System.Collections.Generic;

public class SmoothPatrolBehavior : MonoBehaviour
{
    [SerializeField] private PathManager pathManager;
    [SerializeField] private float baseSpeed = 12f;      // ✅ Default cruise speed
    [SerializeField] private float maxSpeed = 18f;       // ✅ Max speed on straights
    [SerializeField] private float minTurnSpeed = 10f;   // ✅ Min speed in turns
    [SerializeField] private float acceleration = 4f;    // ✅ Smooth acceleration rate
    [SerializeField] private float deceleration = 3.5f;  // ✅ Smooth deceleration rate
    [SerializeField] private float minWaypointDistance = 4f; // ✅ Waypoints closer than this will not slow down AI
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float tiltAmount = 55f;   // ✅ Increased tilting for sharper turns
    [SerializeField] private float tiltSpeed = 10f;
    [SerializeField] private float waypointThreshold = 1.5f;
    [SerializeField] private float waypointCooldownTime = 0.2f; // ✅ Faster responsiveness
    [SerializeField] private float smoothingFactor = 0.4f;

    private List<Vector3> path;

    public void SetPath(List<Vector3> _path)
    {
        this.path = _path;
    }

    public void SetPathManager(PathManager _pathManager)
    {
        this.pathManager = _pathManager;
    }
    private int targetIndex = 0;
    private bool isPatrolling = false;
    private float lastWaypointTime = 0f;
    private float currentSpeed;

    public void StartPatrolling()
    {
        path = pathManager?.GetSmoothPath();

        if (path.Count < 2)
        {
            Debug.LogError("SmoothPatrolBehavior: Not enough path points to start patrolling.");
            return;
        }

        transform.position = path[0];
        targetIndex = 1;
        isPatrolling = true;
        lastWaypointTime = Time.time;
        currentSpeed = baseSpeed; // ✅ Start at base cruise speed
    }

    public void StopPatrolling()
    {
        isPatrolling = false;
    }

    private void Update()
    {
        if (!isPatrolling || path.Count < 2) return;

        Vector3 targetPosition = path[targetIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // ✅ Prevent AI from slowing down excessively in short waypoint segments
        if (distanceToTarget < minWaypointDistance)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            float turnSeverity = 1f - Vector3.Dot(transform.forward, moveDirection);
            float targetSpeed = Mathf.Lerp(maxSpeed, minTurnSpeed, turnSeverity);
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * (targetSpeed > currentSpeed ? acceleration : deceleration));
        }

        // ✅ Apply movement using a natural transition
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingFactor * Time.deltaTime * currentSpeed);

        // ✅ Rotate AI smoothly towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // ✅ Improved Tilt: Increase tilt for a more dramatic banking motion
        float turnDirection = Vector3.Dot(transform.right, moveDirection);
        float tiltFactor = Mathf.Lerp(0f, tiltAmount, Mathf.Abs(turnDirection));
        transform.rotation *= Quaternion.Euler(0, 0, Mathf.Lerp(0, -tiltFactor, tiltSpeed * Time.deltaTime));

        if (HasPassedWaypoint(targetIndex) && Time.time - lastWaypointTime > waypointCooldownTime)
        {
            lastWaypointTime = Time.time;
            targetIndex = (targetIndex + 1) % path.Count;
        }
    }

    private bool HasPassedWaypoint(int index)
    {
        if (index < 1 || index >= path.Count) return false;

        Vector3 lastPosition = path[index - 1];
        Vector3 targetPosition = path[index];
        Vector3 moveDirection = (targetPosition - lastPosition).normalized;
        Vector3 aiDirection = (transform.position - lastPosition).normalized;

        return Vector3.Dot(moveDirection, aiDirection) > 0.95f;
    }
}
