using UnityEngine;

public class DetectionSystem : MonoBehaviour
{
    public float detectionRange = 20f;
    public LayerMask obstaclesLayer; // Assign this in the inspector to include walls, terrain, etc.

    private Transform player;
    private Collider selfCollider;
    public Transform enemyEyePosition;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        selfCollider = GetComponent<Collider>(); // Get enemy's own collider
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;
        if (Vector3.Distance(transform.position, player.position) > detectionRange) return false;     

        // Ensure we aim for the player's center
        Vector3 playerTarget = player.position + Vector3.up * 1f; // Adjust for player's collider height
        Vector3 direction = (playerTarget - enemyEyePosition.position).normalized;

        // Perform a single raycast (more efficient than RaycastAll)
        if (Physics.Raycast(enemyEyePosition.position, direction, out RaycastHit hit, detectionRange, ~obstaclesLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == selfCollider) return false; // Ignore self
            if (hit.transform.CompareTag("Player")) return true; // Player is visible
            return false; // Something is blocking sight
        }

        return false; // No hit = no player detected
    }
}
