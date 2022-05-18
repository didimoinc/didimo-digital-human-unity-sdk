using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Mobile.Controller
{
    public class DidimoLookAtController : ASingletonBehaviour<DidimoLookAtController>
    {
        private bool lookAtEnabled = true;

        public void EnableLookAt(bool value)
        {
            lookAtEnabled = value;
            if (!lookAtEnabled)
            {
                headTracking.Reset();
                activeDidimo.Reset();
            }
        }

        [SerializeField]
        ObjectRotator objectRotator;

        [SerializeField]
        HeadTracking headTracking;

        private Tuple<DidimoComponents, Didimo> _activeDidimo;

        private Didimo activeDidimo
        {
            get
            {
                DidimoComponents didimo = DidimoCache.GetAllDidimos().FirstOrDefault(d => d.gameObject.activeSelf);
                if (didimo != null && (_activeDidimo == null || _activeDidimo.Item1 != didimo))
                {
                    _activeDidimo = new Tuple<DidimoComponents, Didimo>(didimo, new Didimo(didimo));
                }

                return _activeDidimo?.Item2;
            }
        }

        static Camera Camera => Camera.main;

        public void Reset()
        {
            activeDidimo?.Reset();
            headTracking.Reset();
            objectRotator.Reset();
        }

        void OnValidate() { objectRotator.OnValidate(); }

        void Start() { Reset(); }

        void LateUpdate()
        {
            if (lookAtEnabled)
            {
                headTracking.Update(activeDidimo);
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

            [Range(0, 1)]
            [SerializeField]
            float vSensitivity = 1;

            [Range(0, 1)]
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
                if (TouchController.TouchCount != 2)
                {
                    down = null;
                    return;
                }

                Touch touch1 = TouchController.GetTouch(0), touch2 = TouchController.GetTouch(1);
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                    down = new ControlDown
                    {
                        position = TouchController.ScreenPosition,
                        spreadDistance = Vector3.Distance(touch1.position, touch2.position),
                        cameraRotation = this._rotation,
                        cameraDistance = this._distance,
                    };
                if (down == null) return;

                var axis = TouchController.Unitize(TouchController.ScreenPosition - down.position);
                SetRotation(down.cameraRotation +
                            new Vector3(axis.y * vSensitivity * SensitivityScale * (vInvert ? -1 : 1), axis.x * hSensitivity * SensitivityScale * (hInvert ? -1 : 1)));

                var zoom = TouchController.Unitize(Vector3.Distance(touch1.position, touch2.position) - down.spreadDistance);
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
            const float SensitivityScale = 20;

            [Range(0, 1)]
            [SerializeField]
            float vSensitivity = 1;

            [Range(0, 1)]
            [SerializeField]
            float hSensitivity = 1;

            [SerializeField]
            [Range(0.1f, 1)]
            float bonesFollowWeight = 0.3f;

            // [SerializeField] Vector3 minEyeRotation = -Vector3.one * 180;
            // [SerializeField] Vector3 maxEyeRotation = Vector3.one * 180;
            // [SerializeField] Vector3 minHeadRotation = -Vector3.one * 180;
            // [SerializeField] Vector3 maxHeadRotation = Vector3.one * 180;

            [MinMaxSlider(-180, +180)]
            [SerializeField]
            MinMax<Vector3> eyeRotationLimits = new MinMax<Vector3>(new Vector3(-25, -30, 0), new Vector3(5, 30, 0));

            [MinMaxSlider(-180, +180)]
            [SerializeField]
            MinMax<Vector3> headRotationLimits = new MinMax<Vector3>(new Vector3(-45, -70, -15), new Vector3(45, 70, 15));

            // [Header("Output:")]
            [HideInInspector]
            [SerializeField]
            Vector3 headRotation;

            [HideInInspector]
            [SerializeField]
            Vector3 neckRotation;

            [HideInInspector]
            [SerializeField]
            Vector3 eyeRotation;

            [HideInInspector]
            [SerializeField]
            Vector3 targetRotation;

            Vector3? targetPosition;

            public void Update(Didimo didimo)
            {
                if (!Camera.main || didimo == null) return;

                if (TouchController.TouchCount == 1)
                    targetPosition = TouchController.GetLookPosition(hSensitivity * SensitivityScale + 1, vSensitivity * SensitivityScale + 1);

                if (targetPosition == null) return;

                Didimo.Bone leye = didimo.GetBone(Didimo.LeftEye);
                Didimo.Bone reye = didimo.GetBone(Didimo.RightEye);
                Didimo.Bone neck = didimo.GetBone(Didimo.Neck);
                Didimo.Bone head = didimo.GetBone(Didimo.Head);

                Quaternion rotation = Quaternion.LookRotation((Vector3) targetPosition - didimo.EyesCenter);
                leye.transform.rotation = reye.transform.rotation = Quaternion.Slerp(leye.transform.rotation, rotation, bonesFollowWeight);

                var euler = RepeatEuler(leye.transform.localEulerAngles);
                float y = euler.y < eyeRotationLimits.min.y ? euler.y - eyeRotationLimits.min.y : (euler.y > eyeRotationLimits.max.y ? euler.y - eyeRotationLimits.max.y : 0);
                float x = euler.x < eyeRotationLimits.min.x ? euler.x - eyeRotationLimits.min.x : (euler.x > eyeRotationLimits.max.x ? euler.x - eyeRotationLimits.max.x : 0);
                float z = euler.z;

                Quaternion targetRotation = head.transform.rotation * Quaternion.Euler(x, y, z);

                var headRotation = Quaternion.Slerp(head.transform.rotation, targetRotation, GetTimeWeight(bonesFollowWeight));
                headRotation = Quaternion.Euler(ClampEuler(RepeatEuler(headRotation.eulerAngles), headRotationLimits.min, headRotationLimits.max));

                neck.Reset();
                neck.transform.rotation = Quaternion.Lerp(neck.transform.rotation, headRotation, 0.5f);
                head.transform.rotation = headRotation;

                // clamp the eyes
                leye.transform.localEulerAngles = reye.transform.localEulerAngles = ClampEuler(euler, eyeRotationLimits.min, eyeRotationLimits.max);

                // debug
                this.eyeRotation = RepeatEuler(leye.transform.localEulerAngles);
                this.headRotation = RepeatEuler(head.transform.localEulerAngles);
                this.neckRotation = RepeatEuler(neck.transform.localEulerAngles);
                this.targetRotation = RepeatEuler(targetRotation.eulerAngles);
            }

            // public void Update(IEnumerable<Didimo> didimos)
            // {
            //     if (!Camera.main || didimos.Count() == 0) return;
            //
            //     foreach (var didimo in didimos)
            //     {
            //         Update(didimo);
            //     }
            // }

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
            public const string LeftEye = "LeftEye", RightEye = "RightEye", Head = "Head", Neck = "Neck";

            public DidimoComponents    didimo;
            public SkinnedMeshRenderer primarySkin;
            public GameObject gameObject => didimo.gameObject;

            Dictionary<string, Bone> bones = new Dictionary<string, Bone>();

            public Didimo(DidimoComponents didimo)
            {
                this.didimo = didimo;
                primarySkin = didimo.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(s => s.name.Contains("baseFace"));
                bones = primarySkin!.bones.ToDictionary(k => k.name, v => new Bone(v));
                bones.Remove(primarySkin.rootBone.name);
            }

            public Bone GetBone(string name) => bones[name];

            public Vector3 EyesCenter => (GetBone(LeftEye).transform.position + GetBone(RightEye).transform.position) / 2;

            public void Reset()
            {
                foreach (var bone in bones.Values) bone.Reset();
            }

            public class Bone
            {
                public readonly Transform transform;
                readonly        Vector3[] backupTransform;

                public Bone(Transform bone)
                {
                    this.transform = bone;
                    this.backupTransform = new[] {bone.localPosition, bone.localEulerAngles, bone.localScale};
                }

                public void Reset()
                {
                    transform.localPosition = backupTransform[0];
                    transform.localEulerAngles = backupTransform[1];
                    transform.localScale = backupTransform[2];
                }
            }
        }
    }
}