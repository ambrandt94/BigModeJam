using DestroyIt;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TelekineticDestroyerBehavior : MonoBehaviour
{
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float throwForce = 40f;
    [SerializeField] private float liftHeight = 10f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float approachDistance = 30f; // ✅ Move closer before lifting
    [SerializeField] private LayerMask destructibleLayer;
    [SerializeField] private LineRenderer telekinesisBeam; // ✅ Beam effect
    [SerializeField] private ParticleSystem telekinesisEffect;
    [SerializeField] private ChaseBehavior chaseBehavior;

    private Transform player;
    private Destructible currentTarget;
    public void SetCurrentTarget(Destructible target)
    {
        currentTarget = target;
    }

    private Rigidbody liftedObject;
    private bool isThrowing = false;
    public bool IsThrowing() { return  this.isThrowing; }

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        //StartCoroutine(ScanForTargets());
    }

    private IEnumerator ScanForTargets()
    {
        while (true)
        {
            if (!isThrowing)
            {
                currentTarget = FindSuitableDestructible();
                if (currentTarget != null)
                {
                    yield return StartCoroutine(LiftAndThrow(currentTarget));
                }
            }
            yield return new WaitForSeconds(1f); // ? Scan every second
        }
    }

    public void TryToLift(Destructible target)
    {
        currentTarget = target;
        if (currentTarget != null && !isThrowing)
        {
            StartCoroutine(MoveToAndLift(currentTarget));
        }
    }

    private IEnumerator MoveToAndLift(Destructible target)
    {
        isThrowing = true;

        // ✅ Move closer to the building before lifting
        while (Vector3.Distance(transform.position, target.transform.position) > approachDistance)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, chaseBehavior.chaseSpeed* Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 5f * Time.deltaTime);
            yield return null;
        }

        yield return StartCoroutine(LiftAndThrow(target));
    }

    public Destructible FindSuitableDestructible()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, destructibleLayer);
        Destructible destructible = null;
        foreach (Collider hit in hits)
        {
            BuildingSegment destructibleSegment = hit.transform.parent?.GetComponent<BuildingSegment>();
            if (destructibleSegment != null)
            {
                Destructible hitDestructible = hit.transform.parent?.GetComponent<Destructible>();

                if (destructible == null)
                    destructible = hitDestructible;

                if (hitDestructible != null && (hitDestructible.transform.position.y > destructible.transform.position.y))
                {
                    destructible = hitDestructible;
                }
            }
        }
        return destructible;
    }

    public bool CanBeLifted(Destructible destructible)
    {   
        if (!destructible.transform.GetChild(0).TryGetComponent<BoxCollider>(out BoxCollider collider))
        {
            Debug.LogWarning($"No BoxCollider found on {destructible.gameObject.name}, skipping lift check.");
            return false;
        }

        float offset = 0.1f; // Small offset to avoid self-collision
        Vector3 boxTop = collider.bounds.center + new Vector3(0, collider.bounds.extents.y + offset, 0);

        // Cast multiple rays from the outer corners of the box
        Vector3[] raycastOrigins = new Vector3[]
        {
        boxTop + new Vector3(collider.bounds.extents.x, 0, collider.bounds.extents.z),  // Front-Right
        boxTop + new Vector3(-collider.bounds.extents.x, 0, collider.bounds.extents.z), // Front-Left
        boxTop + new Vector3(collider.bounds.extents.x, 0, -collider.bounds.extents.z), // Back-Right
        boxTop + new Vector3(-collider.bounds.extents.x, 0, -collider.bounds.extents.z) // Back-Left
        };

        foreach (Vector3 origin in raycastOrigins)
        {
            if (Physics.Raycast(origin, Vector3.up, out RaycastHit hit, collider.bounds.extents.y * 2))
            {
                if (hit.collider.GetComponent<Destructible>() != null)
                {
                    return false; // Something is on top, cannot be lifted
                }
            }
        }

        return true; // No obstacles detected, can be lifted
    }


    private IEnumerator LiftAndThrow(Destructible target)
    {
        isThrowing = true;

        // ? Lift the object
        liftedObject = target.GetComponent<Rigidbody>();
        if (liftedObject != null)
        {
            liftedObject.isKinematic = true; // Suspend physics
            Vector3 liftPosition = target.transform.position + Vector3.up * liftHeight;

            // ✅ Activate telekinesis effects
            if (telekinesisEffect != null) telekinesisEffect.Play();
            if (telekinesisBeam != null)
            {
                telekinesisBeam.enabled = true;
                telekinesisBeam.SetPosition(0, transform.position);
                telekinesisBeam.SetPosition(1, liftedObject.transform.position);
            }

            float elapsedTime = 0;
            Vector3 startPosition = target.transform.position;
            while (elapsedTime < 1f)
            {
                liftedObject.transform.position = Vector3.Lerp(startPosition, liftPosition, elapsedTime);
                elapsedTime += Time.deltaTime;

                // ✅ Update telekinesis beam
                if (telekinesisBeam != null)
                {
                    telekinesisBeam.SetPosition(0, transform.position);
                    telekinesisBeam.SetPosition(1, liftedObject.transform.position);
                }

                yield return null;
            }

            yield return new WaitForSeconds(holdTime); // ? Hold in the air before throwing

            // ? Throw the object at the player
            liftedObject.isKinematic = false;
            liftedObject.GetComponent<BuildingSegment>()?.ToggleConstraints(false);
            Vector3 throwDirection = (player.position - liftedObject.transform.position).normalized;
            liftedObject.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            // ✅ Turn off telekinesis beam
            if (telekinesisBeam != null) telekinesisBeam.enabled = false;
        }

        yield return new WaitForSeconds(5f); // ? Cooldown before next throw
        isThrowing = false;
    }
}
