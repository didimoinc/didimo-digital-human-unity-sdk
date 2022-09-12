using System;
using UnityEngine;

namespace Didimo.Core.Utility
{
    public class DidimoParts : DidimoBehaviour
    {
        public enum BodyPart
        {
            Unknown,
            HeadMesh,
            EyeLashesMesh,
            MouthMesh,
            BodyMesh,
            ClothingMesh,
            LeftEyeMesh,
            RightEyeMesh,
            HeadJoint,
            HairMesh
        }

        public Renderer BodyPartToRenderer(BodyPart bodyPart)
        {
            switch (bodyPart)
            {
                case BodyPart.BodyMesh:      return BodyMeshRenderer;
                case BodyPart.MouthMesh:     return MouthMeshRenderer;
                case BodyPart.ClothingMesh:  return ClothingMeshRenderer;
                case BodyPart.HairMesh:      return HairMesh != null ? HairMesh.GetComponent<Renderer>() : null;
                case BodyPart.HeadMesh:      return HeadMeshRenderer;
                case BodyPart.EyeLashesMesh: return EyeLashesMeshRenderer;
                case BodyPart.LeftEyeMesh:   return LeftEyeMeshRenderer;
                case BodyPart.RightEyeMesh:  return RightEyeMeshRenderer;
                default:                     return null;
            }
        }

        public BodyPart GetBodyPartType(Transform t)
        {
            if (HeadMeshRenderer != null && t == HeadMeshRenderer.transform) return BodyPart.HeadMesh;
            if (EyeLashesMeshRenderer != null && t == EyeLashesMeshRenderer.transform) return BodyPart.EyeLashesMesh;
            if (MouthMeshRenderer != null && t == MouthMeshRenderer.transform) return BodyPart.MouthMesh;
            if (BodyMeshRenderer != null && t == BodyMeshRenderer.transform) return BodyPart.BodyMesh;
            if (ClothingMeshRenderer != null && t == ClothingMeshRenderer.transform) return BodyPart.ClothingMesh;
            if (LeftEyeMeshRenderer != null && t == LeftEyeMeshRenderer.transform) return BodyPart.LeftEyeMesh;
            if (RightEyeMeshRenderer != null && t == RightEyeMeshRenderer.transform) return BodyPart.RightEyeMesh;
            if (HeadJoint != null && t == HeadJoint.transform) return BodyPart.HeadJoint;
            if (HairMesh != null && t == HairMesh.transform) return BodyPart.HairMesh;

            return BodyPart.Unknown;
        }

        private bool initialized;

        protected class Configuration
        {
            public string headJointName     = "Head";
            public string headMeshName      = "mesh_m_low_baseFace_001";
            public string eyeLashesMeshName = "mesh_m_low_eyeLashes_001";
            public string mouthMeshName     = "mesh_m_low_mouth_001";
            public string bodyMeshName      = "mesh_m_low_baseBody_001";
            public string clothingMeshName  = "mesh_m_low_baseClothing_001";
            public string leftEyeMeshName   = "mesh_l_low_eye_001";
            public string rightEyeMeshName  = "mesh_r_low_eye_001";
            public string hairMeshName      = "hair_001";

            public string[] leftEyelidJointNames =
            {
                "DemLuUpperEyelidLevelTwo01", "DemLuUpperEyelidLevelTwo02", "DemLuUpperEyelidLevelTwo03", "DemLuUpperEyelidLevelTwo04", "DemLuUpperEyelidLevelTwo05",
                "DemLuUpperEyelidLevelTwo06", "DemLuUpperEyelidLevelTwo07", "DemLuUpperEyelidLevelTwo08", "DemLdLowerEyelidLevelTwo08", "DemLdLowerEyelidLevelTwo07",
                "DemLdLowerEyelidLevelTwo06", "DemLdLowerEyelidLevelTwo05", "DemLdLowerEyelidLevelTwo04", "DemLdLowerEyelidLevelTwo02", "DemLdLowerEyelidLevelTwo01",
            };

            public string[] rightEyelidJointNames =
            {
                "DemRuUpperEyelidLevelTwo01", "DemRuUpperEyelidLevelTwo02", "DemRuUpperEyelidLevelTwo03", "DemRuUpperEyelidLevelTwo04", "DemRuUpperEyelidLevelTwo05",
                "DemRuUpperEyelidLevelTwo06", "DemRuUpperEyelidLevelTwo07", "DemRuUpperEyelidLevelTwo08", "DemRdLowerEyelidLevelTwo08", "DemRdLowerEyelidLevelTwo07",
                "DemRdLowerEyelidLevelTwo06", "DemRdLowerEyelidLevelTwo05", "DemRdLowerEyelidLevelTwo04", "DemRdLowerEyelidLevelTwo02", "DemRdLowerEyelidLevelTwo01"
            };
        }

