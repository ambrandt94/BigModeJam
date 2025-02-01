using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
    public float HorizontalInput => Input.GetAxis("Horizontal");
    public float VerticalInput => Input.GetAxis("Vertical");
    public bool SprintInput => Input.GetKey(KeyCode.LeftShift);

    [SerializeField]
    private bool canMove = true;
    [SerializeField]
    private float baseSpeed = 1;
    [SerializeField]
    private float sprintModifier = 1;

    private Camera playerCamera;
    private Rigidbody rigidbody;
    private CharacterAnimator characterAnimator;

    private void Update()
    {
        characterAnimator.SetXYInput(HorizontalInput, VerticalInput);
        ManageRotation();

        characterAnimator.SetBool("Sprinting", SprintInput);
        float speed = SprintInput? baseSpeed * sprintModifier : baseSpeed;
        Vector3 newVelocity = new Vector3(HorizontalInput, 0, VerticalInput).normalized;
        newVelocity = transform.TransformDirection(newVelocity) * speed * 100 * Time.fixedDeltaTime;
        Vector3 force = transform.TransformDirection(newVelocity) * speed - new Vector3(rigidbody.linearVelocity.x, 0, rigidbody.linearVelocity.z);
        rigidbody.AddForce(force, ForceMode.VelocityChange);
        //newVelocity.y = rigidbody.linearVelocity.y;
        //rigidbody.linearVelocity = newVelocity;
    }

    private void ManageRotation()
    {
        Vector3 forward = playerCamera.transform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        transform.rotation = Quaternion.LookRotation(forward);
    }

    private void Awake()
    {
        if (characterAnimator == null) {
            characterAnimator = GetComponent<CharacterAnimator>();
        }
        playerCamera = GetComponentInChildren<Camera>();
        rigidbody = GetComponentInChildren<Rigidbody>();
    }
}
