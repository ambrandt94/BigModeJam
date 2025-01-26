using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChainLink.UI
{
    public class UIMouseFollower : MonoBehaviour
    {
        public bool Active;
        public enum MoveMode
        {
            MoveTowards,
            Lerp
        }

        [SerializeField]
        MoveMode Mode;
        [SerializeField]
        private Vector2 Offset;
        [SerializeField]
        private bool ClampX;
        [SerializeField]
        private bool ClampY;
        [SerializeField]
        float SmoothSpeed;
        [SerializeField]
        RectTransform MouseOverride;

        const float SpeedConst_MoveTowards = 300;
        const float SpeedConst_Lerp = 2.5f;

        public void SetFollowOffset(Vector2 offset)
        {
            Offset = offset;
        }

        public void SetPivot(Vector2 pivot)
        {
            RectTransform r = transform as RectTransform;
            if (r != null)
                r.pivot = pivot;
        }

        private void PerformFollow()
        {
            Vector2 currentPositon = MouseOverride != null ? MouseOverride.position : Input.mousePosition;
            Vector2 newPosition = currentPositon + Offset;
            newPosition += Offset;
            if (SmoothSpeed > 0) {
                if (Mode == MoveMode.MoveTowards)
                    newPosition = Vector2.MoveTowards(transform.position, newPosition, Time.deltaTime * SmoothSpeed * SpeedConst_MoveTowards);
                else
                    newPosition = Vector2.Lerp(transform.position, newPosition, Time.deltaTime * SmoothSpeed * SpeedConst_Lerp);
            }

            if (ClampX)
                newPosition.x = transform.position.x;
            if (ClampY)
                newPosition.y = transform.position.y;


            transform.position = newPosition;
        }

        public static void AlignObjectToMouse(RectTransform t)
        {
            Vector2 currentPositon = Input.mousePosition;

            t.position = currentPositon;
        }

        public static void AlignObjectToMouse(RectTransform t, Vector2 offset)
        {
            Vector2 currentPositon = (Vector2)Input.mousePosition + offset;

            t.position = currentPositon;
        }

        private void Update()
        {
            if (Active)
                PerformFollow();
        }
    }
}