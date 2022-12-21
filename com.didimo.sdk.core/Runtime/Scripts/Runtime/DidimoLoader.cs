using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Didimo.Builder;
using GLTFast;
using UnityEngine;

namespace Didimo
{
    public class DidimoLoader
    {
        public const string DEFAULT_DIDIMO_FILE_NAME = "avatar.gltf";

        private static void RunCoroutineSynchronously(IEnumerator coroutine)
        {
            while (coroutine.MoveNext())
            {
                IEnumerator subCoroutine = coroutine.Current as IEnumerator;
                RunCoroutineSynchronously(subCoroutine);
            }
        }

        /// <summary>
        /// Load a didimo from a file path. This file must be either the
        /// GLTF file or the JSON file depending on the didimo format.
        /// </summary>
        /// <param name="didimoKey">Key from your didimo retrieved from the API. Can be an empty string.</param>
        /// <param name="didimoFilePath">Path to the file of the didimo to load.</param>
        /// <param name="configuration">Additional configuration data to use when loading.
        /// Contains information such as object to parent. Optional</param>
        /// <returns>DidimoComponents component of the loaded didimo game object.</returns>
        /// <exception cref="Exception">Unable to load the didimo for any reason.</exception>
        public static async Task<DidimoComponents> LoadDidimoFromFilePath(
            string didimoKey, string didimoFilePath, Configuration configuration = null)
        {
            string extension = Path.GetExtension(didimoFilePath);
            switch (extension.ToLower())
            {

                case ".gltf":
                case ".glb":

                    GltfImport gltfDidimo = new GltfImport();
                    
                    // Create a settings object and configure it accordingly
                    var settings = new ImportSettings {
                        generateMipMaps = true,
                        anisotropicFilterLevel = 3,
                        nodeNameMethod = ImportSettings.NameImportMethod.OriginalUnique
                    };
                    
                    bool gltfSuccess = await gltfDidimo.Load(didimoFilePath, settings);

                    if (!gltfSuccess)
                    {
                        throw new Exception("Failed to load didimo");
                    }
                    
                    GameObject didimoGO = new GameObject($"didimo_{didimoKey}");
                    await gltfDidimo.InstantiateMainSceneAsync(didimoGO.transform);
                    
                    DidimoImporterJsonConfig didimoImporterJsonConfig =
                        DidimoImporterJsonConfigUtils.GetConfigAtFolder(Path.GetDirectoryName(didimoFilePath)!);
                    if (didimoImporterJsonConfig == null)
                    {
                        throw new Exception($@"Couldn't find file required to import didimo: {DidimoImporterJsonConfigUtils.GetConfigFilePathForFolder(Path.GetDirectoryName(didimoFilePath)!)}.
                                            Note that older didimo packages don't contain this file, and support for those packages has been discontinued. Either generate a new didimo, or use an older version of the SDK.$\n");
                    }

                    IEnumerator jsonSetup = DidimoImporterJsonConfigUtils.SetupDidimoForRuntime(didimoGO, didimoImporterJsonConfig,
                        didimoFilePath, gltfDidimo.GetAnimationClips(), gltfDidimo.GetResetClip(), didimoKey);

                    RunCoroutineSynchronously(jsonSetup);
                    
                    return didimoGO.GetComponent<DidimoComponents>();

                default:
                    throw new Exception($"Unsupported didimo type: '{extension}'");
            }
        }

        /// <summary>
        /// Load a didimo from a folder path. This folder must contain the
        /// didimo avatar.gltf file or the avatar_model.json file depending on
        /// the didimo format.
        /// </summary>
        /// <param name="didimoKey">Key from your didimo retrieved from the API. Can be an empty string.</param>
        /// <param name="rootPath">Path to the root folder of the didimo to load.</param>
        /// <param name="configuration">Additional configuration data to use when loading.
        /// Contains information such as object to parent. Optional</param>
        /// <returns>DidimoComponents component of the loaded didimo game object.</returns>
        /// <exception cref="Exception">Unable to load the didimo for any reason.</exception>
        public static async Task<DidimoComponents> LoadDidimoInFolder(
            string didimoKey, string rootPath, Configuration configuration = null)
        {

            string didimoFilePath = Path.Combine(rootPath, DEFAULT_DIDIMO_FILE_NAME);
            if (File.Exists(didimoFilePath))
            {
                return await LoadDidimoFromFilePath(didimoKey, didimoFilePath, configuration);
            }

            throw new Exception($"Could not find didimo file at {rootPath}");
        }
    }
}