using Didimo.Core.Inspector;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "CameraConfig", menuName = "Didimo/Camera/Camera Config")]
    public class CameraConfig : ScriptableObject
    {
        public Vector3 focus;
        public float distance;
        public Vector2 sensitivity;
        public float zoomSensitivity;

        public float defaultYaw;
        public float defaultPitch;

        [Header("Limits")]
        [MinMaxSlider(0, 1)]
        public Vector2 pitchLimit;

        [MinMaxSlider(0, 1)]
        public Vector2 yawLimit;

        [MinMaxSlider(-2, 2)]
        public Vector2 zoomLimit;

        [Header("Depth of Field")]
        [SerializeField]
        public float depthOfFieldDistanceBias;

        [SerializeField]
        public float focalLength;
    }
}