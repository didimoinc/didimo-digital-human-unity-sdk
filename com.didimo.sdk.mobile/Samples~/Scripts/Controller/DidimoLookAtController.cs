using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;
using UnityEngine.Analytics;

namespace Didimo.Mobile.Controller
{
    public class DidimoLookAtController : ASingletonBehaviour<DidimoLookAtController>
    {
        // const bool IsolationMode = true;
        private bool lookAtEnabled = true;

        // For ASingletonBehaviours, we need to use OnAwake, as using Awake will prevent ASingletonBehaviour from working.
        protected override void OnAwake()
        {
            foreach (var didimo in FindObjectsOfType<DidimoComponents>())
                DidimoCache.Add(didimo);
        }

        public void EnableLookAt(bool value)
        {
            lookAtEnabled = value;
            if (!lookAtEnabled)
            {
                headTracking.Reset();
                ActiveDidimo.ResetToBindPose();
                TouchController.Instance.Reset();
            }
        }

        [SerializeField]
        ObjectRotator objectRotator;

        [SerializeField]
        HeadTracking headTracking;

        private Tuple<DidimoComponents, Didimo> _activeDidimo;

        DidimoComponents FindDidimo() => DidimoCache.GetAllDidimos().FirstOrDefault(d => d.gameObject.activeSelf);

        private Didimo ActiveDidimo
        {
            get
            {
                DidimoComponents didimo = FindDidimo();
                if (didimo != null && (_activeDidimo == null || _activeDidimo.Item1 != didimo))
                {
                    _activeDidimo = new Tuple<DidimoComponents, Didimo>(didimo, new Didimo(didimo));
                }

                return _activeDidimo?.Item2;
            }
        }

        static Camera Camera => Camera.main;

        [Button]
        public void Reset() { Reset(true); }

        public void Reset(bool resetCamera)
        {
            foreach (DidimoComponents didimoComponents in DidimoCache.GetAllDidimos())
            {
                Didimo didimo = new(didimoComponents);
                didimo.ResetToBindPose();
                headTracking.CacheValues(didimo);
            }

            headTracking.Reset();

            TouchController.Instance.Reset();

            if (resetCamera)
            {
                objectRotator.Reset();
            }
        }

