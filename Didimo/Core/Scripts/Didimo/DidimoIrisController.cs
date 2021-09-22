using System;
using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo
{
 

    [System.Serializable]
    public class IrisPresetValues
    {
        static public IrisPresetValues[] presets = {  new IrisPresetValues(new Color(1.0f,1.0f,1.0f), null),    //BLACK = 0,
                                                                                                    
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
            Left      = 0,
            Right     = 1
        }
        
        [SerializeField]
        [InspectorName("Dilation")]
        [Range(0.0f, 0.03f)]
        public float dilation = 0.03f;

        [SerializeField]
        IrisPreset preset;

        Texture2D irisTexture = null;

        [SerializeField, HideInInspector]
        SkinnedMeshRenderer LeftEye;

        [SerializeField, HideInInspector]
        SkinnedMeshRenderer RightEye;



        public void Build(Importer.IrisControllerSettings DidimoIrisControllerConfig)
        {            
            LeftEye = DidimoIrisControllerConfig.LeftEyeMesh;
            RightEye = DidimoIrisControllerConfig.RightEyeMesh;   
        }

        public const string AOpowVarName            = "Vector1_ba14f804b81940f190085ab00e25f7e3";
        public const string AOstrengthVarName       = "Vector1_2d4a1ae612994689bb4f2d00e80976a8";
        public const string EyeHoleInvMatrixVarName = "Matrix4_4d20512c9f054b3e9b03b7fa5fbe725f";
        public const string DiffuseTextureName = "_BaseMap";
        public const string IrisScaleName = "_EyeUVScaleP1";

        int DiffuseTextureID = -1;
        int AOpowVarNameID = -1;
        int AOstrengthVarNameID = -1;
        int EyeHoleInvMatrixVarNameID = -1;
        int IrisScaleNameID = -1;
        MaterialPropertyBlock LeftpropBlock = null;
        MaterialPropertyBlock RightpropBlock = null;


        void ProcessEyepropBlock(Eye eye, MaterialPropertyBlock propBlock, Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock);
            if (irisTexture != null)
                propBlock.SetTexture(DiffuseTextureID, irisTexture);
            
            propBlock.SetFloat(IrisScaleNameID, dilation);            
            renderer.SetPropertyBlock(propBlock);
        }

        public void ApplyPreset()
        {
            Texture2D[] irises = IrisDatabase.Irises;
            irisTexture = irises[preset.value];
        }

        private void LateUpdate()
        {
            if (LeftEye != null && RightEye != null)
            {
                LeftpropBlock ??= new MaterialPropertyBlock();
                RightpropBlock ??= new MaterialPropertyBlock();
                if (DiffuseTextureID == -1)
                    DiffuseTextureID = Shader.PropertyToID(DidimoIrisController.DiffuseTextureName);
                if (AOpowVarNameID == -1)
                    AOpowVarNameID = Shader.PropertyToID(DidimoIrisController.AOpowVarName);
                if (EyeHoleInvMatrixVarNameID == -1)
                    EyeHoleInvMatrixVarNameID = Shader.PropertyToID(DidimoIrisController.EyeHoleInvMatrixVarName);
                if (AOstrengthVarNameID == -1)
                    AOstrengthVarNameID = Shader.PropertyToID(DidimoIrisController.AOstrengthVarName);
                //if (IrisScaleNameID == -1)
                    IrisScaleNameID = Shader.PropertyToID(DidimoIrisController.IrisScaleName);

                ProcessEyepropBlock(Eye.Left, LeftpropBlock, LeftEye);
                ProcessEyepropBlock(Eye.Right, RightpropBlock, RightEye);
            }
        }
    }   
}