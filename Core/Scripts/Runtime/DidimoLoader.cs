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
        public static async Task<DidimoComponents> LoadDidimoFromFilePath(string didimoKey, string didimoFilePath, Configuration configuration = null)
        {
            string extension = Path.GetExtension(didimoFilePath);
            switch (extension.ToLower())
            {
                case ".json":
                    JSONBuildData jsonDidimoData = new JSONBuildData(didimoKey, didimoFilePath);
                    (bool jsonSuccess, DidimoComponents jsonDidimo) = await jsonDidimoData.Build(configuration ?? Configuration.Default());

                    if (!jsonSuccess)
                    {
                        throw new Exception("Failed to load didimo");
                    }

                    return jsonDidimo;

                case ".gltf":
                    GLTFBuildData gltfDidimoData = new GLTFBuildData(didimoKey, didimoFilePath);
                    (bool gltfSuccess, DidimoComponents gltfDidimo) = await gltfDidimoData.Build(configuration ?? Configuration.Default());
                    if (!gltfSuccess)
                    {
                        throw new Exception("Failed to load didimo");
                    }

                    return gltfDidimo;

                default:
                    throw new Exception($"Unsupported didimo type: '{extension}'");
            }
        }

        public static async Task<DidimoComponents> LoadDidimoInFolder(string didimoKey, string rootPath, Configuration configuration = null)
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