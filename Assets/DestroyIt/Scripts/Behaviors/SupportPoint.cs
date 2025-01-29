using UnityEngine;

namespace DestroyIt
{
    /// <summary>
    /// This script manages a single support point (a Trigger Collider) on an object. 
    /// On startup, if the support point is connected to another rigidbody, it will create a custom joint connecting the two.
    /// </summary>
    public class SupportPoint : MonoBehaviour
    {
        public int breakForce = 750;
        public int breakTorque = 750;

        private bool _canSupport = true;

        void Start()
        {
            _canSupport = CanSupport();

            if (_canSupport) {
                Ray ray = new Ray(transform.position - (transform.forward * 0.025f), transform.forward); // need to start the ray behind the transform a little
                //Debug.DrawRay(this.transform.position, this.transform.forward * .15f, Color.red, 10f);
                RaycastHit[] hitInfo = Physics.RaycastAll(ray, 0.075f);

                if (hitInfo.Length < 1)
                    Debug.LogError($"No hits for support: {gameObject.name}");
                for (int i = 0; i < hitInfo.Length; i++) {
                    // ignore colliders without rigidbodies to attach a joint to.
                    if (hitInfo[i].collider.attachedRigidbody == null) continue;

                    // ignore other trigger colliders - we only want to attach to the parent object.
                    if (hitInfo[i].collider.isTrigger) continue;

                    // Get the forward angle, as it relates to the parent
                    Vector3 angleAxis = transform.parent.transform.InverseTransformDirection(transform.TransformDirection(Vector3.forward));

                    // Add a stiff joint for support.
                    transform.parent.GetComponent<Collider>().attachedRigidbody.gameObject.AddStiffJoint(hitInfo[i].collider.attachedRigidbody,
                        transform.localPosition, angleAxis, breakForce, breakTorque);
                    break;
                }
            } else {
                Debug.LogError("Couldn't support", transform.parent.gameObject);
            }

            Destroy(gameObject);
        }

        private bool CanSupport()
        {
            if (transform.parent == null) {
                Debug.Log("[" + name + "] has no parent. Support points are designed to be children of objects that have attached colliders.");
                return false;
            } else if (transform.parent.GetComponent<Collider>() == null || !transform.parent.GetComponent<Collider>().enabled) {
                Debug.Log("[" + transform.parent.name + "] has a support point but no enabled collider. Support points only work on objects with colliders.");
                return false;
            } else if (transform.parent.GetComponent<Collider>().attachedRigidbody == null) {
                Debug.Log("[" + transform.parent.name + "] has a support point but no attached rigidbody. Support points only work on objects with rigidbodies.");
                return false;
            }
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = CanSupport()? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(transform.position - (transform.forward * 0.025f), .01f);
            Gizmos.DrawRay(transform.position - (transform.forward * 0.025f), transform.forward * 0.075f);
        }
    }
}