using System.Collections;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "Didimo Resources", menuName = "Didimo/Didimo Resources")]
    public class DidimoResources : ScriptableObject
    {
        [SerializeField]
        protected DidimoConfig didimoConfig;

        [SerializeField]
        protected ExpressionDatabase expressionDatabase;

        [SerializeField]
        protected MocapDatabase mocapDatabase;

        [SerializeField]
        protected DeformableDatabase deformableDatabase;

        [SerializeField]
        protected IrisDatabase irisDatabase;

        [SerializeField]
        protected CameraConfig cameraConfig;

        [SerializeField]
        protected ShaderResources shaderResources;

        [SerializeField]
        protected GameObject screenshotCamera;

        [SerializeField]
        protected Texture2D screenshotOverlay;

        [SerializeField]
        protected HairPresetDatabase hairPresetDatabase;

        private static DidimoResources _instance;

        public static DidimoConfig DidimoConfig => Instance.didimoConfig;
        public static ExpressionDatabase ExpressionDatabase => Instance.expressionDatabase;
        public static MocapDatabase MocapDatabase => Instance.mocapDatabase;
        public static DeformableDatabase DeformableDatabase => Instance.deformableDatabase;
        public static IrisDatabase IrisDatabase => Instance.irisDatabase;
        public static CameraConfig CameraConfig => Instance.cameraConfig;
        public static ShaderResources ShaderResources => Instance.shaderResources;
        public static GameObject ScreenshotCamera => Instance.screenshotCamera;
        public static Texture2D ScreenshotOverlay => Instance.screenshotOverlay;
        public static HairPresetDatabase HairPresetDatabase => Instance.hairPresetDatabase;
        public static IEnumerator ForceReload()
        {
            Resources.UnloadAsset(_instance);
            _instance = null;
            yield return Resources.UnloadUnusedAssets();
        }

        public static bool IsNull => Instance == null;

        private static DidimoResources Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<DidimoResources>("Didimo Resources");
                return _instance;
            }
        }
    }
}