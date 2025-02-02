using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BigModeJam
{
    public class CharacterPather : MonoBehaviour
    {
        [SerializeField]
        private bool pathing;
        [SerializeField]
        private Transform target;

        Vector2 testValue;

        private CharacterAnimator animator;
        private IAstarAI ai;

        public void Initialize()
        {
            animator = GetComponentInChildren<CharacterAnimator>();
        }

        public void SearchForRandomTravelPoint()
        {
            GoToTarget(NPCManager.Instance.GetRandomTravelPoint());
        }

        [Button("Go to Target")]
        private void GoToTarget(Transform t)
        {
            if (t == null)
                return;
            target = t;
            pathing = true;

            ai.destination = target.transform.position;
            ai.isStopped = false;
            ai.canSearch = true;
            //ai.SearchPath();
        }

        private void OnSearchPath()
        {
            if (target == null)
                return;
            ai.destination = target.transform.position;
        }

        private void Update()
        {
            if (pathing) {

                // Normalize the entity's forward direction on the XZ plane
                Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

                // Normalize the entity's velocity on the XZ plane
                Vector3 velocity = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

                // Compute the forward movement component
                float forwardAmount = Vector3.Dot(forward, velocity);

                // Compute the right movement component
                Vector3 right = new Vector3(forward.z, 0, -forward.x); // Perpendicular to forward
                float rightAmount = Vector3.Dot(right, velocity);

                // Return as a normalized Vector2
                Vector2 movement = new Vector2(rightAmount, forwardAmount).normalized;
                animator.SetXYInput(movement.x, movement.y);
                if (ai.reachedDestination) {
                    animator.IsWalking = false;
                    ai.isStopped = true;
                    Debug.Log($"{gameObject.name} has reached destination");
                    pathing = false;
                    GoToTarget(NPCManager.Instance.GetRandomTravelPoint());
                } else if (ai.reachedEndOfPath) {
                    animator.IsWalking = false;
                    ai.isStopped = true;
                    Debug.Log($"{gameObject.name} has reached end of path");
                    pathing = false;
                    GoToTarget(NPCManager.Instance.GetRandomTravelPoint());
                }
            }
        }

        private void Awake()
        {
            ai = GetComponent<IAstarAI>();
            ai.onSearchPath += OnSearchPath;
            //OnSearchPath();
        }
    }
}