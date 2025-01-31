using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolBehavior : MonoBehaviour
{
    [SerializeField] private List<Transform> patrolPoints;
    [SerializeField] private float patrolSpeed = 5f;
    [SerializeField] private float orbitSpeed = 3f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float tiltAmount = 20f;
    [SerializeField] private float tiltSpeed = 3f;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float speedBlendFactor = 1.5f;
    [SerializeField] private float passDistanceFactor = 1.5f; // ✅ How far past the waypoint before orbiting
    [SerializeField] private float exitAngleTolerance = 5f; // ✅ How close to facing the next waypoint before exiting orbit

    private int currentPatrolIndex = 0;
    private bool isOrbiting = false;
    private float currentTilt = 0f;
    private Coroutine patrolCoroutine;
    private Vector3 orbitCenter; // ✅ Set ONCE per orbit

    private void Start()
    {
        if (patrolPoints.Count < 2)
        {
            Debug.LogError("PatrolBehavior: Need at least two patrol points.");
            return;
        }

        Patrol();
    }

    public void Patrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
        }

        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            Transform nextPoint = patrolPoints[(currentPatrolIndex + 1) % patrolPoints.Count];

            yield return MoveToWaypoint(targetPoint, nextPoint); // ✅ Ensure AI reaches the waypoint first

            yield return MovePastWaypoint(targetPoint, nextPoint); // ✅ Then move slightly past before orbiting

            yield return OrbitUntilAligned(targetPoint, nextPoint); // ✅ Orbit only if past the waypoint

            MoveToNextPatrolPoint();
        }
    }

    private IEnumerator MoveToWaypoint(Transform target, Transform next)
    {
        while (Vector3.Distance(transform.position, target.position) > orbitRadius)
        {
            Vector3 moveDirection = (target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            float currentSpeed = patrolSpeed;
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private IEnumerator MovePastWaypoint(Transform target, Transform next)
    {
        Vector3 moveDirection = (next.position - target.position).normalized;
        Vector3 passPoint = target.position + (moveDirection * orbitRadius * passDistanceFactor); // ✅ Move past waypoint

        while (!HasPassedWaypoint(target, moveDirection)) // ✅ Check if AI has passed waypoint
        {
            float speedFactor = Mathf.Clamp01(Vector3.Distance(transform.position, passPoint) / (orbitRadius * speedBlendFactor));
            float currentSpeed = Mathf.Lerp(orbitSpeed, patrolSpeed, speedFactor);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, passPoint, currentSpeed * Time.deltaTime);

            yield return null;
        }

        orbitCenter = target.position; // ✅ Set orbit center ONCE here, after passing the waypoint
    }

    private IEnumerator OrbitUntilAligned(Transform center, Transform nextPoint)
    {
        isOrbiting = true;
        Vector3 directionToNext = (nextPoint.position - center.position).normalized;
        float orbitDirection = Vector3.Dot(transform.right, directionToNext) > 0 ? 1f : -1f;

        while (!IsFacingNextWaypoint(directionToNext)) // ✅ Continue orbiting until properly aligned
        {
            Vector3 orbitOffset = (transform.position - orbitCenter).normalized * orbitRadius;
            orbitOffset = Quaternion.Euler(0, orbitDirection * (90f * Time.deltaTime), 0) * orbitOffset;
            transform.position = orbitCenter + orbitOffset;

            Quaternion targetRotation = Quaternion.LookRotation(directionToNext);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            float inwardTilt = orbitDirection * -tiltAmount;
            currentTilt = Mathf.Lerp(currentTilt, inwardTilt, Time.deltaTime * tiltSpeed);
            transform.rotation *= Quaternion.Euler(0, 0, currentTilt);

            yield return null;
        }

        isOrbiting = false;
    }

    private bool IsFacingNextWaypoint(Vector3 directionToNext)
    {
        float angle = Vector3.Angle(transform.forward, directionToNext);
        return angle < exitAngleTolerance; // ✅ AI will exit orbit when it's facing within `exitAngleTolerance` degrees of the next waypoint
    }

    private bool HasPassedWaypoint(Transform waypoint, Vector3 directionToNext)
    {
        Vector3 toWaypoint = waypoint.position - transform.position;
        return Vector3.Dot(toWaypoint, directionToNext) < 0; // ✅ AI has passed if the dot product is negative
    }

    private void MoveToNextPatrolPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }

    public void StopPatrol()
    {
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
    }

    public void SetPatrolPoints(List<Transform> newPatrolPoints)
    {
        patrolPoints = newPatrolPoints;
        currentPatrolIndex = 0;
        Patrol();
    }
}
