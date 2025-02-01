using UnityEngine;

public class ThirdPersonOrbitCamAdvanced : MonoBehaviour
{
    public Transform player;
    public Vector3 pivotOffset = new Vector3(0.0f, 1.7f, 0.0f);
    public Vector3 camOffset = new Vector3(0.0f, 0.0f, -3.0f);
    public float smooth = 10f;
    public float horizontalAimingSpeed = 6f;
    public float verticalAimingSpeed = 6f;
    public float maxVerticalAngle = 30f;
    public float minVerticalAngle = -60f;
    public string XAxis = "Analog X";
    public string YAxis = "Analog Y";

    public float maxDistance = 5f;
    public float minDistance = 2f;

    public float zoomSpeed = 5f;

    private float angleH = 0;
    private float angleV = 0;
    private Transform cam;
    private Vector3 smoothPivotOffset;
    private Vector3 smoothCamOffset;
    private Vector3 targetPivotOffset;
    private Vector3 targetCamOffset;
    private float defaultFOV;
    private float targetFOV;
    private float targetMaxVerticalAngle;
    private bool isCustomOffset;

    private Vector3 originalPivotOffset;
    private Vector3 originalCamOffset; // Store original camOffset
    private float currentZoomDistance;

    public float GetH => angleH;

    void Awake()
    {
        cam = transform;

        originalPivotOffset = pivotOffset; // Store original pivotOffset
        originalCamOffset = camOffset;   // Store original camOffset

        defaultFOV = cam.GetComponent<Camera>().fieldOfView;

        ResetTargetOffsets();
        ResetFOV();
        ResetMaxVerticalAngle();


        float initialPlayerScale = player.localScale.magnitude;
        pivotOffset = originalPivotOffset * initialPlayerScale;
        currentZoomDistance = camOffset.magnitude;
        camOffset = -originalCamOffset.normalized * currentZoomDistance;

        cam.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        cam.rotation = Quaternion.identity;

        smoothPivotOffset = pivotOffset; // Initialize smoothPivotOffset
        smoothCamOffset = camOffset;   // Initialize smoothCamOffset

        angleH = player.eulerAngles.y;

        if (camOffset.y > 0)
            Debug.LogWarning("Vertical Cam Offset (Y) will be ignored during collisions!\n" +
                "It is recommended to set all vertical offset in Pivot Offset.");
    }

    void Update()
    {
        // ✅ If aiming, don't update normal camera movement
        if (isCustomOffset) return;

        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalAimingSpeed;
        angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalAimingSpeed;
        angleH += Mathf.Clamp(Input.GetAxis(XAxis), -1, 1) * 60 * horizontalAimingSpeed * Time.deltaTime;
        angleV += Mathf.Clamp(Input.GetAxis(YAxis), -1, 1) * 60 * verticalAimingSpeed * Time.deltaTime;

        angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);

        Quaternion camYRotation = Quaternion.Euler(0, angleH, 0);
        Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
        cam.rotation = aimRotation;

        cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp(cam.GetComponent<Camera>().fieldOfView, targetFOV, Time.deltaTime);

        float playerScale = player.localScale.magnitude;
        pivotOffset = originalPivotOffset * playerScale;

        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        currentZoomDistance -= zoomInput * zoomSpeed;

        float scaledMaxDistance = maxDistance * playerScale;
        float scaledMinDistance = minDistance * playerScale;
        currentZoomDistance = Mathf.Clamp(currentZoomDistance, scaledMinDistance, scaledMaxDistance);

        camOffset = -originalCamOffset.normalized * currentZoomDistance;

        Vector3 baseTempPosition = player.position + camYRotation * pivotOffset;
        Vector3 noCollisionOffset = camOffset;
        while (noCollisionOffset.magnitude >= 0.2f)
        {
            if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset))
                break;
            noCollisionOffset -= noCollisionOffset.normalized * 0.2f;
        }
        if (noCollisionOffset.magnitude < 0.2f)
            noCollisionOffset = Vector3.zero;

        bool customOffsetCollision = isCustomOffset && noCollisionOffset.sqrMagnitude < targetCamOffset.sqrMagnitude;

        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, customOffsetCollision ? pivotOffset : pivotOffset, smooth * Time.deltaTime);
        smoothCamOffset = Vector3.Lerp(smoothCamOffset, customOffsetCollision ? Vector3.zero : noCollisionOffset, smooth * Time.deltaTime);

        cam.position = player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
    }

    public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;

        // ✅ Ensure Camera Moves Instantly to the Target Offsets
        smoothPivotOffset = targetPivotOffset;
        smoothCamOffset = targetCamOffset;

        isCustomOffset = true;
    }


    public void ResetTargetOffsets()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
        isCustomOffset = false;
    }

    public void ResetYCamOffset()
    {
        targetCamOffset.y = camOffset.y;
    }

    public void SetYCamOffset(float y)
    {
        targetCamOffset.y = y;
    }

    public void SetXCamOffset(float x)
    {
        targetCamOffset.x = x;
    }

    public void SetFOV(float customFOV)
    {
        this.targetFOV = customFOV;
    }

    public void ResetFOV()
    {
        this.targetFOV = defaultFOV;
    }

    public void SetMaxVerticalAngle(float angle)
    {
        this.targetMaxVerticalAngle = angle;
    }

    public void ResetMaxVerticalAngle()
    {
        this.targetMaxVerticalAngle = maxVerticalAngle;
    }

    bool DoubleViewingPosCheck(Vector3 checkPos)
    {
        return ViewingPosCheck(checkPos) && ReverseViewingPosCheck(checkPos);
    }

    bool ViewingPosCheck(Vector3 checkPos)
    {
        Vector3 target = player.position + pivotOffset;
        Vector3 direction = target - checkPos;
        if (Physics.SphereCast(checkPos, 0.2f, direction, out RaycastHit hit, direction.magnitude))
        {
            if (hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    bool ReverseViewingPosCheck(Vector3 checkPos)
    {
        Vector3 origin = player.position + pivotOffset;
        Vector3 direction = checkPos - origin;
        if (Physics.SphereCast(origin, 0.2f, direction, out RaycastHit hit, direction.magnitude))
        {
            if (hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
    }

    public void ApplyVerticalRotation(float mouseY)
    {
        angleV -= mouseY; // Invert for natural camera feel
        angleV = Mathf.Clamp(angleV, minVerticalAngle, maxVerticalAngle); // Prevent over-rotation
    }

}