using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChainLink.UI
{
    public class QuadrantPivot : MonoBehaviour
    {
        [SerializeField]
        private bool alignOnUpdate;

        [SerializeField, ReadOnly]
        private int quadrant;
        [SerializeField, ReadOnly]
        private Vector2 mousePosition;
        [SerializeField, ReadOnly]
        private float xMin;
        [SerializeField, ReadOnly]
        private float xMax;
        [SerializeField, ReadOnly]
        private float yMin;
        [SerializeField, ReadOnly]
        private float yMax;

        private Camera _camera;

        public void Align()
        {
            mousePosition = _camera.ScreenToViewportPoint(Input.mousePosition);
            quadrant = GetCurrentQuadrant();
            RectTransform rT = transform as RectTransform;
            if (quadrant == 1) {
                rT.pivot = new Vector2(1, 1);
            } else if (quadrant == 2) {
                rT.pivot = new Vector2(0, 1);
            } else if (quadrant == 3) {
                rT.pivot = new Vector2(0, 0);
            } else {
                rT.pivot = new Vector2(1, 0);
            }
        }

        private int GetCurrentQuadrant()
        {
            if (_camera != null) {
                bool right = mousePosition.x >= .5f;
                bool up = mousePosition.y >= .5f;
                if (right && up)
                    return 1;
                else if (!right && up)
                    return 2;
                else if (!right && !up)
                    return 3;
                else return 4;
            }
            return 2;
        }

        private void Update()
        {
            if (!alignOnUpdate)
                return;
            if (_camera != null) {
                xMin = _camera.pixelRect.xMin;
                xMax = _camera.pixelRect.xMax;
                yMin = _camera.pixelRect.yMin;
                yMax = _camera.pixelRect.yMax;
            }
            Align();
        }

        private void Awake()
        {
            _camera = Camera.main;
        }
    }
}