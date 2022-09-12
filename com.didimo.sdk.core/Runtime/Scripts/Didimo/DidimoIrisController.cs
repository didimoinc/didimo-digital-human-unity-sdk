using Didimo.Core.Config;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo
{
    [System.Serializable, ExecuteInEditMode]
    public class IrisPresetValues
    {
        static public IrisPresetValues[] presets =
        {
            new IrisPresetValues(new Color(1.0f, 1.0f, 1.0f), null), //BLACK = 0,
        };

        public Color Color = new Color(1.0f, 1.0f, 1.0f);
        public Texture IrisTexture;

        public IrisPresetValues(Color col, Texture tex)
        {
            Color = col;
            IrisTexture = tex;
        }
    }

    [System.Serializable]
    public class IrisPreset
    {
        public int value;
    }

    [ExecuteInEditMode]
    public class DidimoIrisController : DidimoBehaviour
    {
        public enum Eye
        {
            Undefined = -1,
            Left = 0,
            Right = 1
        }

        [SerializeField]
        [InspectorName("Dilation")]
        [Range(0.0f, 0.03f)]
        public float dilation = 0.03f;

        [SerializeField]
        IrisPreset preset = new IrisPreset();

        Texture2D irisTexture = null;

        SkinnedMeshRenderer LeftEye => DidimoComponents.Parts.LeftEyeMeshRenderer;

        SkinnedMeshRenderer RightEye => DidimoComponents.Parts.RightEyeMeshRenderer;


        public const string DiffuseTextureName = "_BaseMap";
        public const string IrisScaleName = "_EyeUVScaleP1";

        int DiffuseTextureID = -1;    
        int IrisScaleNameID = -1;
        MaterialPropertyBlock LeftpropBlock = null;
        MaterialPropertyBlock RightpropBlock = null;
        
        void ProcessEyepropBlock(Eye eye, MaterialPropertyBlock propBlock, Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock, 0);
            if (irisTexture != null)
            {
                propBlock.SetTexture(DiffuseTextureID, irisTexture);
                propBlock.SetFloat(IrisScaleNameID, dilation);
                renderer.SetPropertyBlock(propBlock, 0);
            }
        }

        public void SetPreset(int presetID)
        {
            preset ??= new IrisPreset();

            preset.value = presetID;
            ApplyPreset();
        }

        public void ApplyPreset()
        {
            var irisDatabase = Resources.Load<IrisDatabase>("IrisDatabase");
            irisTexture = irisDatabase.Irises[preset.value];
            ApplyBlocks();
        }

        // public void OnValidate()
        // {
        //     ApplyBlocks();
        // }

        void ApplyBlocks()
        {
            if (!string.IsNullOrEmpty(DidimoComponents.didimoVersion) && LeftEye != null && RightEye != null)
            {
                LeftpropBlock ??= new MaterialPropertyBlock();
                RightpropBlock ??= new MaterialPropertyBlock();
                ProcessIDs();

                ProcessEyepropBlock(Eye.Left, LeftpropBlock, LeftEye);
                ProcessEyepropBlock(Eye.Right, RightpropBlock, RightEye);
            }
        }

        private void ProcessIDs()
        {
            if (DiffuseTextureID == -1)
                DiffuseTextureID = Shader.PropertyToID(DiffuseTextureName);
         
            if (IrisScaleNameID == -1)
                IrisScaleNameID = Shader.PropertyToID(IrisScaleName);
        }

        private void LateUpdate()
        {
            ApplyBlocks();
        }
    }
}