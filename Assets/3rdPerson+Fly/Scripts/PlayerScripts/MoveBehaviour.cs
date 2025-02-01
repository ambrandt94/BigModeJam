using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct SpeedPhase
{
	public int phaseNum;
	public float walkSpeed;
	public float runSpeed;
	public float sprintSpeed;
	public float flySpeed;
}


// MoveBehaviour inherits from GenericBehaviour. This class corresponds to basic walk and run behaviour, it is the default behaviour.
public class MoveBehaviour : GenericBehaviour
{
	public float walkSpeed = 0.15f;                 // Default walk speed.
	public float runSpeed = 1.0f;                   // Default run speed.
	public float sprintSpeed = 2.0f;                // Default sprint speed.
	public float speedDampTime = 0.1f;              // Default damp time to change the animations based on current speed.
	public string jumpButton = "Jump";              // Default jump button.
	public float jumpHeight = 1.5f;                 // Default jump height.
	public float jumpInertialForce = 10f;          // Default horizontal inertial force when jumping.

	private float speed, speedSeeker;               // Moving speed.
	private int jumpBool;                           // Animator variable related to jumping.
	private int groundedBool;                       // Animator variable related to whether or not the player is on ground.
	private bool jump;                              // Boolean to determine whether or not the player started a jump.
	private bool isColliding;                       // Boolean to determine if the player has collided with an obstacle.

	public float rotationSlerpSpeed = 10f;


	public int currentSpeedPhaseNum = 0;
	public List<SpeedPhase> speedPhases;

	public void IncreaseSpeedPhase()
	{
		if (currentSpeedPhaseNum >= 3)
			return;
        currentSpeedPhaseNum++;	
        UpdateSpeeds();
    }

	public void DecreaseSpeedPhase()
	{
        if (currentSpeedPhaseNum <= -3)
            return;

		currentSpeedPhaseNum--;       
        UpdateSpeeds();
    }

	public void UpdateSpeeds()
	{
        SpeedPhase currentSpeedPhase = speedPhases.FirstOrDefault(obj => obj.phaseNum == currentSpeedPhaseNum);
        this.walkSpeed = currentSpeedPhase.walkSpeed;
		this.runSpeed = currentSpeedPhase.runSpeed;
		this.sprintSpeed = currentSpeedPhase.sprintSpeed;
		if(GetComponent<FlyBehaviour>() != null)
		{
			GetComponent<FlyBehaviour>().flySpeed = currentSpeedPhase.flySpeed;

        }
	}

	// Start is always called after any Awake functions.
	void Start()
	{
		// Set up the references.
		jumpBool = Animator.StringToHash("Jump");
		groundedBool = Animator.StringToHash("Grounded");
		behaviourManager.GetAnim.SetBool(groundedBool, true);

		// Subscribe and register this behaviour as the default behaviour.
		behaviourManager.SubscribeBehaviour(this);
		behaviourManager.RegisterDefaultBehaviour(this.behaviourCode);
		speedSeeker = runSpeed;
	}

	// Update is used to set features regardless the active behaviour.
	void Update()
	{
		// Get jump input.
		if (!jump && Input.GetButtonDown(jumpButton) && behaviourManager.IsCurrentBehaviour(this.behaviourCode) && !behaviourManager.IsOverriding())
		{
			jump = true;
		}
	}

	// LocalFixedUpdate overrides the virtual function of the base class.
	public override void LocalFixedUpdate()
	{
		// Call the basic movement manager.
		MovementManagement(behaviourManager.GetH, behaviourManager.GetV);

		// Call the jump manager.
		JumpManagement();
	}

