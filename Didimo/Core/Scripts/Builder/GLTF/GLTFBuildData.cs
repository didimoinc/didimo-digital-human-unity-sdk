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
            DidimoEyeShadowController eyeShadowController = didimoGO.AddComponent<DidimoEyeShadowController>();
            eyeShadowController.Build(importResult.eyeShadowController);
            DidimoIrisController didimoIrisController = didimoGO.AddComponent<DidimoIrisController>();
            didimoIrisController.Build(importResult.irisController);
            didimoGO.AddComponent<DidimoMaterials>();

            LegacyAnimationPoseController poseController = context.DidimoComponents.gameObject.AddComponent<LegacyAnimationPoseController>();
            poseController.BuildController(importResult.animationClips, importResult.resetAnimationClip, importResult.headJoint);

            OnAfterBuild(configuration, context);
            return Task.FromResult((true, context.DidimoComponents));
        }
    }
}