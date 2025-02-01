using UnityEngine;

[CreateAssetMenu(menuName = "SpellEffects/TelekinesisEffect")]
public class TelekinesisEffect : ScriptableObject, ISpellEffect
{
    public float moveSpeed = 15f;      // Speed of movement
    public float depthSpeed = 5f;      // Q/E forward/backward movement speed
    public float maxDistance = 10f;    // Maximum distance from the caster
    public float minDistance = 1f;     // Minimum distance from the caster
    public float followSmoothness = 10f; // How smoothly the object follows trackingTransform
    public float forgivenessTime = 0.3f; // Time before dropping an object when the raycast is lost

    private Transform telekinesisTarget;
    private Rigidbody targetRigidbody;
    private Transform trackingTransform;
    private SpellCaster caster;

    public void SetCaster(SpellCaster _caster)
    {
        this.caster = _caster;
    }
    private float initialDistance; // The distance at which the object was first hit
    private float lastHitTime; // Tracks last valid raycast hit

    public void Apply(Transform target, Vector3 hitPoint, float deltaTime)
    {
        if (telekinesisTarget == null && caster != null)
        {
            telekinesisTarget = target;
            targetRigidbody = target.GetComponent<Rigidbody>();
            //caster = FindObjectOfType<SpellCaster>();
            lastHitTime = Time.time;

            if (targetRigidbody)
            {
                targetRigidbody.useGravity = false;
                targetRigidbody.isKinematic = true;
            }

            // Create tracking transform at the exact hit location
            trackingTransform = new GameObject("TelekinesisTracking").transform;
            trackingTransform.position = hitPoint;
            initialDistance = Vector3.Distance(caster.transform.position, hitPoint);

            // ✅ Debug Sphere to visualize trackingTransform
            GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.localScale = Vector3.one * 0.2f;
            debugSphere.transform.position = trackingTransform.position;
            debugSphere.name = "Debug_TelekinesisTracker";
            Object.Destroy(debugSphere.GetComponent<Collider>());
            debugSphere.transform.SetParent(trackingTransform, true);
        }
        else
        {
            lastHitTime = Time.time;
        }
    }

    public void UpdateEffect(bool hasValidRaycast)
    {
        if (telekinesisTarget == null || caster == null) return;

        if (!hasValidRaycast && Time.time - lastHitTime > forgivenessTime)
        {
            Release();
            return;
        }

        // ✅ Fix: Get the exact world position where the player is aiming
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        Vector3 newTargetPosition;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            newTargetPosition = hit.point;
        }
        else
        {
            newTargetPosition = ray.GetPoint(initialDistance); // Keep it at a fixed depth
        }

        // ✅ Fix: Allow Q/E to control depth
        float depthAdjustment = (Input.GetKey(KeyCode.Q) ? -depthSpeed : (Input.GetKey(KeyCode.E) ? depthSpeed : 0)) * Time.deltaTime;
        initialDistance = Mathf.Clamp(initialDistance + depthAdjustment, minDistance, maxDistance);
        newTargetPosition = ray.origin + ray.direction * initialDistance; // Adjust the depth

        // ✅ Fix: Move the tracking transform smoothly
        trackingTransform.position = Vector3.Lerp(trackingTransform.position, newTargetPosition, Time.deltaTime * followSmoothness);

        // Debug log tracking position
        Debug.Log($"[TRACKING] Tracking Position: {trackingTransform.position}, Distance: {initialDistance}");

        // ✅ Fix: Ensure object follows trackingTransform precisely
        if (targetRigidbody)
        {
            targetRigidbody.MovePosition(Vector3.Lerp(targetRigidbody.position, trackingTransform.position, Time.deltaTime * followSmoothness));
        }
        else
        {
            telekinesisTarget.position = Vector3.Lerp(telekinesisTarget.position, trackingTransform.position, Time.deltaTime * followSmoothness);
        }
    }

    public void Release()
    {
        if (telekinesisTarget != null && targetRigidbody != null)
        {
            targetRigidbody.useGravity = true;
            targetRigidbody.isKinematic = false;
        }

        if(trackingTransform != null)
            Destroy(trackingTransform.gameObject);

        telekinesisTarget = null;
        targetRigidbody = null;
    }

    // Debugging Helpers
    public bool HasTarget() => telekinesisTarget != null;
    public Vector3 GetTargetPosition() => telekinesisTarget ? telekinesisTarget.position : Vector3.zero;
}
