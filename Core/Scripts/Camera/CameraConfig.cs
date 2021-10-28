using Didimo.Inspector;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Didimo/Camera/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Default Settings")]
        [SerializeField]
        protected Vector3 focus;

        [SerializeField]
        protected float distance;

        [SerializeField]
        protected Vector2 sensitivity;

        [SerializeField]
        protected float zoomSensitivity;

        [SerializeField]
        protected float defaultPitch;

        [Header("Limits")]
        [SerializeField]
        [MinMaxSlider(0, 1)]
        protected Vector2 pitchLimit;

        [SerializeField]
        [MinMaxSlider(0, 1)]
        protected Vector2 yawLimit;

        [SerializeField]
        [MinMaxSlider(-2, 2)]
        protected Vector2 zoomLimit;

        [Header("Depth of Field")]
        [SerializeField]
        protected float depthOfFieldDistanceBias;

        [SerializeField]
        protected float focalLength;

        public static Vector3 Focus => DidimoResources.CameraConfig.focus;
        public static float Distance => DidimoResources.CameraConfig.distance;
        public static Vector2 Sensitivity => DidimoResources.CameraConfig.sensitivity;
        public static float ZoomSensitivity => DidimoResources.CameraConfig.zoomSensitivity;
        public static float DefaultPitch => DidimoResources.CameraConfig.defaultPitch;

        public static Vector2 PitchLimit => DidimoResources.CameraConfig.pitchLimit;
        public static Vector2 YawLimit => DidimoResources.CameraConfig.yawLimit;
        public static Vector2 ZoomLimit => DidimoResources.CameraConfig.zoomLimit;
        public static float DepthOfFieldDistanceBias => DidimoResources.CameraConfig.depthOfFieldDistanceBias;
        public static float FocalLength => DidimoResources.CameraConfig.focalLength;
    }
}