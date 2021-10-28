using System;
using DigitalSalmon;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Didimo
{
    public class DragOrbit : MonoBehaviour
    {
        public class DragOrbitState : ICloneable
        {
            public float inputYaw;
            public float inputPitch;
            public float inputZoom;

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

        protected float Distance => CameraConfig.Distance + state.harmonicZoom;

        private Vector2 HarmonicRot => new Vector2(state.harmonicYaw, state.harmonicPitch);
        private Vector2 MousePosition => new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        protected void Awake()
        {
            pinchZoom = new PinchZoom();

            // Create harmonics
            yawHarmonic = new HarmonicFloat(() => state.harmonicYaw, (v) => state.harmonicYaw = v, () => 0.8f, () => 10);
            pitchHarmonic = new HarmonicFloat(() => state.harmonicPitch, (v) => state.harmonicPitch = v, () => 0.8f, () => 10);
            zoomHarmonic = new HarmonicFloat(() => state.harmonicZoom, (v) => state.harmonicZoom = v, () => 0.8f, () => 10);

            // Set default values
            state.harmonicYaw = 0.5f;
            state.harmonicPitch = CameraConfig.DefaultPitch;
            state.harmonicZoom = 0;

            state.inputYaw = state.harmonicYaw;
            state.inputPitch = state.harmonicPitch;
            state.inputZoom = state.harmonicZoom;

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
            state.inputYaw = 0.5f;
            state.inputPitch = CameraConfig.DefaultPitch;
            state.inputZoom = 0;

            if (instant)
            {
                yawHarmonic.SetValue(state.inputYaw);
                pitchHarmonic.SetValue(state.inputPitch);
                zoomHarmonic.SetValue(state.inputZoom);
            }

            HandleTransform();
        }

        private void HandlePostProcessing()
        {
            if (SceneContext.Volume != null)
            {
                if (SceneContext.Volume.profile.TryGet(out DepthOfField depthOfField))
                {
                    depthOfField.focusDistance.value = Distance + CameraConfig.DepthOfFieldDistanceBias;
                    depthOfField.focalLength.value = Distance + CameraConfig.FocalLength;
                    depthOfField.gaussianStart.value = Distance + CameraConfig.DepthOfFieldDistanceBias;
                    depthOfField.gaussianEnd.value = Distance + CameraConfig.DepthOfFieldDistanceBias + CameraConfig.FocalLength;
                }
            }
        }

        private void HandleInput()
        {
            Vector2 delta = Vector2.zero;
            float zoomDelta = 0;

            if (Input.touchSupported)
            {
                pinchZoom.Update();

                if (Input.touchCount == 1 || Input.touchCount > 2)
                {
                    // Drag to rotate.
                    (Vector2 position, Vector2 delta) avgTouch = GetAverageTouch();
                    delta = avgTouch.delta * -1;
                }

                if (Input.touchCount == 2)
                {
                    zoomDelta = pinchZoom.GetZoomDelta() * -1;
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

            state.inputYaw += delta.x * CameraConfig.Sensitivity.x * -0.0001f;
            state.inputPitch += delta.y * CameraConfig.Sensitivity.y * -0.0001f;
            state.inputZoom += zoomDelta * CameraConfig.ZoomSensitivity * 0.01f;

            if (state.inputPitch < CameraConfig.PitchLimit.x) state.inputPitch = CameraConfig.PitchLimit.x;
            if (state.inputPitch > CameraConfig.PitchLimit.y) state.inputPitch = CameraConfig.PitchLimit.y;

            if (state.inputYaw < CameraConfig.YawLimit.x) state.inputYaw = CameraConfig.YawLimit.x;
            if (state.inputYaw > CameraConfig.YawLimit.y) state.inputYaw = CameraConfig.YawLimit.y;

            if (state.inputZoom > CameraConfig.ZoomLimit.y) state.inputZoom = CameraConfig.ZoomLimit.y;
            if (state.inputZoom < CameraConfig.ZoomLimit.x) state.inputZoom = CameraConfig.ZoomLimit.x;

            yawHarmonic.SetTarget(state.inputYaw);
            pitchHarmonic.SetTarget(state.inputPitch);
            zoomHarmonic.SetTarget(state.inputZoom);

            state.prevPosition = MousePosition;
        }

        private (Vector2 position, Vector2 delta) GetAverageTouch()
        {
            int touchCount = Input.touchCount;

            Vector2 avgPosition = Vector2.zero;
            Vector2 avgDelta = Vector2.zero;

            if (touchCount == 0)
            {
                return (avgPosition, avgDelta);
            }

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
            if (yawHarmonic.IsRunning || pitchHarmonic.IsRunning) return;

            state.inputYaw = state.inputYaw % 360;
            if (state.inputYaw < 0) state.inputYaw += 360;
            state.harmonicYaw = state.inputYaw;
        }

        private void HandleTransform()
        {
            Vector3 focus = CameraConfig.Focus;
            transform.position = focus + HarmonicDir * -Distance;
            transform.LookAt(focus, Vector3.up);
        }

        private static Vector3 EquirectangularProjection(Vector2 inputVector)
        {
            inputVector.y = 1 - inputVector.y;

            float xx = Mathf.Lerp(0, 1, (Mathf.Sin(inputVector.x * (2 * Mathf.PI)) * -1 + 1) / 2);
            float xy = Mathf.Cos((inputVector.y + 0.5f) / 2 * (2 * Mathf.PI)) + 1;
            float x = Mathf.Lerp(xx, 0.5f, xy);

            float yx = Mathf.Lerp(0, 1, (Mathf.Sin((inputVector.x + 0.25f) * (2 * Mathf.PI)) * -1 + 1) / 2);
            float yy = Mathf.Cos((inputVector.y + 0.5f) / 2 * (2 * Mathf.PI)) + 1;
            float y = Mathf.Lerp(yx, 0.5f, yy);

            float z = (Mathf.Cos(inputVector.y / 2 * (2 * Mathf.PI)) + 1) / 2;

            Vector3 xyz = new Vector3(-2 * x, 2 * z, 2 * y) + new Vector3(1, -1, -1);
            xyz.z = -xyz.z;

            return xyz.normalized;
        }
    }
}