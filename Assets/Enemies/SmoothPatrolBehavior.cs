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

        //transform.position = path[0];
        //targetIndex = 0;
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

        // ✅ Set AI to a constant speed regardless of waypoint spacing
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);

        // ✅ Rotate AI smoothly towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // ✅ Increase tilt when turning
        float turnDirection = Vector3.Dot(transform.right, moveDirection);
        float tiltFactor = Mathf.Lerp(0f, tiltAmount, Mathf.Abs(turnDirection));
        transform.rotation *= Quaternion.Euler(0, 0, Mathf.Lerp(0, -tiltFactor, tiltSpeed * Time.deltaTime));

        // ✅ Debug: Show AI movement trail
        Debug.DrawRay(transform.position, moveDirection * 3f, Color.red);

        // ✅ Ensure AI properly loops back after reaching last waypoint
        if (HasPassedWaypoint(targetIndex) && Time.time - lastWaypointTime > waypointCooldownTime)
        {
            lastWaypointTime = Time.time;

            if (targetIndex == path.Count - 1)
            {
                // ✅ Loop back to first waypoint
                targetIndex = 0;
            }
            else
            {
                // ✅ Move to the next waypoint
                targetIndex++;
            }

            Debug.Log($"✅ Switching to waypoint {targetIndex}");
        }
    }




    private bool HasPassedWaypoint(int index)
    {
        if (index < 0 || index >= path.Count) return false;

        Vector3 lastPosition = (index == 0) ? path[path.Count - 1] : path[index - 1]; // ✅ Correctly get the last position even when looping
        Vector3 targetPosition = path[index];
        Vector3 moveDirection = (targetPosition - lastPosition).normalized;
        Vector3 aiDirection = (transform.position - lastPosition).normalized;

        // ✅ Ensure AI smoothly transitions when looping back to the first waypoint
        return Vector3.Dot(moveDirection, aiDirection) > 0.85f;
    }

}
