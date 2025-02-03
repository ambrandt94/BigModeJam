using DestroyIt;
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

    public float burstSpeedMultiplier = 1.5f; // Multiplier for burst speed
    public float burstDuration = 0.5f;
    public GameObject burstEffect; // Assign your burst effect GameObject in the Inspector

    private bool isBursting;
    private float burstTimer;

    public LayerMask groundLayers; // Assign ground layers in the Inspector
    public SpellCaster spellCaster; // Assign the spellcaster component in the Inspector
    public BaseSpell groundCollisionSpell;


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
            ToggleFly();
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

    public void ToggleFly()
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

    private bool burstInput;

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
        burstInput = Input.GetKeyDown(KeyCode.LeftShift);
        HandleBurst(); // Call burst handling method
        //rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, Time.fixedDeltaTime * velocityTransitionSpeed);
    }

    void FlyManagement(float horizontal, float vertical)
    {
        Vector3 direction = Rotating(horizontal, vertical);
        float speed = flySpeed;

        if (behaviourManager.IsSprinting())
        {
            if (!isBursting && burstInput)
            {
                StartBurst();
            }

            if (isBursting)
            {
                speed *= burstSpeedMultiplier;
            }
        }

        desiredVelocity = direction * speed;

        // Existing code for particle effect
        if (rb == null)
            return;
        if (rb.linearVelocity.magnitude >= collisionDamageThresholdSpeed)
        {
            speedEffectParticleSystem.Stop();
            speedEffectParticleSystem.Play();
        }
        else
        {
            speedEffectParticleSystem.Stop();
        }
    }

    private void HandleBurst()
    {
        if (isBursting)
        {
            burstTimer -= Time.deltaTime;

            if (burstTimer <= 0)
            {
                EndBurst();
            }
        }
    }

    private void StartBurst()
    {
        isBursting = true;
        burstTimer = burstDuration;       
        burstEffect.GetComponent<ParticleSystem>().Stop();
        burstEffect.GetComponent<ParticleSystem>().Play(); // Assuming you have a Particle System
    }

    private void EndBurst()
    {
        isBursting = false;
        burstEffect.GetComponent<ParticleSystem>().Stop();       
    }

    Vector3 Rotating(float horizontal, float vertical)
    {
        if (isBouncing)
        {
            // Maintain velocity direction for rotation during bounce
            Vector3 bounceDirection = lastReflectedDirection;
            Quaternion bounceRotation = Quaternion.LookRotation(bounceDirection);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, bounceRotation, behaviourManager.turnSmoothing);
            rb.MoveRotation(newRotation);
            behaviourManager.SetLastDirection(bounceDirection);
            behaviourManager.Repositioning();
            return bounceDirection;
        }
        else
        {
            // Get full camera movement directions
            Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward); // Forward in full 3D space
            Vector3 right = behaviourManager.playerCamera.TransformDirection(Vector3.right); // Strafing direction
            Vector3 up = behaviourManager.playerCamera.TransformDirection(Vector3.up); // Vertical movement

            // Allow movement in full 3D space using WASD + Space/Ctrl
            float flyVertical = Input.GetAxis("Jump") - Input.GetAxis("Crouch"); // Space = 1, Ctrl = -1, Neutral = 0
            Vector3 targetDirection = (forward * vertical) + (right * horizontal) + (up * flyVertical);

            if (targetDirection != Vector3.zero)
            {
                targetDirection.Normalize(); // Normalize to prevent speed boosts in diagonal movement

                // Set target rotation to always align with movement direction
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // Smoothly rotate while preserving only the Y-axis rotation
                Quaternion smoothRotation = Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    Time.deltaTime * 10f // Adjust for smoothness
                );

                rb.MoveRotation(smoothRotation);
                behaviourManager.SetLastDirection(targetDirection);
            }

            // If idle, keep last rotation
            if (!(Mathf.Abs(horizontal) > 0.2f || Mathf.Abs(vertical) > 0.2f || Mathf.Abs(flyVertical) > 0.2f))
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

        // 1. Ground Collision and Spell Cast: (This remains separate)
        if (((1 << collision.gameObject.layer) & groundLayers) != 0 && impactSpeed > bounceThresholdSpeed)
        {
            if (spellCaster != null)
            {
                spellCaster.Cast(groundCollisionSpell, transform.position, transform.forward.normalized);
                ToggleFly();
                return;
            }
            else
            {
                Debug.LogWarning("Spellcaster not assigned in FlyBehaviour!");
            }
            return; // Early exit to prevent ground collision from also triggering a bounce.
        }

        // 2. Bounce Logic (Simplified and Corrected):
        if (((1 << collision.gameObject.layer) & destructibleLayers) != 0)
        {
            // Destructible object: Apply damage (no bounce).
            if (collision.gameObject.GetComponentInParent<Destructible>() != null)
            {
                collision.gameObject.GetComponentInParent<Destructible>().ApplyDamage(50);
            }

            if (collision.gameObject.GetComponentInParent<Rigidbody>() != null &&
                 collision.gameObject.GetComponentInParent<Rigidbody>().mass > rb.mass &&
                 impactSpeed > bounceThresholdSpeed) // Add speed check here
            {
                // Non-destructible object with sufficient mass: Bounce.
                HandleBounce(collision, impactSpeed, collisionNormal);
            }
        }
        else if (collision.gameObject.GetComponentInParent<Rigidbody>() != null &&
                 collision.gameObject.GetComponentInParent<Rigidbody>().mass > rb.mass &&
                 impactSpeed > bounceThresholdSpeed) // Add speed check here
        {
            // Non-destructible object with sufficient mass: Bounce.
            HandleBounce(collision, impactSpeed, collisionNormal);
        }
        // Optionally, you could add an 'else' block here to handle collisions with
        // objects that don't have rigidbodies, or that have a mass less than
        // the player's mass, if you needed to do something specific in those cases.
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
