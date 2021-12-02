using System;
using UnityEngine;
#if USING_UNITY_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Didimo
{
    public class DragOrbit : MonoBehaviour
    {
        public CameraConfig config;

        public class DragOrbitState : ICloneable
        {
            public float Yaw;
            public float Pitch;
            public float Zoom;

            // The actual camera values
            public float   harmonicYaw;
            public float   harmonicPitch;
            public float   harmonicZoom;
            public Vector2 prevPosition;

            public object Clone() => MemberwiseClone();
        }

        // The target input values
        private DragOrbitState state = new DragOrbitState();

        // The harmonics
        private HarmonicFloat yawHarmonic;
        private HarmonicFloat pitchHarmonic;
        private HarmonicFloat zoomHarmonic;

        private PinchZoom pinchZoom;

        public Vector3 HarmonicDir => EquirectangularProjection(HarmonicRot);

        protected float Distance => config.distance + state.harmonicZoom;

        private Vector2 HarmonicRot => new Vector2(state.harmonicYaw, state.harmonicPitch);
        private Vector2 MousePosition => Input.mousePosition;

        protected void Awake()
        {
            pinchZoom = new PinchZoom();

            // Create harmonics
            yawHarmonic = new HarmonicFloat(() =>
                state.harmonicYaw, (v) => state.harmonicYaw = v,
                () => 0.8f, () => 10);
            pitchHarmonic = new HarmonicFloat(() =>
                state.harmonicPitch, (v) => state.harmonicPitch = v,
                () => 0.8f, () => 10);
            zoomHarmonic = new HarmonicFloat(() => state.harmonicZoom,
                (v) => state.harmonicZoom = v, () => 0.8f, () => 10);

            // Set default values
            state.harmonicYaw = 0.5f;
            state.harmonicPitch = config.defaultPitch;
            state.harmonicZoom = 0;

            state.Yaw = state.harmonicYaw;
            state.Pitch = state.harmonicPitch;
            state.Zoom = state.harmonicZoom;

            yawHarmonic.SetValue(state.harmonicYaw);
            pitchHarmonic.SetValue(state.harmonicPitch);
            zoomHarmonic.SetValue(state.harmonicZoom);
        }

        protected void Update()
        {
            HandleInput();
            CalculateHarmonics();
            HandleWrapping();
            HandleTransform();
            HandlePostProcessing();
        }

        public DragOrbitState GetStateClone() => state.Clone() as DragOrbitState;

        public void SetState(DragOrbitState st) { state = st; }

        public void ResetView(bool instant)
        {
            // Set default values
            state.Yaw = 0.5f;
            state.Pitch = config.defaultPitch;
            state.Zoom = 0;

            if (instant)
            {
                yawHarmonic.SetValue(state.Yaw);
                pitchHarmonic.SetValue(state.Pitch);
                zoomHarmonic.SetValue(state.Zoom);
            }

            HandleTransform();
        }

        private void HandlePostProcessing()
        {
#if USING_UNITY_URP
            if (SceneContext.Volume == null)
            {
                return;
            }

            if (SceneContext.Volume.profile
                .TryGet(out DepthOfField depthOfField) == false)
            {
                return;
            }

            depthOfField.focusDistance.value
                = Distance + config.depthOfFieldDistanceBias;

            depthOfField.focalLength.value
                = Distance + config.focalLength;

            depthOfField.gaussianStart.value
                = Distance + config.depthOfFieldDistanceBias;

            depthOfField.gaussianEnd.value
                = Distance + config.depthOfFieldDistanceBias
                + config.focalLength;
#endif
        }

        private void HandleInput()
        {
            Vector2 delta = Vector2.zero;
            float zoomDelta = 0;

            if (Input.touchSupported)
            {
                pinchZoom.Update();

                // Drag to rotate
                if (Input.touchCount == 1 || Input.touchCount > 2)
                {
                    delta = GetAverageTouch().delta;
                }
                else if (Input.touchCount == 2)
                {
                    zoomDelta = pinchZoom.GetZoomDelta();
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    delta = state.prevPosition - MousePosition;
                }

                zoomDelta = -Input.mouseScrollDelta.y;
            }

            state.Yaw += delta.x * config.sensitivity.x * Time.deltaTime;
            state.Pitch += delta.y * config.sensitivity.y * Time.deltaTime;
            state.Zoom += zoomDelta * config.zoomSensitivity * Time.deltaTime;

            state.Yaw = Mathf.Clamp(state.Yaw,
                config.yawLimit.x, config.yawLimit.y);

            state.Pitch = Mathf.Clamp(state.Pitch,
                config.pitchLimit.x, config.pitchLimit.y);

            state.Zoom = Mathf.Clamp(state.Zoom,
                config.zoomLimit.x, config.zoomLimit.y);

            yawHarmonic.SetTarget(state.Yaw);
            pitchHarmonic.SetTarget(state.Pitch);
            zoomHarmonic.SetTarget(state.Zoom);

            state.prevPosition = MousePosition;
        }

        private (Vector2 position, Vector2 delta) GetAverageTouch()
        {
            int touchCount = Input.touchCount;

            Vector2 avgPosition = Vector2.zero;
            Vector2 avgDelta = Vector2.zero;

            if (touchCount == 0)
                return (avgPosition, avgDelta);

            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                avgPosition += touch.position;
                avgDelta += touch.deltaPosition;
            }

            return (avgPosition / touchCount, avgDelta / touchCount);
        }

        private void CalculateHarmonics()
        {
            yawHarmonic.Update();
            pitchHarmonic.Update();
            zoomHarmonic.Update();
        }

        private void HandleWrapping()
        {
            if (yawHarmonic.IsRunning || pitchHarmonic.IsRunning)
            {
                return;
            }

            state.Yaw = state.Yaw % 360;
            if (state.Yaw < 0)
            {
                state.Yaw += 360;
            }
            state.harmonicYaw = state.Yaw;
        }

        private void HandleTransform()
        {
            Vector3 focus = config.focus;
            transform.position = focus + HarmonicDir * -Distance;
            transform.LookAt(focus, Vector3.up);
        }

        private static Vector3 EquirectangularProjection(Vector2 inputVector)
        {
            inputVector.y = 1 - inputVector.y;

            float xx = Mathf.Lerp(0, 1,
                (Mathf.Sin(inputVector.x * (2 * Mathf.PI)) * -1 + 1) / 2);
            float xy = Mathf.Cos((inputVector.y + 0.5f) / 2 * (2 * Mathf.PI)) + 1;
            float x = Mathf.Lerp(xx, 0.5f, xy);

            float yx = Mathf.Lerp(0, 1,
                (Mathf.Sin((inputVector.x + 0.25f) *
                (2 * Mathf.PI)) * -1 + 1) / 2);
            float yy = Mathf.Cos((inputVector.y + 0.5f) / 2 * (2 * Mathf.PI)) + 1;
            float y = Mathf.Lerp(yx, 0.5f, yy);

            float z = (Mathf.Cos(inputVector.y / 2 * (2 * Mathf.PI)) + 1) / 2;

            Vector3 xyz = new Vector3(-2 * x, 2 * z, 2 * y) + new Vector3(1, -1, -1);
            xyz.z = -xyz.z;

            return xyz.normalized;
        }
    }
}