    using System;
using UnityEngine;

// FlyBehaviour inherits from GenericBehaviour. This class corresponds to the flying behaviour.
public class FlyBehaviour : GenericBehaviour
{
    public string flyButton = "Fly";              // Default fly button.
    public float flySpeed = 100f;                 // Default flying speed.
    public float sprintFactor = 2.0f;             // How much sprinting affects fly speed.
    public float flyMaxVerticalAngle = 75f;       // Angle to clamp camera vertical movement when flying.
    public float bounceThresholdSpeed = 7.5f;     // Speed threshold to cause a bounce effect off of objects.
    public float collisionDamageThresholdSpeed = 15.0f; // Speed threshold to cause damage.
    public float slowDownFactor = 0.5f;          // Factor by which to reduce speed when breaking through an object.
    public LayerMask destructibleLayers;         // Layers considered destructible.
    public float velocityTransitionSpeed = 5f;   // Speed of velocity interpolation.
    public float collisionImpactCooldown = 1f; // Time after collision during which FlyManagement cannot override speed.
    public float bounceDuration = 0.5f;          // Duration of the bounce state.

    private int flyBool;                          // Animator variable related to flying.
    private bool fly = false;                     // Boolean to determine whether or not the player activated fly mode.
    private CapsuleCollider col;                  // Reference to the player capsule collider.
    private Rigidbody rb;                         // Reference to the player's Rigidbody.
    private Vector3 desiredVelocity = Vector3.zero; // The target velocity set by FlyManagement or collision.
    private float collisionCooldownTimer = 0f;    // Timer to track collision impact cooldown.
    private bool isBouncing = false;              // Flag to indicate whether the player is in a bounce state.
    private float bounceTimer = 0f;

    [SerializeField] private ParticleSystem speedEffectParticleSystem;

    // Start is always called after any Awake functions.
    void Start()
    {
        flyBool = Animator.StringToHash("Fly");
        col = this.GetComponent<CapsuleCollider>();
        rb = this.GetComponent<Rigidbody>();
        behaviourManager.SubscribeBehaviour(this);
        speedEffectParticleSystem.gameObject.SetActive(true);
    }

    // Update is used to set features regardless the active behaviour.
    void Update()
    {
        if (Input.GetButtonDown(flyButton) && !behaviourManager.IsOverriding()
            && !behaviourManager.GetTempLockStatus(behaviourManager.GetDefaultBehaviour))
        {
            fly = !fly;
            behaviourManager.UnlockTempBehaviour(behaviourManager.GetDefaultBehaviour);
            rb.useGravity = !fly;

            if (fly)
            {
                behaviourManager.RegisterBehaviour(this.behaviourCode);
            }
            else
            {
                col.direction = 1;
                behaviourManager.GetCamScript.ResetTargetOffsets();
                behaviourManager.UnregisterBehaviour(this.behaviourCode);
            }
        }

        fly = fly && behaviourManager.IsCurrentBehaviour(this.behaviourCode);
        behaviourManager.GetAnim.SetBool(flyBool, fly);

        if (collisionCooldownTimer > 0f)
        {
            collisionCooldownTimer -= Time.deltaTime;
        }

        if (isBouncing)
        {
            bounceTimer -= Time.deltaTime;          
            if (bounceTimer <= 0f)
            {
                isBouncing = false;
            }
        }
    }  
   

