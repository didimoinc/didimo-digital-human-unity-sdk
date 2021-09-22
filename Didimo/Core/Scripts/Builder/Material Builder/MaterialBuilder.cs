using System;
using System.Threading.Tasks;
using UnityEngine;

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
                Debug.LogWarning($"Failed to load Material Instance. Could not locate appropriate shader ({data.ShaderName})");
                material = null;
                return false;
            }

            material = new Material(shader) {name = data.Name};
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

        public static void ProcessPropertyBlocksInHierarchy(Transform transform, Action<MaterialPropertyBlock> onPropertyBlock)
        {
            foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propertyBlock);
                onPropertyBlock?.Invoke(propertyBlock);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        public static bool CreateBuilderForCurrentPipeline(out MaterialBuilder builder)
        {
            builder = null;
            switch (DidimoConfig.Pipeline)
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

            Debug.LogWarning($"No valid material builder available for pipeline: {DidimoConfig.Pipeline}");
            return false;
        }

        protected virtual bool RequiresMaterialModification(string name, out Action<Material> modificationAction)
        {
            modificationAction = null;
            return false;
        }
    }
}