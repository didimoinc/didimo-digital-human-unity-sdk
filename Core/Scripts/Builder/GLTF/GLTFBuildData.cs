using System.Threading.Tasks;
using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo.Builder.GLTF
{
    /// <summary>
    /// <c>BuildData</c> object to build a didimo from a GLTF package.
    /// </summary>
    public class GLTFBuildData : BuildData
    {
        public const string DEFAULT_DIDIMO_FILE_NAME = "avatar.gltf";

        public readonly ImportSettings ImportSettings;

        public string GLTFDidimoFilePath { get; }

        /// <summary>
        /// Create a BuildDidimo from a path to a didimo file.
        /// </summary>
        /// <param name="didimoKey">Key of this didimo. Can be an empty string.</param>
        /// <param name="gltfDidimoFilePath">Path to the GLTF file of the didimo package.</param>
        public GLTFBuildData(string didimoKey, string gltfDidimoFilePath) : this(didimoKey,
            gltfDidimoFilePath,
            new ImportSettings {animationSettings = new AnimationSettings {useLegacyClips = true}})
        {
        }

        /// <summary>
        /// Create a BuildDidimo from a path to a didimo file with specific import settings.
        /// </summary>
        /// <param name="didimoKey">Key of this didimo. Can be an empty string.</param>
        /// <param name="gltfDidimoFilePath">Path to the GLTF file of the didimo package.</param>
        /// <param name="gltfImportSettings">Import settings for the didimo.</param>
        public GLTFBuildData(
            string didimoKey, string gltfDidimoFilePath,
            ImportSettings gltfImportSettings): base(didimoKey, gltfDidimoFilePath)
        {
            GLTFDidimoFilePath = gltfDidimoFilePath;
            ImportSettings = gltfImportSettings;
        }

        /// <summary>
        /// Build the didimo GameObject with the adequate materials and add the required components.
        /// </summary>
        /// <param name="configuration"><c>Configuration</c> settings for the build.</param>
        /// <returns>Tuple containing a bool for the task success and the created <c>DidimoComponents</c> component.
        /// If the task fails the component is null instead.</returns>
        public override Task<(bool success, DidimoComponents didimo)> Build(Configuration configuration)
        {
            OnBeforeBuild(configuration, out DidimoBuildContext context);

            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);
            ImportSettings.shaderForName = shaderName =>
            {
                materialBuilder.FindIdealShader(shaderName, out Shader shader);
                return shader;
            };

            Importer.ImportResult importResult
                = Importer.LoadFromFile(GLTFDidimoFilePath, ImportSettings, Format.GLTF);
            GameObject didimoGO = importResult.rootObject;
            didimoGO.transform.SetParent(context.RootTransform);
            context.MeshHierarchyRoot = didimoGO.transform;

            AddRequiredComponents(importResult, context.DidimoComponents.gameObject);

            OnAfterBuild(configuration, context);
            return Task.FromResult((true, context.DidimoComponents));
        }

        /// <summary>
        /// Build a didimo from a Unity ScriptedImporter.
        /// This is used to add the required components to the scripted importer
        /// that creates the GameObject when dragging didimos into the project.
        /// </summary>
        /// <param name="gltfImportResult">Import result returned by the GLTFImporter</param>
        /// <returns>The created <c>DidimoComponents</c> component.</returns>
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

        /// <summary>
        /// Add the essential components to the didimo. This includes the
        /// animator, pose controller, iris controller and eye shadow controller.
        /// </summary>
        /// <param name="gltfImportResult"></param>
        /// <param name="root"></param>
        public static void AddRequiredComponents(Importer.ImportResult gltfImportResult, GameObject root)
        {
            root.AddComponent<DidimoAnimator>();
#if INSTANCING_SUPPORT
            if (gltfImportResult.FaceMesh != null)
            {
                DidimoInstancingHelper dih = root.GetComponent<DidimoInstancingHelper>();
                if (!dih)
                    dih = root.AddComponent<DidimoInstancingHelper>();
                dih.Build(gltfImportResult.FaceMesh);
            }
#endif
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
            poseController.BuildController(
                gltfImportResult.animationClips, gltfImportResult.resetAnimationClip, gltfImportResult.headJoint);
        }
    }
}