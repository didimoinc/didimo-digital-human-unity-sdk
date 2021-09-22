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

        public static IEnumerator ForceReload()
        {
            Resources.UnloadAsset(_instance);
            _instance = null;
            yield return Resources.UnloadUnusedAssets();
        }

        private static DidimoResources Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<DidimoResources>("Didimo Resources");
#if UNITY_EDITOR

                // On project re-import Resources.Load might fail.
                // This will fix that issue, but the asset must be in the following path.
                // TODO: Find a better solution
                if (_instance == null) _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<DidimoResources>("Assets/Didimo/Core/Content/Resources/Didimo Resources.asset");
#endif
                return _instance;
            }
        }
    }
}