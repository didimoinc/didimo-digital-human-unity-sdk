using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Didimo.Core.Utility;
using Didimo.GLTFUtility;
using UnityEngine;
using UnityEngine.Rendering;

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
        public GLTFBuildData(string didimoKey, string gltfDidimoFilePath, ImportSettings gltfImportSettings) : base(
            didimoKey, gltfDidimoFilePath)
        {
            GLTFDidimoFilePath = gltfDidimoFilePath;
            ImportSettings = gltfImportSettings;
        }

        public static void WaitCoroutine(IEnumerator func) {
            while (func.MoveNext ()) {
                if (func.Current != null) {
                    IEnumerator num;
                    try {
                        num = (IEnumerator)func.Current;
                    } catch (InvalidCastException) {
                        if (func.Current.GetType () == typeof(WaitForSeconds))
                            Debug.LogWarning ("Skipped call to WaitForSeconds. Use WaitForSecondsRealtime instead.");
                        return;  // Skip WaitForSeconds, WaitForEndOfFrame and WaitForFixedUpdate
                    }
                    WaitCoroutine (num);
                }
            }
        }
        
        /// <summary>
        /// Build the didimo GameObject with the adequate materials and add the required components.
        /// </summary>
        /// <param name="configuration"><c>Configuration</c> settings for the build.</param>
        /// <returns>Tuple containing a bool for the task success and the created <c>DidimoComponents</c> component.
        /// If the task fails the component is null instead.</returns>
        public override Task<(bool success, DidimoComponents didimo)> Build(Configuration configuration)
        {
            // OnBeforeBuild(configuration, out DidimoBuildContext context);

            MaterialBuilder.CreateBuilderForCurrentPipeline(out MaterialBuilder materialBuilder);
            ImportSettings.shaderForName = shaderName =>
            {
                materialBuilder.FindIdealShader(shaderName, out Shader shader);
                return shader;
            };

            ImportSettings.postMaterialCreate = material => materialBuilder.PostMaterialCreate(material);
            DidimoImporterJsonConfig didimoImporterJsonConfig =
                DidimoImporterJsonConfigUtils.GetConfigAtFolder(Path.GetDirectoryName(GLTFDidimoFilePath)!);

            Importer.ImportResult importResult = Importer.LoadFromFile(GLTFDidimoFilePath, ImportSettings, gltfObject =>
            {
                if (didimoImporterJsonConfig != null)
                {
                    // We will be settings the materials na importing the textures after importing the gltf
                    gltfObject.images = null;
                    gltfObject.textures = null;
                    gltfObject.materials = null;
                    gltfObject.extensions ??= new GLTFObject.Extensions();
                    gltfObject.extensions.Didimo = new DidimoExtension();
                }
            }, Format.GLTF);
            ImportSettings.animationType = configuration.AnimationType;
            ImportSettings.avatar = configuration.Avatar;
            ImportSettings.avatarDefinition = configuration.Avatar
                ? ImportSettings.AvatarDefinition.CopyFromAnotherAvatar
                : ImportSettings.AvatarDefinition.CreateFromThisModel;
            GameObject didimoGO = importResult.rootObject;
            didimoGO.transform.SetParent(configuration.Parent);
            // context.MeshHierarchyRoot = didimoGO.transform;

            // context.DidimoComponents.didimoVersion = importResult.didimoVersion;

            DidimoComponents didimoComponents = ComponentUtility.GetOrAdd<DidimoComponents>(didimoGO);
            didimoComponents.DidimoKey = DidimoKey;
            if (didimoImporterJsonConfig == null)
            {
                didimoComponents.gameObject.AddComponent<DidimoParts>();
                didimoComponents.Parts.SetupForDidimoVersion(importResult.didimoVersion);
                AddRequiredComponents(importResult, didimoComponents.gameObject, ImportSettings,
                    didimoComponents.Parts.HeadJoint, didimoComponents);

            }
            else
            {
                WaitCoroutine(DidimoImporterJsonConfigUtils.SetupDidimoForRuntime(didimoComponents.gameObject, didimoImporterJsonConfig, GLTFDidimoFilePath,
                    importResult.animationClips, importResult.resetAnimationClip, null));
                
                SetAvatar(ImportSettings, didimoComponents.gameObject);
            }


            DidimoBuildContext didimoBuildContext = DidimoBuildContext.CreateNew(didimoComponents, RootDirectory);
            OnAfterBuild(configuration, didimoBuildContext);

            return Task.FromResult((true, didimoComponents));
        }

        /// <summary>
        /// Build a didimo from a Unity ScriptedImporter.
        /// This is used to add the required components to the scripted importer
        /// that creates the GameObject when dragging didimos into the project.
        /// </summary>
        /// <param name="gltfImportResult">Import result returned by the GLTFImporter</param>
        /// <param name="importSettings">Settings to configure how some parts of the GLTF should be imported</param>
        /// <param name="assetPath">Path of the GLTF file</param>
        /// <returns>The created <c>DidimoComponents</c> component.</returns>
        public static DidimoComponents BuildFromScriptedImporter(Importer.ImportResult gltfImportResult,
            ImportSettings importSettings, string assetPath = "")
        {
            string rootDirectory = Path.GetDirectoryName(assetPath) ?? "";

            GLTFBuildData buildData = new GLTFBuildData(string.Empty, rootDirectory);
            DidimoImporterJsonConfig importerJsonConfig = DidimoImporterJsonConfigUtils.GetConfigAtFolder(rootDirectory);
            Configuration configuration = Configuration.Default();

            if (!gltfImportResult.isDidimo && importerJsonConfig == null) return null;


            gltfImportResult.isDidimo = true;
            // buildData.OnBeforeBuild(configuration, out DidimoBuildContext context);
            DidimoComponents didimoComponents = gltfImportResult.rootObject.AddComponent<DidimoComponents>();

            if (importerJsonConfig != null)
            {
                DidimoImporterJsonConfigUtils.BuildDidimoParts(didimoComponents, importerJsonConfig);
            }
            else
            {
                gltfImportResult.rootObject.AddComponent<DidimoParts>()
                    .SetupForDidimoVersion(gltfImportResult.didimoVersion);
            }

            didimoComponents.didimoVersion = gltfImportResult.didimoVersion;
            DidimoBuildContext context = DidimoBuildContext.CreateNew(didimoComponents, rootDirectory);
            AddRequiredComponents(gltfImportResult, gltfImportResult.rootObject, importSettings,
                didimoComponents.Parts.HeadJoint, didimoComponents);

            GLTFDidimoHair.ApplyHairMaterials(gltfImportResult);

            buildData.OnAfterBuild(configuration, context);

            return context.DidimoComponents;
        }

        /// <summary>
        /// Add the essential components to the didimo. This includes the
        /// animator, pose controller, iris controller and eye shadow controller.
        /// </summary>
        /// <param name="gltfImportResult">Import result returned by the GLTFImporter</param>
        /// <param name="root">Root object where required components will be attached to</param>
        /// <param name="importSettings">Settings to configure how some parts of the GLTF should be imported</param>
        /// <param name="didimoComponents">The DidimoComponents component of the didimo</param>
        public static void AddRequiredComponents(Importer.ImportResult gltfImportResult, GameObject root,
            ImportSettings importSettings, Transform headJoint, DidimoComponents didimoComponents)
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
            root.AddComponent<DidimoEyeShadowController>();

            root.AddComponent<DidimoIrisController>();

            LegacyAnimationPoseController poseController = root.AddComponent<LegacyAnimationPoseController>();
            poseController.BuildController(gltfImportResult.animationClips, gltfImportResult.resetAnimationClip,
                headJoint);

            didimoComponents.Parts.LeftEyeMeshRenderer.updateWhenOffscreen = true;
            didimoComponents.Parts.RightEyeMeshRenderer.updateWhenOffscreen = true;
            if (didimoComponents.Parts.EyeLashesMeshRenderer != null)
            {
                didimoComponents.Parts.EyeLashesMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            
            SetAvatar(importSettings, root);
        }

        public static void SetAvatar(ImportSettings importSettings, GameObject root)
        {
            if ((importSettings.animationType == ImportSettings.AnimationType.Generic ||
                 importSettings.animationType == ImportSettings.AnimationType.Humanoid) &&
                importSettings.avatarDefinition != ImportSettings.AvatarDefinition.NoAvatar)
            {
                Animator animator = ComponentUtility.GetOrAdd<Animator>(root);

                if (importSettings.avatarDefinition == ImportSettings.AvatarDefinition.CreateFromThisModel)
                {
                    if (importSettings.animationType == ImportSettings.AnimationType.Generic)
                    {
                        importSettings.avatar = AvatarBuilder.BuildGenericAvatar(root, "");
                    }
                    else if (importSettings.animationType == ImportSettings.AnimationType.Humanoid)
                    {
                        BuildHumanoidAvatar(root, importSettings);
                    }
                }

                animator.avatar = importSettings.avatar;
            }
        }

        private static void BuildHumanoidAvatar(GameObject root, ImportSettings importSettings)
        {
            HumanDescription humanDescription = new HumanDescription();
            Transform[] transforms = root.GetComponentsInChildren<Transform>();
            List<SkeletonBone> skeletonBones = new List<SkeletonBone>(transforms.Length);

            Avatar defaultAvatar = ResourcesLoader.DidimoDefaultAvatar();

            for (int i = 0; i < transforms.Length; i++)
            {
                SkeletonBone skeletonBone = new SkeletonBone();
                skeletonBone.name = transforms[i].name;
                try
                {
                    skeletonBone.rotation = defaultAvatar.humanDescription.skeleton
                        .First(s => s.name == skeletonBone.name).rotation;
                }
                catch (Exception)
                {
                    // Debug.LogWarning(e);
                    // Skip this bone
                    continue;
                }

                skeletonBone.position = transforms[i].localPosition;
                //skeletonBone[i].rotation = transforms[i].localRotation;
                skeletonBone.scale = transforms[i].localScale;
                skeletonBones.Add(skeletonBone);
            }

            humanDescription.skeleton = skeletonBones.ToArray();
            //humanDescription.skeleton = defaultAvatar.humanDescription.skeleton.Clone() as SkeletonBone[];
            humanDescription.human = defaultAvatar.humanDescription.human;

            importSettings.avatar = AvatarBuilder.BuildHumanAvatar(root, humanDescription);
        }
    }
}