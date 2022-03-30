using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class DownloadResponse : DidimoResponse
    {
        [JsonProperty("uri")] public string Uri { get; private set; }
    }
}