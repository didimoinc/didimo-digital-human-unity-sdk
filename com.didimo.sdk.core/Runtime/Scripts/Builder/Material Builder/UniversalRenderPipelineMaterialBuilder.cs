using Didimo.Core.Config;
using System;
using System.IO;
using System.Linq;
using Didimo.Core.Utility;
using UnityEditor;
using UnityEngine;

namespace Didimo.Builder
{
    public class UniversalRenderingPipelineMaterialBuilder : MaterialBuilder
    {
       

        public override bool NameToProperty(
            string name, out string propertyName)
        {
            propertyName = name;

            switch (name)
            {
                case "colorSampler":
                    propertyName = "_BaseMap";
                    return true;
                case "specSampler":
                    propertyName = "_SpecularMap";
                    return true;
                case "roughSampler":
                    propertyName = "_RoughnessMap";
                    return true;
                case "aoSampler":
                    propertyName = "_AmbientOcclusionMap";
                    return true;
                case "cavitySampler":
                    propertyName = "_CavityMap";
                    return true;
                case "normalSampler":
                case "normalMap":
                    propertyName = "_NormalMap";
                    return true;
                case "microNormalSampler":
                    propertyName = "_NormalMicro";
                    return true;
                case "SpecColor":
                    propertyName = "_SpecColor";
                    return true;
                case "roughnessBias":
                    propertyName = "_RoughBias";
                    return true;
                case "zBias":
                    propertyName = "_ZBias";
                    return true;
                case "zBiasSampler":
                    propertyName = "_ZBiasMap";
                    return true;
                case "opacitySampler":
                    propertyName = "_OpacityMap";
                    return true;
                case "hairColor":
                case "diffColor":
                    propertyName = "_Color";
                    return true;
            }

            return true;
        }

        protected override bool RequiresMaterialModification(
            string name, out Action<Material> modificationAction)
        {
            void ModifyMaterial(Material material)
            {
                const string PROPERTY_NAME = "_EnableSRGBGammaCorrection";
                if (material.HasProperty(PROPERTY_NAME))
                {
                    material.SetFloat(PROPERTY_NAME, 1);
                }
            }

            modificationAction = ModifyMaterial;
            return true;
        }

        public override bool PostMaterialCreate(Material mat)
        {
            // var bodyPart = components.Parts.GetBodyPartType(renderer.transform);
            // MaterialUtility.FixupDefaultShaderParams(mat, bodyPart);            
            return true;
        }
    }
}