using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private float arcRadius = 3f;  // ✅ Distance from the waypoint for the turn arc
    [SerializeField] private float turnSegmentAngle = 15f;  // ✅ Angle increments for smooth arc

    private List<Vector3> smoothPath = new List<Vector3>();

    public List<Vector3> GetSmoothPath()
    {
        smoothPath.Clear();

        if (waypoints.Count < 2)
        {
            Debug.LogError("PathManager: Not enough waypoints to generate a path.");
            return smoothPath;
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 prevPoint = waypoints[(i - 1 + waypoints.Count) % waypoints.Count].position;
            Vector3 currentPoint = waypoints[i].position;
            Vector3 nextPoint = waypoints[(i + 1) % waypoints.Count].position;

            // ✅ Compute turn angle
            float turnAngle = Vector3.SignedAngle(prevPoint - currentPoint, nextPoint - currentPoint, Vector3.up);
            bool isLeftTurn = turnAngle > 0;

            // ✅ Get the correct first control point for a proper outer arc
            Vector3 firstTurnPoint = GetCorrectTurnStart(prevPoint, currentPoint, nextPoint, turnAngle);

            // ✅ Generate arc points along the outer side of the turn
            List<Vector3> turnArc = GenerateOuterTurnArc(firstTurnPoint, currentPoint, nextPoint, turnAngle, isLeftTurn);

            // ✅ Ensure correct order
            smoothPath.Add(firstTurnPoint);
            smoothPath.AddRange(turnArc);
        }

        Debug.Log($"Generated AI Path with {smoothPath.Count} points");
        return smoothPath;
    }

    private Vector3 GetCorrectTurnStart(Vector3 prev, Vector3 current, Vector3 next, float turnAngle)
    {
        Vector3 toPrev = (prev - current).normalized;
        Vector3 toNext = (next - current).normalized;

        // ✅ Compute perpendicular vector to determine correct *outside* turn direction
        Vector3 turnDirection = Vector3.Cross(toPrev, toNext).normalized;
        Vector3 perpendicular = Vector3.Cross(turnDirection, toNext).normalized;

        // ✅ Move outward perpendicular to the turn before starting the arc
        Vector3 offset = perpendicular * arcRadius;

        Debug.DrawRay(current, offset, Color.blue, 5f);

        return current + offset;
    }

    private List<Vector3> GenerateOuterTurnArc(Vector3 startPoint, Vector3 centerPoint, Vector3 endPoint, float turnAngle, bool isLeftTurn)
    {
        List<Vector3> turnPoints = new List<Vector3>();

        // ✅ The total angle we want to cover is `180 - turnAngle` to stay on the outside of the turn
        float totalArcAngle = Mathf.Clamp(180 - Mathf.Abs(turnAngle), 45, 180);

        // ✅ Determine how many segments are needed
        int numSegments = Mathf.Max(1, Mathf.CeilToInt(totalArcAngle / turnSegmentAngle));

        Vector3 startDir = (startPoint - centerPoint).normalized;

        for (int i = 1; i <= numSegments; i++)
        {
            float angle = i * (totalArcAngle / numSegments);
            Vector3 rotatedDir = Quaternion.Euler(0, angle, 0) * startDir;
            Vector3 newPoint = centerPoint + rotatedDir * arcRadius;

            turnPoints.Add(newPoint);
            Debug.DrawRay(centerPoint, rotatedDir * arcRadius, Color.red, 125f);
        }

        // ✅ Reverse the order for left turns
        if (isLeftTurn)
        {
            turnPoints.Reverse();
        }

        return turnPoints;
    }
}
