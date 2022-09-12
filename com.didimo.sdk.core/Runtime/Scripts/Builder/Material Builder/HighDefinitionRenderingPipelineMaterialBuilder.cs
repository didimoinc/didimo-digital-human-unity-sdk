using Didimo.Core.Config;
using Didimo.Core.Utility;
using System;
using UnityEngine;

namespace Didimo.Builder
{    
    public class HighDefinitionRenderingPipelineMaterialBuilder : MaterialBuilder
    {       
        public override bool NameToProperty(string name, out string propertyName)
        {
            switch (name)
            {
                case "Color":
                    propertyName = "_Color";
                    return true;
                case "colorSampler":
                    propertyName = "_MainTex";
                    return true;
                case "opacitySampler":
                    propertyName = "_MainTex";
                    return true;
                case "roughness":
                case "roughSampler":
                    propertyName = "_MetallicGlossMap";
                    return true;
                case "normalSampler":
                    propertyName = "_BumpMap";
                    return true;
                case "aoSampler":
                    propertyName = "_OcclusionMap";
                    return true;
            }

            propertyName = name;
            return false;
        }

        protected override bool RequiresMaterialModification(
            string name, out Action<Material> modificationAction)
        {
            switch (name)
            {
                case "eyelashMat":
                    modificationAction = (m) =>
                    {
                        MaterialUtility.SetBlendMode(m, MaterialUtility.BlendMode.Transparent);
                    };
                    return true;
            }
            modificationAction = (m) =>
            {
                // var bodyPart = ShaderResources.GetBodyPartID(name);
                // MaterialUtility.FixupDefaultShaderParams(m, bodyPart);                
            };
            return true;

        }

        public override bool PostMaterialCreate(Material mat)
        {
            // var bodyPart = ShaderResources.GetBodyPartID(mat.name);
            // MaterialUtility.FixupDefaultShaderParams(mat, bodyPart);
            return true;
        }           
    }
}