using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Didimo.Builder;
using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    [JsonConverter(typeof(JsonSubtypes))]
    [JsonSubtypes.FallBackSubTypeAttribute(typeof(MaterialDataContainer))]
    [JsonSubtypes.KnownSubTypeWithPropertyAttribute(typeof(HairMaterialDataContainer), "hairColor")]
    public class MaterialDataContainer
    {
        [JsonProperty("materials")]
        protected List<MaterialData> materialData;

        [JsonProperty("geoMap")]
        protected Dictionary<string, List<string>> materialToHierarchyNodes;

        [JsonIgnore] public IReadOnlyList<MaterialData> MaterialData => materialData;

        [JsonIgnore] public IReadOnlyDictionary<string, List<string>> MaterialToHierarchyNodes => materialToHierarchyNodes; // Shallow immutable.

        [JsonIgnore] public IReadOnlyList<string> AllHierarchyNodes { get; private set; }

        [JsonIgnore] public IReadOnlyDictionary<string, MaterialData> HierarchyNodeToMaterialData { get; private set; }

        public virtual void ApplyToHierarchy(DidimoBuildContext context, MaterialBuilder builder)
        {
            foreach (string hierarchyName in AllHierarchyNodes)
            {
                if (!TryFindDataForHierarchyNode(hierarchyName, out MaterialData data))
                {
                    Debug.LogWarning($"Could not find MaterialData for {hierarchyName}.");
                    continue;
                }

                // First, try to find an existing material on the hierarchy node.
                if (builder.TryFindMaterial(context, hierarchyName, out Material material))
                {
                }
                else
                {
                    // If we don't find an existing, create a new one from the material data.
                    if (!builder.TryBuildMaterial(context, data, out material))
                    {
                        Debug.LogWarning($"Could not build MaterialData for {hierarchyName}.");
                        continue;
                    }

                    if (!builder.ApplyMaterialToHierarchy(context, hierarchyName, material))
                    {
                        Debug.LogWarning($"Could not apply newly built material to {hierarchyName}.");
                        continue;
                    }
                }

                builder.TryApplyMaterialParameters(context, material, data);
            }
        }

        public virtual void ApplyToTarget(DidimoBuildContext context, MaterialBuilder builder, Transform target)
        {
            List<Material> materials = new List<Material>();
            for (int materialIndex = 0; materialIndex < MaterialData.Count; materialIndex++)
            {
                MaterialData data = MaterialData[materialIndex];

                // If we don't find an existing, create a new one from the material data.
                if (!builder.TryBuildMaterial(context, data, out Material material))
                {
                    Debug.LogWarning($"Could not build MaterialData for {data.Name}.");
                    continue;
                }

                builder.TryApplyMaterialParameters(context, material, data);

                materials.Add(material);
            }

            Renderer targetRenderer = target.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.sharedMaterials = materials.ToArray();
            }
        }

        public async Task Prepare(DidimoBuildContext context, MaterialBuilder builder)
        {
            foreach (MaterialData data in MaterialData)
            {
                await PrepareData(context, builder, data);
            }
        }

        public bool TryFindDataForHierarchyNode(string hierarchyName, out MaterialData data) => HierarchyNodeToMaterialData.TryGetValue(hierarchyName, out data);

        public bool TryFindData(string materialName, out MaterialData data)
        {
            data = materialData.FirstOrDefault(m => m.Name == materialName);
            return data != null;
        }

        public static MaterialDataContainer CreateNew(List<MaterialData> materialData, Dictionary<string, List<string>> materialToHierarchyNodes) => new MaterialDataContainer {materialData = materialData, materialToHierarchyNodes = materialToHierarchyNodes};

        public static async Task PrepareParameter(DidimoBuildContext context, MaterialBuilder builder, MaterialDataParameter parameter)
        {
            if (builder.NameToProperty(parameter.Name, out string propertyName))
            {
                parameter.Property = propertyName;
            }

            // Load textures from cache.
            if (parameter is TextureMaterialDataParameter textureParameter)
            {
                string name = Path.GetFileName(textureParameter.Value);

                Task<(bool success, Texture2D texture)> localLoadTask = context.DidimoComponents.TextureCache.TryGetOrLoad(context, name);
                await localLoadTask;
                if (localLoadTask.Result.success)
                {
                    textureParameter.Texture = localLoadTask.Result.texture;
                    return;
                }

                Debug.LogWarning($"Failed to locate/load texture in any cache ({name})");
            }
        }

        protected async Task PrepareData(DidimoBuildContext context, MaterialBuilder builder, MaterialData data)
        {
            foreach (MaterialDataParameter parameter in data.Parameters)
            {
                await PrepareParameter(context, builder, parameter);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            AllHierarchyNodes = MaterialToHierarchyNodes.Values.SelectMany(l => l).Distinct().ToList();

            Dictionary<string, MaterialData> hierarchyNodeToMaterialData = new Dictionary<string, MaterialData>();
            foreach (KeyValuePair<string, List<string>> kvp in MaterialToHierarchyNodes)
            {
                foreach (string nodeName in kvp.Value)
                {
                    if (!TryFindData(kvp.Key, out MaterialData data)) continue;
                    hierarchyNodeToMaterialData[nodeName] = data;
                }
            }

            HierarchyNodeToMaterialData = hierarchyNodeToMaterialData;
        }
    }
}