using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo.Builder.JSON
{
    public class JSONBuildData : BuildData
    {
        public const     string DEFAULT_DIDIMO_FILE_NAME = "avatar_model.json";
        private readonly string rootDir;

        public string DidimoFilePath { get; }

        public JSONBuildData(string didimoKey, string didimoFilePath) : base(didimoKey, didimoFilePath)
        {
            DidimoFilePath = Path.Combine(didimoFilePath);
            rootDir = didimoFilePath;
        }


        public override async Task<(bool success, DidimoComponents didimo)> Build(Configuration configuration)
        {
            OnBeforeBuild(configuration, out DidimoBuildContext context);

            (bool meshSuccess, string meshJson) = await JsonLoader.TryLoadFromPathAsync(DidimoFilePath);
            if (!meshSuccess)
            {
                Debug.LogWarning($"Failed to load model at path: {DidimoFilePath}");
                return (false, null);
            }

            DidimoModelDataObject didimoData = JsonConvert.DeserializeObject<DidimoModelDataObject>(meshJson);

            // (bool metadataSuccess, string metadataJson) = await JsonLoader.TryLoadFromPathAsync(MetaDataPath);
            // if (!metadataSuccess) {
            //     Debug.LogWarning($"Failed to load metadata at path: {MetaDataPath}");
            //     return (false, null);
            // }

            context.UnitsPerMeter = didimoData.unitsPerMeter;

            HierarchyBuilder hierarchyBuilder = new HierarchyBuilder();
            if (!hierarchyBuilder.TryBuild(context, didimoData))
            {
                Debug.LogWarning("Failed to build hierarchy");
                return (false, null);
            }

            MeshBuilder meshBuilder = new MeshBuilder();
            if (!meshBuilder.TryBuild(context, didimoData))
            {
                Debug.LogWarning("Failed to build mesh");
                return (false, null);
            }

            BoneWeightBuilder boneWeightBuilder = new BoneWeightBuilder();
            if (!boneWeightBuilder.TryBuild(context, didimoData))
            {
                Debug.LogWarning("Failed to build bones");
                return (false, null);
            }

            ConstraintBuilder constraintBuilder = new ConstraintBuilder();
            if (!constraintBuilder.TryBuild(context, didimoData))
            {
                Debug.LogWarning("Failed to build constraints");
                return (false, null);
            }

            // Find the appropriate material builder and build/apply materials for this Didimo.
            if (MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder))
            {
                if (!await materialBuilder.TryBuild(context, didimoData.MaterialDataContainer))
                {
                    Debug.LogWarning("Failed to build materials");
                    return (false, null);
                }
            }

            OnAfterBuild(configuration, context);
            return (true, context.DidimoComponents);
        }
    }
}