    public override void LocalFixedUpdate()
    {
        behaviourManager.GetCamScript.SetMaxVerticalAngle(flyMaxVerticalAngle);

        if (!isBouncing && collisionCooldownTimer <= 0f)
        {
            FlyManagement(behaviourManager.GetH, behaviourManager.GetV);
        }
        else if (isBouncing)
        {
            Rotating(behaviourManager.GetH, behaviourManager.GetV);
            //speedEffectParticleSystem.Stop();
        }

        // Smoothly transition velocity to the desired velocity.
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, Time.fixedDeltaTime * velocityTransitionSpeed);
        if (rb.linearVelocity.magnitude >= collisionDamageThresholdSpeed)
        {
            speedEffectParticleSystem.Stop();          
            speedEffectParticleSystem.Play();
        }
        else
        {
            speedEffectParticleSystem.Stop();
        }
        Debug.Log("LocalFixedUpdate velocity: " + rb.linearVelocity.magnitude.ToString());
        //rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, Time.fixedDeltaTime * velocityTransitionSpeed);
    }

    void FlyManagement(float horizontal, float vertical)
    {
        Vector3 direction = Rotating(horizontal, vertical);
        float speed = flySpeed * (behaviourManager.IsSprinting() ? sprintFactor : 1);
        if (behaviourManager.IsSprinting())
        {
            speedEffectParticleSystem.Stop();
            speedEffectParticleSystem.Play();
        }
        else
        {
            speedEffectParticleSystem.Stop();
        }
        desiredVelocity = direction * speed;
    }

    Vector3 Rotating(float horizontal, float vertical)
    {

        if (isBouncing)
        {
            // During bounce, maintain velocity direction for rotation.
            Vector3 bounceDirection = lastReflectedDirection;

            // Align rotation with bounce direction.
            Quaternion bounceRotation = Quaternion.LookRotation(bounceDirection);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, bounceRotation, behaviourManager.turnSmoothing);
            rb.MoveRotation(newRotation);
            behaviourManager.SetLastDirection(bounceDirection);
            behaviourManager.Repositioning();

            // Return current direction to maintain movement.
            return bounceDirection;
        }
        else
        {
            // Get camera's forward direction without vertical influence.
            Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);
            forward.y = 0; // Lock Y-axis movement to prevent tilting
            forward.Normalize();

            // Compute right direction to allow horizontal movement.
            Vector3 right = new Vector3(forward.z, 0, -forward.x);
            Vector3 targetDirection = forward * vertical + right * horizontal;

            if (behaviourManager.IsMoving() && targetDirection != Vector3.zero)
            {
                // Get current rotation and smoothly transition to the target rotation.
                Quaternion currentRotation = rb.rotation;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // Preserve current Y rotation for smoothness
                Quaternion smoothRotation = Quaternion.Slerp(
                    currentRotation,
                    Quaternion.Euler(0, targetRotation.eulerAngles.y, 0), // Lock to Y-axis rotation only
                    Time.deltaTime * 5f // Adjust smoothing speed
                );

                rb.MoveRotation(smoothRotation);
                behaviourManager.SetLastDirection(targetDirection);
            }

            // If idle, keep last known good rotation.
            if (!(Mathf.Abs(horizontal) > 0.2f || Mathf.Abs(vertical) > 0.2f))
            {
                behaviourManager.Repositioning();
                col.direction = 1;
            }
            else
            {
                col.direction = 2;
            }

            return targetDirection;
        }
    }
    Vector3 lastReflectedDirection = Vector3.zero;
    void OnCollisionEnter(Collision collision)
    {
        if (!fly) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        Vector3 collisionNormal = collision.contacts[0].normal;

        if (impactSpeed > collisionDamageThresholdSpeed)
        {
            GetComponent<Animator>().Play("Flying Collision");
            collisionCooldownTimer = collisionImpactCooldown;

            if (((1 << collision.gameObject.layer) & destructibleLayers) != 0)
            {

                if (collision.gameObject.GetComponentInParent<DestructibleObject>() != null)
                {
                    collision.gameObject.GetComponentInParent<DestructibleObject>().DestroyObject();
                    //desiredVelocity *= slowDownFactor;
                }               

            }
            else
            {
                HandleBounce(collision, impactSpeed, collisionNormal);
            }
        }
        else
        {
            GetComponent<Animator>().Play("Flying Collision");
            HandleBounce(collision, impactSpeed, collisionNormal);

        }
    }

    private void HandleBounce(Collision collision, float impactSpeed, Vector3 collisionNormal)
    {
        // Non-destructible object: Calculate the bounce direction.
        Vector3 currentVelocity = rb.linearVelocity.normalized;

        // Get the camera's forward direction.
        Vector3 cameraLookDirection = behaviourManager.playerCamera.forward.normalized;
        Vector3 reflectedCameraLookDirection = Vector3.Reflect(cameraLookDirection, collisionNormal).normalized;
        lastReflectedDirection = reflectedCameraLookDirection;

        // Reflect the current velocity using the collision normal.
        Vector3 reflectedDirection = Vector3.Reflect(currentVelocity, collisionNormal).normalized;
        // Blend the reflected direction with the camera look direction.
        Vector3 blendedDirection = Vector3.Lerp(reflectedDirection, reflectedCameraLookDirection, 1f).normalized;

        // Debug: Visualize directions.
        Debug.DrawRay(collision.contacts[0].point, collisionNormal * 2f, Color.red, 2f);       // Collision normal.
        Debug.DrawRay(collision.contacts[0].point, reflectedDirection * 2f, Color.green, 2f); // Reflected direction.
        Debug.DrawRay(collision.contacts[0].point, blendedDirection * 2f, Color.blue, 2f);   // Blended direction.

        // Apply separation force to prevent getting stuck.
        Vector3 separationForce = collisionNormal * 0.5f;
        rb.position += separationForce;

        // Set desired velocity for the bounce and clamp it.
        desiredVelocity = blendedDirection * Mathf.Clamp(impactSpeed * sprintFactor, 0f, flySpeed * sprintFactor);

        // Align rotation with the blended direction.
        rb.rotation = Quaternion.LookRotation(blendedDirection);

        // Start the bounce state.
        isBouncing = true;
        bounceTimer = bounceDuration;
    }
}
