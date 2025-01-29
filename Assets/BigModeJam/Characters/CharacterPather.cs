using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BigModeJam
{
    public class CharacterPather : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        private IAstarAI ai;

        [Button("Go to Target")]
        private void GoToTarget(Transform t)
        {
            target = t;
            ai.destination = target.transform.position;
            //ai.isStopped = false;
            //ai.canSearch = true;
            //ai.SearchPath();
        }

        private void OnSearchPath()
        {
            Debug.Log("Search");
            if (target == null)
                return;
            ai.destination = target.transform.position;
        }

        private void Update()
        {
            if (ai.reachedDestination) {
                //ai.isStopped = true;
                //ai.canSearch = false;
                Debug.Log($"{gameObject.name} has reached destination");
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