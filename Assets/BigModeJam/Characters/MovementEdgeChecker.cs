using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class MovementEdgeChecker : MonoBehaviour
{
    public bool CanGrab => !MaxCollisionCheck && MinCollisionCheck;
    public bool MaxCollisionCheck => Physics.CheckSphere(MaxHeightCheckPosition, checkRadius, grabbableLayer);
    public bool MinCollisionCheck => Physics.CheckSphere(MinHeightCheckPosition, checkRadius, grabbableLayer);

    public Vector3 MinHeightCheckPosition => (transform.position - Vector3.up * minHeight) + transform.forward * maxDistance;
    public Vector3 MaxHeightCheckPosition => (transform.position + Vector3.up * maxHeight) + transform.forward * maxDistance;

    public Vector3 AngleCheckPosition => new Vector3(root.position.x, transform.position.y, root.position.z);
    public Vector3 AngleCheckForward => transform.forward;

    public Action OnGrabbedEdge;
    public Action OnGrabEnd;

    [SerializeField]
    public LayerMask grabbableLayer;
    [SerializeField]
    private Transform root;
    [SerializeField]
    private Transform testGrabPoint;
    [SerializeField]
    private float checkRadius;
    [SerializeField]
    private float maxDistance;
    [SerializeField]
    private float maxHeight;
    [SerializeField]
    private float minHeight;
    [SerializeField, ReadOnly]
    private bool checkingForEdge;
    [SerializeField, ReadOnly]
    private Vector3 offsetFromRoot;
    [SerializeField, ReadOnly]
    private bool onEdge;
    [SerializeField, ReadOnly]
    private float angleDif;

    private Coroutine cooldownRoutine;

    public void ToggleCheckForEdge(bool checking)
    {
        checkingForEdge = checking;
    }

    [Button("Apply Hang")]
    public void ApplyHangingTransform()
    {
        onEdge = true;
        Debug.Log("Grab Edge");
        //transform.position = testGrabPoint.transform.position;
        OnGrabbedEdge?.Invoke();
        root.position = transform.TransformPoint(-offsetFromRoot);
        //RaycastHit hitInfo = new RaycastHit();
        //if (Physics.Raycast(new Ray(AngleCheckPosition, AngleCheckForward), out hitInfo, 3, grabbableLayer)) {
        //    Quaternion rot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        //    root.transform.rotation = rot;
        //} else {
        //    Debug.Log("No Hit");
        //}
        transform.localPosition = offsetFromRoot;
        checkingForEdge = false;
    }

    public void OnLeaveEdge()
    {
        if (cooldownRoutine != null) {
            StopCoroutine(cooldownRoutine);
            cooldownRoutine = null;
        }
        cooldownRoutine = StartCoroutine(LeaveEdgeRoutine());
    }

    private IEnumerator LeaveEdgeRoutine()
    {
        onEdge = false;
        yield return new WaitForSeconds(.5f);
        checkingForEdge = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(AngleCheckPosition, AngleCheckPosition + AngleCheckForward);
        bool hits = MaxCollisionCheck;
        Gizmos.color = hits ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * maxHeight);
        Gizmos.DrawLine(transform.position + Vector3.up * maxHeight, MaxHeightCheckPosition);
        Gizmos.DrawWireSphere(MaxHeightCheckPosition, checkRadius);
        hits = MinCollisionCheck;
        Gizmos.color = hits ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position - Vector3.up * minHeight);
        Gizmos.DrawLine(transform.position - Vector3.up * minHeight, MinHeightCheckPosition);
        Gizmos.DrawWireSphere(MinHeightCheckPosition, checkRadius);
    }

    private void Update()
    {
        if (onEdge) {
            
        }
    }

    private void FixedUpdate()
    {
        if (checkingForEdge) {
            if (CanGrab) {
                ApplyHangingTransform();
            }
        }
    }

    private void Awake()
    {
        offsetFromRoot = transform.localPosition;
    }
}