        void OnValidate()
        {
            try
            {
                objectRotator.OnValidate();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        void Start() { Reset(); }

        void LateUpdate()
        {
            if (lookAtEnabled)
            {
                headTracking.Update(ActiveDidimo);
            }

            objectRotator.Update();
        }

        [Serializable]
        class ObjectRotator
        {
            const float SensitivityScale = 100;

            [SerializeField]
            Vector3 origin = new Vector3(0, 1.69f, 0);

            [SerializeField]
            bool vInvert = false;

            [SerializeField]
            bool hInvert = false;

            [Range(0, 2)]
            [SerializeField]
            float vSensitivity = 1;

            [Range(0, 2)]
            [SerializeField]
            float hSensitivity = 1;

            [Range(0, 1)]
            [SerializeField]
            float zoomSensitivity = 1;

            [SerializeField]
            Vector3 rotation = new Vector3(-1.8f, 180, 0);

            [MinMaxSlider(-180, +180)]
            [SerializeField]
            MinMax<Vector3> rotationLimits = new MinMax<Vector3>(new Vector3(-25, -180, 0), new Vector3(70, 180, 0));

            [SerializeField]
            [Range(0.1f, 1)]
            float distance = 0.5f;

            [MinMaxSlider(0.1f, 1)]
            [SerializeField]
            MinMax<float> distanceLimits = new MinMax<float>(0.49f, 0.63f);

            [MinMaxSlider(5, 40)]
            [SerializeField]
            MinMax<float> fovLimits = new MinMax<float>(8, 16);

            // [Header("Output:")]
            [HideInInspector]
            [SerializeField]
            Vector3 _rotation;

            [HideInInspector]
            [SerializeField]
            float _distance;

            [HideInInspector]
            [SerializeField]
            float _fov;

            ControlDown down;

            public void OnValidate() { Reset(); }

            public void Reset()
            {
                SetDistance(distance);
                SetRotation(rotation);
                UpdateCamera();
                down = null;
            }

            public void Update()
            {
                if (TouchController.Instance.TouchCount != 2)
                {
                    down = null;
                    return;
                }

                Touch touch1 = TouchController.Instance.GetTouch(0), touch2 = TouchController.Instance.GetTouch(1);
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    down = new ControlDown
                    {
                        position = TouchController.Instance.ScreenPosition,
                        spreadDistance = Vector3.Distance(touch1.position, touch2.position),
                        cameraRotation = this._rotation,
                        cameraDistance = this._distance,
                    };
                }

                if (down == null) return;

                var axis = TouchController.Instance.Unitize(TouchController.Instance.ScreenPosition - down.position);
                SetRotation(down.cameraRotation +
                            new Vector3(axis.y * vSensitivity * SensitivityScale * (vInvert ? -1 : 1), axis.x * hSensitivity * SensitivityScale * (hInvert ? -1 : 1)));

                var zoom = TouchController.Instance.Unitize(Vector3.Distance(touch1.position, touch2.position) - down.spreadDistance);
                SetDistance(down.cameraDistance - zoom * zoomSensitivity);

                UpdateCamera();
            }

            void SetRotation(Vector3 rotation) => _rotation = ClampEuler(RepeatEuler(rotation), rotationLimits.min, rotationLimits.max);
            void SetDistance(float distance) => _distance = Mathf.Clamp(distance, distanceLimits.min, distanceLimits.max);
            void SetFOV() => _fov = Mathf.Clamp01((_distance - distanceLimits.min) / (distanceLimits.max - distanceLimits.min)) * (fovLimits.max - fovLimits.min) + fovLimits.min;

            void UpdateCamera()
            {
                if (!Camera) return;
                SetFOV();
                Camera.transform.position = origin;
                Camera.transform.eulerAngles = _rotation;
                Camera.transform.position -= Camera.transform.forward * _distance;
                Camera.fieldOfView = _fov;
            }

            class ControlDown
            {
                public Vector3 position,       cameraRotation;
                public float   spreadDistance, cameraDistance;
            }
        }

        [Serializable]
        class HeadTracking
        {
            Quaternion      leyeCache = Quaternion.identity;
            Quaternion      headCache = Quaternion.identity;
            private Vector3 eyeCenter;
            private Vector3 leftEyeForward;

            const float SensitivityScale = 20;

            [Range(0, 2)]
            [SerializeField]
            float vSensitivity = 1;

            [Range(0, 2)]
            [SerializeField]
            float hSensitivity = 1;

            [SerializeField]
            [Range(0.1f, 1)]
            float bonesFollowWeight = 0.3f;

            [SerializeField]
            [Range(0.1f, 1)]
            float neckContribution = 0.7f;

            [MinMaxSlider(-180, +180)]
            [SerializeField]
            MinMax<Vector3> eyeRotationLimits = new MinMax<Vector3>(new Vector3(-25, -30, 0), new Vector3(5, 30, 0));

            [MinMaxSlider(-180, +180)]
            [SerializeField]
            MinMax<Vector3> headRotationLimits = new MinMax<Vector3>(new Vector3(-45, -70, -15), new Vector3(45, 70, 15));

            Vector3? targetPosition;

            public void CacheValues(Didimo didimo)
            {
                leyeCache = didimo.LeftEyeBone.rotation;
                headCache = didimo.HeadBone.rotation;
                eyeCenter = (didimo.LeftEyeBone.transform.position + didimo.RightEyeBone.transform.position) / 2;
                leftEyeForward = didimo.LeftEyeBone.forward;
            }

            public void Update(Didimo didimo)
            {
                if (!Camera.main || didimo == null) return;

                if (TouchController.Instance.TouchCount == 1 && TouchController.Instance.GetTouch(0).phase == TouchPhase.Moved)
                {
                    targetPosition = TouchController.Instance.GetLookPosition(hSensitivity * SensitivityScale + 1, vSensitivity * SensitivityScale + 1);
                }

                if (targetPosition == null) return;

                Transform leye = didimo.LeftEyeBone;
                Transform reye = didimo.RightEyeBone;
                Transform neck = didimo.NeckBone;
                Transform head = didimo.HeadBone;

                var side = Vector3.Dot(leftEyeForward, (Vector3)targetPosition - eyeCenter);
                if (side < 0) return;

                Quaternion rotation = Quaternion.LookRotation((Vector3)targetPosition - eyeCenter);
                leye.transform.rotation = reye.transform.rotation = Quaternion.Slerp(leyeCache, rotation, bonesFollowWeight);

                var euler = RepeatEuler(leye.transform.localEulerAngles);
                float y = euler.y < eyeRotationLimits.min.y ? euler.y - eyeRotationLimits.min.y : (euler.y > eyeRotationLimits.max.y ? euler.y - eyeRotationLimits.max.y : 0);
                float x = euler.x < eyeRotationLimits.min.x ? euler.x - eyeRotationLimits.min.x : (euler.x > eyeRotationLimits.max.x ? euler.x - eyeRotationLimits.max.x : 0);
                float z = euler.z;

                Quaternion targetRotation = headCache * Quaternion.Euler(x, y, z);

                var headRotation = Quaternion.Slerp(headCache, targetRotation, GetTimeWeight(bonesFollowWeight));
                headRotation = Quaternion.Euler(ClampEuler(RepeatEuler(headRotation.eulerAngles), headRotationLimits.min, headRotationLimits.max));

                didimo.ResetNeck();
                neck.transform.rotation = Quaternion.Lerp(neck.transform.rotation, headRotation, neckContribution);
                head.transform.rotation = headRotation;

                // clamp the eyes
                leye.transform.localEulerAngles = reye.transform.localEulerAngles = ClampEuler(euler, eyeRotationLimits.min, eyeRotationLimits.max);
                
                CacheValues(didimo);
            }

            static float GetTimeWeight(float weight) => weight * 10 * Time.deltaTime;

            public void Reset() => targetPosition = null;
        }

        static Vector3 ClampEuler(Vector3 euler, Vector3 minAngle, Vector3 maxAngle) =>
            new Vector3(Mathf.Clamp(euler.x, minAngle.x, maxAngle.x), Mathf.Clamp(euler.y, minAngle.y, maxAngle.y), Mathf.Clamp(euler.z, minAngle.z, maxAngle.z));

        static Vector3 RepeatEuler(Vector3 euler) =>
            new Vector3(euler.x > 180 ? euler.x - 360 : (euler.x < -180 ? euler.x + 360 : euler.x),
                euler.y > 180 ? euler.y - 360 : (euler.y < -180 ? euler.y + 360 : euler.y),
                euler.z > 180 ? euler.z - 360 : (euler.z < -180 ? euler.z + 360 : euler.z));

        [Serializable]
        struct MinMax<T>
        {
            public T min, max;

            public MinMax(T min, T max)
            {
                this.min = min;
                this.max = max;
            }
        }

        class Didimo
        {
            private const string LeftEye = "LeftEye", RightEye = "RightEye", Head = "Head", Neck = "Neck";

            public  DidimoComponents    didimo;
            private Matrix4x4           neckInverseBindPose;
            private SkinnedMeshRenderer primarySkin;
            public Transform LeftEyeBone { get; private set; }
            public Transform RightEyeBone { get; private set; }
            public Transform HeadBone { get; private set; }

            public Transform NeckBone { get; private set; }

            public void ResetNeck()
            {
                Matrix4x4 wBone = primarySkin.rootBone.localToWorldMatrix * neckInverseBindPose;
                NeckBone.position = wBone.MultiplyPoint3x4(Vector3.zero);
                NeckBone.rotation = wBone.rotation;
            }

            public Didimo(DidimoComponents didimo)
            {
                this.didimo = didimo;
                primarySkin = didimo.Parts.HeadMeshRenderer;
                LeftEyeBone = primarySkin.bones.FirstOrDefault(b => b.name == LeftEye);
                RightEyeBone = primarySkin.bones.FirstOrDefault(b => b.name == RightEye);
                HeadBone = didimo.Parts.HeadJoint;
                NeckBone = primarySkin.bones.FirstOrDefault(b => b.name == Neck);

                int neckBoneIndex = Array.IndexOf(primarySkin.bones, NeckBone);
                neckInverseBindPose = primarySkin.sharedMesh.bindposes[neckBoneIndex].inverse;
            }

            //public Vector3 EyesCenterForward => LeftEyeBone.forward;

            public void ResetToBindPose()
            {
                Transform[] bones = primarySkin.bones;
                for (int i = 0; i < bones.Length; i++)
                {
                    if (bones[i] == didimo.transform) continue;
                    Matrix4x4 wBone = primarySkin.rootBone.localToWorldMatrix * primarySkin.sharedMesh.bindposes[i].inverse;
                    primarySkin.bones[i].position = wBone.MultiplyPoint3x4(Vector3.zero);
                    primarySkin.bones[i].rotation = wBone.rotation;
                }
            }
        }
    }
}