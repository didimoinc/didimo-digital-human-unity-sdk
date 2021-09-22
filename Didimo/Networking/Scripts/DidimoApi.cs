using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Didimo.Builder;
using UnityEngine;

namespace Didimo.Networking
{
    public class DidimoApi : IApi
    {
        // Wait this amount of milliseconds between calls to checking didimo creation progress
        private const int CHECK_STATUS_WAIT_TIME = 2500;

        public async Task<(bool success, AccountStatusResponse status)> AccountStatus()
        {
            AccountStatusQuery didimoDetailsQuery = new AccountStatusQuery();
            AccountStatusResponse result = await didimoDetailsQuery.ExecuteQuery();

            return (result != null, result);
        }

        public async Task<(bool success, byte[] data)> Deform(string didimoKey, byte[] data, Action<float> statusProgress = null)
        {
            Task<(bool success, DidimoDetailsResponse status)> didimoStatus = CheckDidimoStatus(didimoKey);
            await didimoStatus;
            if (!didimoStatus.Result.success)
            {
                Debug.LogWarning($"Failed to get status from didimo {didimoKey}.");
                return (false, null);
            }

            Downloadable downloadable = didimoStatus.Result.status.GetDownloadableArtifact(DidimoDetailsResponse.DownloadArtifactType.Deformer_dmx);
            if (downloadable == null)
            {
                Debug.LogError($"Failed to get downloadable of type {DidimoDetailsResponse.DownloadArtifactType.Deformer_dmx}");
                return (false, null);
            }

            (bool success, byte[] bytes) downloadArtifact = await downloadable.Download();
            if (!downloadArtifact.success)
            {
                Debug.LogError("Failed to download deformation matrix");
                return (false, null);
            }

            DeformQuery deformQuery = new DeformQuery(didimoKey, downloadArtifact.bytes, data);
            DeformResponse deformResponse = await deformQuery.ExecuteQuery();

            string deformID = deformResponse.DeformedID;
            (bool success, DidimoDetailsResponse status, string errorCode) checkForStatusUntilCompletion =
                await CheckForStatusUntilCompletion(deformID, statusProgress, IApi.StatusType.Deform);

            if (!checkForStatusUntilCompletion.success) return (false, null);

            downloadable = checkForStatusUntilCompletion.status.GetDownloadableForTransferFormat(DidimoDetailsResponse.DownloadTransferFormatType.Package);
            if (downloadable == null)
            {
                Debug.LogError($"Failed to get downloadable of type {DidimoDetailsResponse.DownloadTransferFormatType.Package}");
                return (false, null);
            }

            (bool success, string path) = await downloadable.DownloadToDisk(true);

            if (!success)
            {
                Debug.LogWarning($"Failed to download deformed data.");
                return (false, null);
            }

            string downloadPath = path + "/" + DeformQuery.DeformedAssetName;
            (success, data) = await DidimoDownloader.Download(downloadPath);

            if (!success)
            {
                Debug.LogError($"Failed to load deformed asset from {downloadPath}");
                return (false, null);
            }

            return (true, data);
        }

        public async Task<(bool success, DidimoDetailsResponse status, string errorCode)> CheckForStatusUntilCompletion(string statusKey, Action<float> statusProgress = null,
            IApi.StatusType statusType = IApi.StatusType.Didimo)
        {
            Task<(bool success, DidimoDetailsResponse status)> statusTask;
            do
            {
                statusTask = CheckDidimoStatus(statusKey, statusType);
                await statusTask;

                // TODO: we should be throwing errors, so they can then be caught.
                if (!statusTask.Result.success) return (false, statusTask.Result.status, statusTask.Result.status.ErrorCode);
                if (statusProgress != null)
                {
                    statusProgress(statusTask.Result.status.Percent);
                }

                await Task.Delay(CHECK_STATUS_WAIT_TIME);
            }
            while (!statusTask.Result.status.IsDone);

            return (true, statusTask.Result.status, null);
        }

        public async Task<(bool success, string didimoKey)> CreateNewDidimo(string filePath)
        {
            NewDidimoQuery newDidimoQuery = new NewDidimoQuery(filePath, DidimoNetworkingResources.NetworkConfig.GetFeaturesForApi());
            NewDidimoResponse result = await newDidimoQuery.ExecuteQuery();

            return (result != null, result?.DidimoKey);
        }

        public async Task<(bool success, DidimoDetailsResponse status)> CheckDefaultDidimoStatus()
        {
            DefaultDidimoDetailsQuery didimoDetailsQuery = new DefaultDidimoDetailsQuery();
            return await CheckDidimoStatus(didimoDetailsQuery);
        }

        public async Task<(bool success, DidimoDetailsResponse status)> CheckDidimoStatus(string didimoKey, IApi.StatusType statusType = IApi.StatusType.Didimo)
        {
            switch (statusType)
            {
                case IApi.StatusType.Didimo:
                    return await CheckDidimoStatus(new DidimoDetailsQuery(didimoKey));
                case IApi.StatusType.Deform:
                    return await CheckDidimoStatus(new DeformDetailsQuery(didimoKey));
                default:
                    throw new ArgumentOutOfRangeException(nameof(statusType), statusType, null);
            }
        }