        public SkinnedMeshRenderer HeadMeshRenderer;
        public SkinnedMeshRenderer EyeLashesMeshRenderer;
        public SkinnedMeshRenderer MouthMeshRenderer;
        public SkinnedMeshRenderer BodyMeshRenderer;
        public SkinnedMeshRenderer ClothingMeshRenderer;
        public SkinnedMeshRenderer LeftEyeMeshRenderer;
        public SkinnedMeshRenderer RightEyeMeshRenderer;
        public Transform HeadJoint;
        public Transform HairMesh;
        public Transform[] LeftEyelidJointsRenderer;
        public Transform[] RightEyelidJointsRenderer;

        private DidimoParts() { }

        public void SetupForDidimoVersion(string didimoVersion, bool forceUpdate = false)
        {
            if (!forceUpdate && initialized) return;

            Configuration configuration;
            switch (didimoVersion)
            {
                case "":
                case null:
                    configuration = SetupForVersion0();
                    break;
                case "2.5":
                    configuration = SetupForVersion2_5();
                    break;
                case "2.5.1":
                    configuration = SetupForVersion2_5_1();
                    break;
                default:
                    throw new ArgumentException($"Unsupported didimo version: {didimoVersion}");
            }

            SetTransforms(configuration);
            initialized = true;
        }

        public void SetupForPartNames(string headJoint, string headMesh, string eyeLashesMesh, string mouthMesh, string leftEyeMesh, string rightEyeMesh, string hairMesh = null,
            string bodyMesh = null, string clothingMesh = null)
        {
            Configuration configuration = new Configuration();
            configuration.headMeshName = headMesh;
            configuration.eyeLashesMeshName = eyeLashesMesh;
            configuration.mouthMeshName = mouthMesh;
            configuration.leftEyeMeshName = leftEyeMesh;
            configuration.rightEyeMeshName = rightEyeMesh;
            configuration.hairMeshName = hairMesh;
            configuration.bodyMeshName = bodyMesh;
            configuration.clothingMeshName = clothingMesh;

            SetTransforms(configuration);
        }

        private void SetTransforms(Configuration configuration)
        {
            HeadJoint = GetTransform(DidimoComponents.transform, configuration.headJointName);
            HairMesh = GetTransform(DidimoComponents.transform, configuration.hairMeshName, false);
            HeadMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.headMeshName);
            EyeLashesMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.eyeLashesMeshName);
            MouthMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.mouthMeshName);
            LeftEyeMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.leftEyeMeshName);
            RightEyeMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.rightEyeMeshName);
            BodyMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.bodyMeshName, false);
            ClothingMeshRenderer = GetSkinnedMeshRenderer(DidimoComponents.transform, configuration.clothingMeshName, false);

            LeftEyelidJointsRenderer = SetTransforms(DidimoComponents.transform, configuration.leftEyelidJointNames);
            RightEyelidJointsRenderer = SetTransforms(DidimoComponents.transform, configuration.rightEyelidJointNames);
        }

        private static SkinnedMeshRenderer GetSkinnedMeshRenderer(Transform didimoRoot, string transformName, bool required = true)
        {
            return ComponentUtility.SafeGetComponent<SkinnedMeshRenderer>(GetTransform(didimoRoot, transformName, required)?.gameObject);
        }

        private static Transform GetTransform(Transform didimoRoot, string transformName, bool required = true)
        {
            if (string.IsNullOrEmpty(transformName)) return null;

            if (!didimoRoot.TryFindRecursive(transformName, out Transform result) && required)
            {
                Debug.LogWarning($"Could not find transform with name {transformName}");
            }

            return result;
        }

        private static Transform[] SetTransforms(Transform didimoRoot, string[] transformNames)
        {
            if (transformNames == null) return null;

            Transform[] result = new Transform[transformNames.Length];

            try
            {
                for (int i = 0; i < transformNames.Length; i++)
                {
                    result[i] = GetTransform(didimoRoot, transformNames[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return result;
        }

        private Configuration SetupForVersion0()
        {
            Configuration configuration = new Configuration();
            configuration.headJointName = "JntMHead";
            configuration.headMeshName = "mesh_m_low_baseFace_001";
            configuration.eyeLashesMeshName = null;
            configuration.mouthMeshName = "mesh_m_low_mouthTongue_001";
            configuration.bodyMeshName = null;
            configuration.clothingMeshName = null;
            configuration.leftEyeMeshName = "mesh_l_low_cornea_001";
            configuration.rightEyeMeshName = "mesh_r_low_cornea_001";
            configuration.hairMeshName = null;
            configuration.leftEyelidJointNames = null;
            configuration.rightEyelidJointNames = null;
            return configuration;
        }

        private Configuration SetupForVersion2_5() { return new Configuration(); }

        private Configuration SetupForVersion2_5_1()
        {
            Configuration configuration = new Configuration();
            configuration.headMeshName = "ddmo_head";
            configuration.eyeLashesMeshName = "ddmo_eyelashes";
            configuration.mouthMeshName = "ddmo_mouth";
            configuration.bodyMeshName = "ddmo_body";
            configuration.clothingMeshName = "ddmo_clothing";
            configuration.leftEyeMeshName = "ddmo_eyeLeft";
            configuration.rightEyeMeshName = "ddmo_eyeRight";
            configuration.hairMeshName = "ddmo_hair";

            return configuration;
        }
    }
}