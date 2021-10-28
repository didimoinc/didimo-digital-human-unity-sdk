using UnityEngine;

namespace Didimo
{
    public class PinchZoom
    {
        private Vector2 touch0Start;
        private Vector2 touch1Start;

        public void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch t0 = Input.GetTouch(0);
                if (t0.phase == TouchPhase.Began)
                {
                    touch0Start = t0.position;
                }
            }

            if (Input.touchCount > 1)
            {
                Touch t1 = Input.GetTouch(1);
                if (t1.phase == TouchPhase.Began)
                {
                    touch1Start = t1.position;
                }
            }
        }

        public float GetZoomDelta()
        {
            if (Input.touchCount != 2) return 0;

            float startDistance = (touch1Start - touch0Start).magnitude;
            float currentDistance = (Input.GetTouch(1).position - Input.GetTouch(0).position).magnitude;

            return currentDistance - startDistance;
        }
    }
}