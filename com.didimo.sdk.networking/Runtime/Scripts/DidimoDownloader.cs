using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public static class DidimoDownloader
    {
        public static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                if (file.Name == "")
                { // Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }

                file.ExtractToFile(completeFileName, true);
            }
        }

        public static async Task<(bool success, string path)> DownloadAndUnpack(string didimoKey, string uri)
        {
            (bool success, string path) = await DownloadToDisk(didimoKey, uri);

            if (!success) { return (false, null); }

            return await UnpackAndDelete(path, CreateFolderForDownload(didimoKey));
        }

        public static async Task<(bool success, byte[] data)> Download(string uri)
        {
            try
            {
                using UnityWebRequest request = UnityWebRequest.Get(new Uri(uri));
                await request.SendWebRequest();

                if (request.error != null)
                {
                    Debug.LogError(request.error);
                    return (false, null);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Unsuccessful download.");
                    return (false, null);
                }

                return (true, request.downloadHandler.data);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download due to error: {e}");
                return (false, null);
            }
        }

        public static async Task<(bool success, string path)> DownloadToDisk(string didimoKey, string uri)
        {
            try
            {
                string didimoDirectory = CreateFolderForDownload(didimoKey);
                if (string.IsNullOrEmpty(uri)) return (false, null);

                string filePath = Path.Combine(didimoDirectory, $"{didimoKey}.zip");

                using UnityWebRequest request = UnityWebRequest.Get(uri);
                request.downloadHandler = new DownloadHandlerFile(filePath);
                await request.SendWebRequest();

                if (request.error != null)
                {
                    Debug.LogWarning(request.error);
                    return (false, null);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("Unsuccessful download.");
                    return (false, null);
                }

                return (true, filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return (false, null);
            }
        }

        public static async Task<(bool success, string path)> UnpackAndDelete(string zipFilePath, string destinationFolder)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using (FileStream zipFileStream = new FileStream(zipFilePath, FileMode.Open))
                    using (ZipArchive archive = new ZipArchive(zipFileStream, ZipArchiveMode.Update))
                    {
                        ExtractToDirectory(archive, destinationFolder, true);
                    }

                    File.Delete(zipFilePath);

                    return (true, destinationFolder);
                });
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return (false, null);
            }
        }

        public static string CreateFolderForDownload(string didimoKey)
        {
            DirectoryInfo rootDir = new DirectoryInfo(DidimoNetworkingResources.NetworkConfig.DownloadRoot);
            string fullPath = Path.Combine(rootDir.FullName, didimoKey);

            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            return rootDir.CreateSubdirectory(didimoKey).FullName;
        }
    }
}