        public async Task<(bool success, DidimoComponents didimo)> CreateDidimoAndImportGltf(string photoPath, Configuration configuration = null,
            Action<float> creationProgress = null)
        {
            Task<(bool success, string didimoKey)> createNewDidimoTask = CreateNewDidimo(photoPath);
            await createNewDidimoTask;

            if (!createNewDidimoTask.Result.success) return (false, null);

            Task<(bool success, DidimoDetailsResponse status, string errorCode)> checkForStatusUntilCompletionTask =
                CheckForStatusUntilCompletion(createNewDidimoTask.Result.didimoKey, creationProgress);
            await checkForStatusUntilCompletionTask;

            Downloadable downloadable = checkForStatusUntilCompletionTask.Result.status.GetDownloadableForTransferFormat(DidimoDetailsResponse.DownloadTransferFormatType.Gltf);
            if (downloadable == null)
            {
                Debug.LogError($"Failed to get downloadable of type {DidimoDetailsResponse.DownloadTransferFormatType.Gltf}");
                return (false, null);
            }

            (bool success, string path) downloadResult = await downloadable.DownloadToDisk(true);
            return await Import(createNewDidimoTask.Result.didimoKey, downloadResult.path, configuration);
        }

        public async Task<(bool success, Speech.Phrase phrase)> TextToSpeech(string text, string voice)
        {
            TextToSpeechQuery unsetMetaQuery = new TextToSpeechQuery(text, voice);
            Task<TextToSpeechResponse> queryTask = unsetMetaQuery.ExecuteQuery();
            await queryTask;

            return (false, null);
        }

        public async Task<bool> DeleteMetadata(string didimoKey, string key)
        {
            DeleteMetaQuery deleteMetaQuery = new DeleteMetaQuery(didimoKey, key);
            Task<DidimoEmptyResponse> queryTask = deleteMetaQuery.ExecuteQuery();
            await queryTask;

            return true;
        }

        public async Task<(bool success, string value)> GetMetadata(string didimoKey, string key)
        {
            GetMetaQuery setMetaQuery = new GetMetaQuery(didimoKey, key);
            Task<MetaDataResponse> queryTask = setMetaQuery.ExecuteQuery();
            await queryTask;

            return (true, queryTask.Result.Value);
        }

        public async Task<(bool success, string value)> UpdateMetadata(string didimoKey, string value, string key)
        {
            UpdateMetaQuery updateMetaQuery = new UpdateMetaQuery(didimoKey, value, key);
            Task<MetaDataResponse> queryTask = updateMetaQuery.ExecuteQuery();
            await queryTask;

            return (true, queryTask.Result.Value);
        }

        public async Task<bool> SetMetadata(string didimoKey, string key, string value)
        {
            SetMetaQuery setMetaQuery = new SetMetaQuery(didimoKey, key, value);
            Task<DidimoResponse> queryTask = setMetaQuery.ExecuteQuery();
            await queryTask;

            return true;
        }

        public async Task<(bool success, List<string> didimoKeys)> GetAllDidimoKeys()
        {
            Task<(bool success, List<DidimoDetailsResponse> didimos)> queryTask = GetAllDidimos();
            await queryTask;

            if (!queryTask.Result.success) return (false, null);

            List<string> didimoKeys = queryTask.Result.didimos.Select(m => m.DidimoKey).ToList();
            return (true, didimoKeys);
        }

        public async Task<(bool success, List<DidimoDetailsResponse> didimos)> GetAllDidimos()
        {
            ListQuery listQuery = new ListQuery();
            Task<ListResponse> queryTask = listQuery.ExecuteQuery();
            await queryTask;

            if (!queryTask.Result.Didimos.Any())
            {
                Debug.LogWarning("GetAllDidimos API call succeeded but there were no didimos on the account.");
                return (false, null);
            }

            return (true, queryTask.Result.Didimos.ToList());
        }

        public async Task<(bool success, DidimoComponents didimo)> ImportDefaultDidimoGltf(Configuration configuration = null)
        {
            Task<DidimoDetailsResponse> detailsQuery = new DefaultDidimoDetailsQuery().ExecuteQuery();
            await detailsQuery;

            Downloadable downloadable = detailsQuery.Result.GetDownloadableForTransferFormat(DidimoDetailsResponse.DownloadTransferFormatType.Gltf);
            if (downloadable == null)
            {
                Debug.LogError($"Failed to get downloadable of type {DidimoDetailsResponse.DownloadTransferFormatType.Gltf}");
                return (false, null);
            }

            (bool success, string path) = await downloadable.DownloadToDisk(true);
            if (!success)
            {
                Debug.LogError("Failed to download didimo.");
                return (false, null);
            }

            return await Import(detailsQuery.Result.DidimoKey, path, configuration);
        }

        public async Task<bool> DeleteDidimo(string didimoKey)
        {
            DeleteQuery deleteQuery = new DeleteQuery(didimoKey);
            await deleteQuery.ExecuteQuery();
            return true;
        }

        public async Task<(bool success, DidimoDetailsResponse status)> CheckDidimoStatus(GetQuery<DidimoDetailsResponse> detailsQuery)
        {
            Task<DidimoDetailsResponse> queryTask = detailsQuery.ExecuteQuery();
            await queryTask;

            if (queryTask.Result == null)
            {
                return (false, null);
            }

            return (true, queryTask.Result);
        }

        private static async Task<(bool success, DidimoComponents didimo)> Import(string didimoKey, string path, Configuration configuration = null)
        {
            DidimoComponents didimoComponents = await DidimoLoader.LoadDidimoInFolder(didimoKey, path, configuration);

            return (true, didimoComponents);
        }
    }
}