using UnityEngine;

namespace ChainLink.Core
{
    public class SimpleRotate : MonoBehaviour
    {
        [SerializeField]
        private Vector3 vector;
        [SerializeField]
        private float speed;

        void Update()
        {
            if (speed > 0)
                transform.Rotate(vector * speed * Time.deltaTime);
        }
    }
}