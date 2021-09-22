using System;
using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo
{
    [ExecuteInEditMode]
    public class DidimoEyeShadowController : DidimoBehaviour
    {
        public enum Eye
        {
            Undefined = -1,
            Left      = 0,
            Right     = 1
        }

        [SerializeField]
        protected float cornerTightness;

        [SerializeField]
        [InspectorName("AO Strength")]
        [Range(0.000f, 1.0f)]
        public float aoStrength = 0.864f;

        [SerializeField]
        [InspectorName("AO Sharpness")]
        [Range(0.0001f, 2.0f)]
        public float aoPOW = 0.112f;

        [SerializeField]
        [Range(0.00f, 360.0f)]
        protected float rotation = 179.0f;

        [SerializeField]
        [Range(-0.01f, 0.01f)]
        protected float offset_X = 0.00126f;

        [SerializeField]
        [Range(-0.01f, 0.01f)]
        protected float offset_Y = -0.0020f;
         
        [SerializeField]
        [Range(0.5f, 1.5f)]
        protected float scale_X = 0.912f;

        [SerializeField]
        [Range(0.5f, 1.5f)]
        protected float scale_Y = 1.0f;

        [SerializeField]
        [Range(0.0f, 5.0f)]
        protected float MainLightInfluence;

        [SerializeField, HideInInspector]
        SkinnedMeshRenderer LeftEye;

        [SerializeField, HideInInspector]
        SkinnedMeshRenderer RightEye;

        [SerializeField, HideInInspector]
        Transform[] LeftEyeHoleBoneTransforms;

        [SerializeField, HideInInspector]
        Transform[] RightEyeHoleBoneTransforms;

        [SerializeField, HideInInspector]
        public Transform HeadTransform;

        [SerializeField, HideInInspector]
        private Matrix4x4 LeftEyeHoleMatrix;

        [SerializeField, HideInInspector]
        private Matrix4x4 RightEyeHoleMatrix;

        public void Build(Importer.EyeShadowControllerSettings didimoEyeShadowControllerConfig)
        {
            LeftEyeHoleBoneTransforms = didimoEyeShadowControllerConfig.LeftEyelidJoints;
            RightEyeHoleBoneTransforms = didimoEyeShadowControllerConfig.RightEyelidJoints;
            HeadTransform = didimoEyeShadowControllerConfig.HeadJoint;
            LeftEye = didimoEyeShadowControllerConfig.LeftEyeMesh;
            RightEye = didimoEyeShadowControllerConfig.RightEyeMesh;   
        }

        public const string AOpowVarName            = "Vector1_ba14f804b81940f190085ab00e25f7e3";
        public const string AOstrengthVarName       = "Vector1_2d4a1ae612994689bb4f2d00e80976a8";
        public const string EyeHoleInvMatrixVarName = "Matrix4_4d20512c9f054b3e9b03b7fa5fbe725f";

        int AOpowVarNameID = -1;
        int AOstrengthVarNameID = -1;
        int EyeHoleInvMatrixVarNameID = -1;
        MaterialPropertyBlock LeftpropBlock = null;
        MaterialPropertyBlock RightpropBlock = null;

        public Transform[] GetEyeTransforms(Eye eye)
        {
            switch (eye)
            {
                case Eye.Left:
                    return LeftEyeHoleBoneTransforms;
                case Eye.Right:
                    return RightEyeHoleBoneTransforms;
                default:
                    throw new NotImplementedException($"Unrecognized Eye type: {eye}.");
            }
        }

        public Matrix4x4 GetEyeHoleMatrix(Eye eye)
        {
            switch (eye)
            {
                case Eye.Left:
                    return LeftEyeHoleMatrix;
                case Eye.Right:
                    return RightEyeHoleMatrix;
                default:
                    throw new NotImplementedException($"Unrecognized Eye type: {eye}.");
            }
        }

        private void SetEyeHoleMatrix(Eye eye, Matrix4x4 matrix4X4)
        {
            switch (eye)
            {
                case Eye.Left:
                    LeftEyeHoleMatrix = matrix4X4;
                    break;
                case Eye.Right:
                    RightEyeHoleMatrix = matrix4X4;
                    break;
                default:
                    throw new NotImplementedException($"Unrecognized Eye type: {eye}.");
            }
        }

        public void CalculateBounds(in Matrix4x4 mat, out Rect rect, Eye eye)
        {
            Matrix4x4 invMat = mat.inverse;

            float minx = float.MaxValue, miny = float.MaxValue, maxx = -float.MaxValue, maxy = -float.MaxValue;

            Transform[] eyeTransforms = GetEyeTransforms(eye);
            int nodeCount = eyeTransforms.Length;

            for (int i = 0; i < nodeCount; ++i)
            {
                Vector3 p = eyeTransforms[i].position;
                p = HeadTransform.InverseTransformPoint(p);
                p = invMat.MultiplyPoint(p);
                if (p.x < minx)
                    minx = p.x;
                if (p.y < miny)
                    miny = p.y;

                if (p.x > maxx)
                    maxx = p.x;
                if (p.y > maxy)
                    maxy = p.y;
            }

            rect = new Rect(minx, miny, maxx - minx, maxy - miny);
        }

        void CalculateBasisMatrix(Vector3 centreLine, Vector3 offs, out Matrix4x4 mat)
        {
            Vector3 xln = centreLine.normalized;
            Vector3 zin = new Vector3(0, 0, 1);
            Vector3 yup = Vector3.Cross(zin, centreLine);

            mat = new Matrix4x4(new Vector4(xln.x, xln.y, xln.z, 0.0f),
                new Vector4(yup.x, yup.y, yup.z, 0.0f),
                new Vector4(zin.x, zin.y, zin.z, 0.0f),
                new Vector4(offs.x, offs.y, offs.z, 1.0f));
        }

        void ProcessEye(Eye eye)
        {
            Vector3 centreLine, centreLineOffset;
            Rect bounds;
            Transform[] eyeTransforms = GetEyeTransforms(eye);
            MathUtils.CalculateLeastSquares(out centreLine, out centreLineOffset, eyeTransforms, HeadTransform, 0);
            Matrix4x4 eyeMat = GetEyeHoleMatrix(eye);
            CalculateBasisMatrix(centreLine, centreLineOffset, out eyeMat);
            CalculateBounds(eyeMat, out bounds, eye);
            float eyeNegator = eye == Eye.Left ? 1.0f : -1.0f;
            Matrix4x4 translate = Matrix4x4.Translate(new Vector3(this.offset_X * eyeNegator, this.offset_Y, 0.0f));
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(bounds.width * 0.5f * this.scale_X, bounds.height * 0.5f * this.scale_Y, 1.0f));
            Matrix4x4 rotate = Matrix4x4.Rotate(Quaternion.AngleAxis(this.rotation * eyeNegator, new Vector3(0, 0, 1)));
            eyeMat = HeadTransform.localToWorldMatrix * eyeMat * translate * rotate * scale ;
            SetEyeHoleMatrix(eye, eyeMat);            
        }

        void ProcessEyepropBlock(Eye eye, MaterialPropertyBlock propBlock, Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock);
            ProcessEye(eye);
            propBlock.SetFloat(AOpowVarNameID, aoPOW);
            propBlock.SetFloat(AOstrengthVarNameID, aoStrength);
            propBlock.SetMatrix(EyeHoleInvMatrixVarNameID, GetEyeHoleMatrix(eye).inverse);
            renderer.SetPropertyBlock(propBlock);
        }

        private void LateUpdate()
        {
            LeftpropBlock ??= new MaterialPropertyBlock();
            RightpropBlock ??= new MaterialPropertyBlock();

            if (AOpowVarNameID == -1)
                AOpowVarNameID = Shader.PropertyToID(DidimoEyeShadowController.AOpowVarName);
            if (EyeHoleInvMatrixVarNameID == -1)
                EyeHoleInvMatrixVarNameID = Shader.PropertyToID(DidimoEyeShadowController.EyeHoleInvMatrixVarName);
            if (AOstrengthVarNameID == -1)
                AOstrengthVarNameID = Shader.PropertyToID(DidimoEyeShadowController.AOstrengthVarName);

            ProcessEyepropBlock(Eye.Left, LeftpropBlock, LeftEye);
            ProcessEyepropBlock(Eye.Right, RightpropBlock, RightEye);
        }
    }   
}