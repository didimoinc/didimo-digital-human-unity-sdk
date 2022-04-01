using System;
using System.Collections.Generic;

namespace Didimo.Networking
{
    public class APIKeyRequestHeader : IRequestHeader
    {
        private const string AUTHORIZATION_FIELD = "Didimo-Api-Key";

        private readonly string apiKey;

        private APIKeyRequestHeader(string apiKey) { this.apiKey = apiKey; }

        public bool TryGetHeaders(Uri uri, out Dictionary<string, string> headers)
        {
            headers = new Dictionary<string, string>();
            headers[AUTHORIZATION_FIELD] = apiKey;
            return true;
        }

        public static APIKeyRequestHeader FromConfig() => new APIKeyRequestHeader(DidimoNetworkingResources.NetworkConfig.ApiKey);

        private static string HashToString(byte[] hash) => BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}