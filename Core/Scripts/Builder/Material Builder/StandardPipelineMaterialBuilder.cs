using System;
using UnityEngine;

namespace Didimo.Builder
{
    public class StandardPipelineMaterialBuilder : MaterialBuilder
    {
        public override bool FindIdealShader(string shaderName, out Shader shader)
        {
            shader = Shader.Find("Standard");
            return shader != null;
        }

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

        protected override bool RequiresMaterialModification(string name, out Action<Material> modificationAction)
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

            modificationAction = null;
            return false;
        }

        // protected override bool RequiresTextureModification(string name, out Func<Texture, Texture> modificationAction) {
        // 	switch (name) {
        // 		case "roughness":
        // 		case "roughSampler":
        // 			modificationAction = TextureUtility.RoughnessToMetallicAlpha;
        // 			return true;
        // 		case "opacitySampler":
        // 			modificationAction = TextureUtility.OpacityToDiffuseAlpha;
        // 			return true;
        // 	}
        //
        // 	modificationAction = null;
        // 	return false;
        // }
    }
}