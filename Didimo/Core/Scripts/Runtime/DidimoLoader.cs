using System;
using System.IO;
using System.Threading.Tasks;
using Didimo.Builder;
using Didimo.Builder.GLTF;
using Didimo.Builder.JSON;

namespace Didimo
{
    public class DidimoLoader
    {
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
                case ".json":
                    JSONBuildData jsonDidimoData = new JSONBuildData(didimoKey, didimoFilePath);
                    (bool jsonSuccess, DidimoComponents jsonDidimo)
                        = await jsonDidimoData.Build(configuration ?? Configuration.Default());

                    if (!jsonSuccess)
                    {
                        throw new Exception("Failed to load didimo");
                    }

                    return jsonDidimo;

                case ".gltf":
                    GLTFBuildData gltfDidimoData = new GLTFBuildData(didimoKey, didimoFilePath);
                    (bool gltfSuccess, DidimoComponents gltfDidimo)
                        = await gltfDidimoData.Build(configuration ?? Configuration.Default());
                    if (!gltfSuccess)
                    {
                        throw new Exception("Failed to load didimo");
                    }

                    return gltfDidimo;

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
            string didimoFilePath = Path.Combine(rootPath, JSONBuildData.DEFAULT_DIDIMO_FILE_NAME);
            if (File.Exists(didimoFilePath))
            {
                return await LoadDidimoFromFilePath(didimoKey, didimoFilePath, configuration);
            }

            didimoFilePath = Path.Combine(rootPath, GLTFBuildData.DEFAULT_DIDIMO_FILE_NAME);
            if (File.Exists(didimoFilePath))
            {
                return await LoadDidimoFromFilePath(didimoKey, didimoFilePath, configuration);
            }

            throw new Exception($"Could not find didimo file at {rootPath}");
        }
    }
}