using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Didimo.Core.Config;
using Didimo.Core.Model;
using Didimo.Core.Utility;
using UnityEngine.Rendering;

namespace Didimo.Builder
{
    public abstract class MaterialBuilder
    {
        public Transform TargetTransformOverride { get; set; }

        public async Task<bool> TryBuild(DidimoBuildContext context, MaterialDataContainer materialDataContainer)
        {
            await materialDataContainer.Prepare(context, this);

            if (TargetTransformOverride != null)
            {
                materialDataContainer.ApplyToTarget(context, this, TargetTransformOverride);
            }

            materialDataContainer.ApplyToHierarchy(context, this);

            return true;
        }

        public bool TryBuildMaterial(DidimoBuildContext context, MaterialData data, out Material material)
        {
            if (!FindIdealShader(data.ShaderName, out Shader shader))
            {
                Debug.LogWarning($"Failed to load Material Instance. " +
                    "Could not locate appropriate shader ({data.ShaderName})");
                material = null;
                return false;
            }

            material = new Material(shader) { name = data.Name };
            if (material == null) return false;

            return TryApplyMaterialParameters(context, material, data);
        }

        public bool TryApplyMaterialParameters(DidimoBuildContext context, Material material, MaterialData data)
        {
            if (data.Parameters != null)
            {
                foreach (MaterialDataParameter parameter in data.Parameters)
                {
                    parameter.ApplyToMaterial(context, material);
                }
            }

            if (RequiresMaterialModification(data.Name, out Action<Material> modificationAction))
            {
                modificationAction(material);
            }

            return true;
        }

        public bool TryFindMaterial(DidimoBuildContext context, string hierarchyName, out Material material)
        {
            material = null;
            if (!context.MeshHierarchyRoot.TryFindRecursive(hierarchyName, out Transform target))
            {
                return false;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer.sharedMaterial != null)
            {
                material = renderer.sharedMaterial;
            }

            return material != null;
        }

        public bool ApplyMaterialToHierarchy(DidimoBuildContext context, string hierarchyName, Material material)
        {
            Transform target = null;

            if (!context.MeshHierarchyRoot.TryFindRecursive(hierarchyName, out target))
            {
                Debug.LogWarning($"Can not find transform {hierarchyName} in hierarchy.");
                return false;
            }

            Renderer renderer = target.GetComponentInChildren<Renderer>();
            if (renderer == null) return false;

            renderer.sharedMaterial = material;
            return true;
        }

        public abstract bool NameToProperty(string name, out string propertyName);

        public abstract bool FindIdealShader(string shaderName, out Shader shader);

        public static void ProcessPropertyBlocksInHierarchy(
            Transform transform,
            Component callingComponent,
            Action<MaterialPropertyBlock, Component, Renderer, Material> onPropertyBlock)
        {
            foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                List<Material> mlist = new List<Material>();
                renderer.GetSharedMaterials(mlist);
                int idx = 0;
                foreach (var m in mlist)
                {
                    onPropertyBlock?.Invoke(propertyBlock, callingComponent, renderer, m);
                    renderer.SetPropertyBlock(propertyBlock, idx);
                    ++idx;
                }

            }
        }

        public static bool CreateBuilderForCurrentPipeline(out MaterialBuilder builder)
        {
            builder = null;

            var buildConfig = Resources.Load<DidimoBuildConfig>("DidimoBuildConfig");

            switch (buildConfig.Pipeline)
            {
                case SupportedRenderPipelines.Standard:
                    builder = new StandardPipelineMaterialBuilder();
                    return true;

                case SupportedRenderPipelines.UniversalRenderPipeline:
                    builder = new UniversalRenderingPipelineMaterialBuilder();
                    return true;

                case SupportedRenderPipelines.HighDefinitionRenderPipeline:
                    break;
            }

            Debug.LogWarning($"No valid material builder available " +
                $"for pipeline: {buildConfig.Pipeline}");

            return false;
        }

        protected virtual bool RequiresMaterialModification(string name,
            out Action<Material> modificationAction)
        {
            modificationAction = null;
            return false;
        }

        public static bool CopyMaterialProperty(Material source, int sourceIdx, Material target, int targetIdx)
        {
            try
            {

                var sourcePropType = source.shader.GetPropertyType(sourceIdx);
                var targetPropType = target.shader.GetPropertyType(targetIdx);
                var sourcePropName = source.shader.GetPropertyName(sourceIdx);
                var targetPropName = target.shader.GetPropertyName(targetIdx);
                if (sourcePropType == targetPropType)
                {
                    if (source.HasProperty(sourcePropName))
                    {
                        switch (sourcePropType)
                        {
                            case ShaderPropertyType.Color:
                                {
                                    Color v = source.GetColor(sourcePropName);
                                    target.SetColor(targetPropName, v);
                                    return true;
                                }
                            case ShaderPropertyType.Range:
                            case ShaderPropertyType.Float:
                                {
                                    float v = source.GetFloat(sourcePropName);
                                    target.SetFloat(targetPropName, v);
                                    return true;
                                }
                            case ShaderPropertyType.Texture:
                                {
                                    var v = source.GetTexture(sourcePropName);
                                    if (v == null)
                                        Debug.Log("Warning: setting empty texture to '" + sourcePropName + "' on '" + target.name + "'");

                                    target.SetTexture(targetPropName, v);
                                    return true;
                                }
                            case ShaderPropertyType.Vector:
                                {
                                    var v = source.GetVector(sourcePropName);
                                    target.SetVector(targetPropName, v);
                                    return true;
                                }
                        }
                    }
                    else
                    {
                        Debug.Log("Tried to find material propety '" + sourcePropName + "' on '" + target.name + " 'but failed.");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error copying material property: " + e.Message);
            }
            return false;
        }
        public static int findShaderPropertyContaining(Shader shader, string nameFragment, ShaderPropertyType propType)
        {
            for (var i = 0; i < shader.GetPropertyCount(); ++i)
            {
                var checkPropType = shader.GetPropertyType(i);
                if (checkPropType == propType)
                {
                    var propName = shader.GetPropertyName(i);
                    if (propName.ToLower().Contains(nameFragment))
                        return i;
                    propName = shader.GetPropertyDescription(i);
                    if (propName.ToLower().Contains(nameFragment))
                        return i;
                }
            }
            return -1;
        }
        public static void CopyMaterialProperties(Material Source, Material Target, Dictionary<string, string[]> variableMap = null)
        {
            var propCount = Target.shader.GetPropertyCount();
            for (var i = 0; i < propCount; ++i)
            {
                var propName = Target.shader.GetPropertyName(i);
                var propType = Target.shader.GetPropertyType(i);
                var sourcePropIdx = Source.shader.FindPropertyIndex(propName);
                if (sourcePropIdx == -1)
                {
                    if (variableMap != null && variableMap.ContainsKey(propName))
                    {
                        string[] potentialAliases = variableMap[propName];
                        foreach (var potentialAlias in potentialAliases)
                        {
                            if (potentialAlias.StartsWith("*"))
                                sourcePropIdx = findShaderPropertyContaining(Source.shader, potentialAlias.Substring(1).ToLower(), propType);
                            else
                                sourcePropIdx = Source.shader.FindPropertyIndex(propName);
                            if (sourcePropIdx != -1)
                                break;
                        }
                    }
                }
                if (sourcePropIdx != -1)
                {
                    CopyMaterialProperty(Source, sourcePropIdx, Target, i);
                }
            }
        }
    }
}