	// Execute the idle and walk/run jump movements.
	void JumpManagement()
	{
		// Start a new jump.
		if (jump && !behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.IsGrounded())
		{
			// Set jump related parameters.
			behaviourManager.LockTempBehaviour(this.behaviourCode);
			behaviourManager.GetAnim.SetBool(jumpBool, true);
			// Is a locomotion jump?
			if (behaviourManager.GetAnim.GetFloat(speedFloat) > 0.1)
			{
				// Temporarily change player friction to pass through obstacles.
				GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
				GetComponent<CapsuleCollider>().material.staticFriction = 0f;
				// Remove vertical velocity to avoid "super jumps" on slope ends.
				RemoveVerticalVelocity();
				// Set jump vertical impulse velocity.
				float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
				velocity = Mathf.Sqrt(velocity);
				behaviourManager.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
			}
		}
		// Is already jumping?
		else if (behaviourManager.GetAnim.GetBool(jumpBool))
		{
			// Keep forward movement while in the air.
			if (!behaviourManager.IsGrounded() && !isColliding && behaviourManager.GetTempLockStatus())
			{
				behaviourManager.GetRigidBody.AddForce(transform.forward * (jumpInertialForce * Physics.gravity.magnitude * sprintSpeed), ForceMode.Acceleration);
			}
			// Has landed?
			if ((behaviourManager.GetRigidBody.linearVelocity.y < 0) && behaviourManager.IsGrounded())
			{
				behaviourManager.GetAnim.SetBool(groundedBool, true);
				// Change back player friction to default.
				GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
				GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
				// Set jump related parameters.
				jump = false;
				behaviourManager.GetAnim.SetBool(jumpBool, false);
				behaviourManager.UnlockTempBehaviour(this.behaviourCode);
			}
		}
	}

	// Deal with the basic player movement
	void MovementManagement(float horizontal, float vertical)
	{
		// On ground, obey gravity.
		if (behaviourManager.IsGrounded())
			behaviourManager.GetRigidBody.useGravity = true;

		// Avoid takeoff when reached a slope end.
		else if (!behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.GetRigidBody.linearVelocity.y > 0)
		{
			RemoveVerticalVelocity();
		}

		// Call function that deals with player orientation.
		Rotating(horizontal, vertical);

		// Set proper speed.
		Vector2 dir = new Vector2(horizontal, vertical);
		speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
		// This is for PC only, gamepads control speed via analog stick.
		speedSeeker += Input.GetAxis("Mouse ScrollWheel");
		speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
		speed *= speedSeeker;
		if (behaviourManager.IsSprinting())
		{
			speed = sprintSpeed;
		}

		behaviourManager.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
	}

	// Remove vertical rigidbody velocity.
	private void RemoveVerticalVelocity()
	{
		Vector3 horizontalVelocity = behaviourManager.GetRigidBody.linearVelocity;
		horizontalVelocity.y = 0;
		behaviourManager.GetRigidBody.linearVelocity = horizontalVelocity;
	}

    // Rotate the player to match correct orientation, according to camera and key pressed.
    Vector3 Rotating(float horizontal, float vertical)
    {
        // Get camera's forward direction and remove vertical influence.
        Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);
        forward.y = 0; // Lock Y-axis movement to prevent tilting
        forward.Normalize();

        // Get right direction (perpendicular to forward).
        Vector3 right = behaviourManager.playerCamera.TransformDirection(Vector3.right);
        right.y = 0; // Lock Y-axis movement
        right.Normalize();

        // Calculate the movement direction based on both mouse and keyboard input.
        Vector3 targetDirection = (forward * vertical) + (right * horizontal);
        targetDirection.y = 0; // Ensure we don't apply vertical rotation
        targetDirection.Normalize(); // Normalize to prevent faster diagonal movement

        if (behaviourManager.IsMoving() && targetDirection != Vector3.zero)
        {
            // Get current rotation and smoothly transition to the target rotation.
            Quaternion currentRotation = behaviourManager.GetRigidBody.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            // Smooth rotation with Slerp, preserving only Y-axis rotation
            Quaternion smoothRotation = Quaternion.Slerp(
                currentRotation,
                Quaternion.Euler(0, targetRotation.eulerAngles.y, 0), // Lock to Y-axis rotation only
                Time.deltaTime * 10f // Adjust smoothing speed
            );

            behaviourManager.GetRigidBody.MoveRotation(smoothRotation);
            behaviourManager.SetLastDirection(targetDirection);
        }

        // If idle, keep last known good rotation.
        if (!(Mathf.Abs(horizontal) > 0.2f || Mathf.Abs(vertical) > 0.2f))
        {
            behaviourManager.Repositioning();
        }

        return targetDirection;
    }



    // Collision detection.
    private void OnCollisionStay(Collision collision)
	{
		isColliding = true;
		// Slide on vertical obstacles
		if (behaviourManager.IsCurrentBehaviour(this.GetBehaviourCode()) && collision.GetContact(0).normal.y <= 0.1f)
		{
			GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
			GetComponent<CapsuleCollider>().material.staticFriction = 0f;
		}
	}
	private void OnCollisionExit(Collision collision)
	{
		isColliding = false;
		GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
		GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
	}
}
