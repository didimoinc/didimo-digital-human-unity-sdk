using System.Threading.Tasks;
using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo.Builder.GLTF
{
    public class GLTFBuildData : BuildData
    {
        public const string DEFAULT_DIDIMO_FILE_NAME = "avatar.gltf";

        public readonly ImportSettings ImportSettings;

        public string GLTFDidimoFilePath { get; }

        public GLTFBuildData(string didimoKey, string gltfDidimoFilePath) : this(didimoKey,
            gltfDidimoFilePath,
            new ImportSettings {animationSettings = new AnimationSettings {useLegacyClips = true}})
        {
        }

        public GLTFBuildData(string didimoKey, string gltfDidimoFilePath, ImportSettings gltfImportSettings) : base(didimoKey, gltfDidimoFilePath)
        {
            GLTFDidimoFilePath = gltfDidimoFilePath;
            ImportSettings = gltfImportSettings;
        }

        public override Task<(bool success, DidimoComponents didimo)> Build(Configuration configuration)
        {
            OnBeforeBuild(configuration, out DidimoBuildContext context);

            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);
            ImportSettings.shaderForName = shaderName =>
            {
                materialBuilder.FindIdealShader(shaderName, out Shader shader);
                return shader;
            };

            Importer.ImportResult importResult = Importer.LoadFromFile(GLTFDidimoFilePath, ImportSettings, Format.GLTF);
            GameObject didimoGO = importResult.rootObject;
            didimoGO.transform.SetParent(context.RootTransform);
            context.MeshHierarchyRoot = didimoGO.transform;

            AddRequiredComponents(importResult, context.DidimoComponents.gameObject);

            OnAfterBuild(configuration, context);
            return Task.FromResult((true, context.DidimoComponents));
        }

        public static DidimoComponents BuildFromScriptedImporter(Importer.ImportResult gltfImportResult)
        {
            GLTFBuildData buildData = new GLTFBuildData(string.Empty, string.Empty);
            Configuration configuration = Configuration.Default();
            
            // buildData.OnBeforeBuild(configuration, out DidimoBuildContext context);
            DidimoComponents didimoComponents = gltfImportResult.rootObject.AddComponent<DidimoComponents>();
            DidimoBuildContext context = DidimoBuildContext.CreateNew(didimoComponents, string.Empty);
            AddRequiredComponents(gltfImportResult, gltfImportResult.rootObject);
            buildData.OnAfterBuild(configuration, context);

            return context.DidimoComponents;
        }


        public static void AddRequiredComponents(Importer.ImportResult gltfImportResult, GameObject root)
        {
            root.AddComponent<DidimoAnimator>();
            
            if (gltfImportResult.eyeShadowController != null)
            {
                DidimoEyeShadowController eyeShadowController = root.AddComponent<DidimoEyeShadowController>();
                eyeShadowController.Build(gltfImportResult.eyeShadowController);
            }

            if (gltfImportResult.irisController != null)
            {
                DidimoIrisController didimoIrisController = root.AddComponent<DidimoIrisController>();
                didimoIrisController.Build(gltfImportResult.irisController);
            }
            
            LegacyAnimationPoseController poseController = root.AddComponent<LegacyAnimationPoseController>();
            poseController.BuildController(gltfImportResult.animationClips, gltfImportResult.resetAnimationClip, gltfImportResult.headJoint);
        }
    }
}