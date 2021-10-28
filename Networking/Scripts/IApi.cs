using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Didimo.Builder;
using UnityEngine.Serialization;

namespace Didimo.Networking
{
    public interface IApi
    {
        enum StatusType
        {
            Didimo,
            Deform
        }

        Task<(bool success, AccountStatusResponse status)> AccountStatus();
        Task<(bool success, byte[] data)> Deform(string didimoKey, byte[] data, Action<float> statusProgress = null);

        Task<(bool success, DidimoDetailsResponse status, string errorCode)> CheckForStatusUntilCompletion(string statusKey, Action<float> statusProgress = null,
            StatusType statusType = StatusType.Didimo);

        Task<(bool success, string didimoKey)> CreateNewDidimo(string filePath);
        Task<(bool success, DidimoDetailsResponse status)> CheckDefaultDidimoStatus();

        Task<(bool success, DidimoDetailsResponse status)> CheckDidimoStatus(string didimoKey, StatusType statusType = StatusType.Didimo);
        
        Task<(bool success, DidimoComponents didimo)> CreateDidimoAndImportGltf(string photoPath, Configuration configuration = null, Action<float> creationProgress = null);

        Task<(bool success, Speech.Phrase phrase)> TextToSpeech(string text, string voice);

        Task<bool> DeleteMetadata(string didimoKey, string key);
        Task<(bool success, string value)> GetMetadata(string didimoKey, string key);

        Task<(bool success, string value)> UpdateMetadata(string didimoKey, string value, string key);
        Task<bool> SetMetadata(string didimoKey, string key, string value);
        Task<(bool success, List<string> didimoKeys)> GetAllDidimoKeys();
        Task<(bool success, List<DidimoDetailsResponse> didimos)> GetAllDidimos();

        Task<(bool success, DidimoComponents didimo)> ImportDefaultDidimoGltf(Configuration configuration = null);

        Task<bool> DeleteDidimo(string didimoKey);

        Task<(bool success, DidimoDetailsResponse status)> CheckDidimoStatus(GetQuery<DidimoDetailsResponse> detailsQuery);
    }
}