using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class DeformResponse : DidimoResponse
    {
        [JsonProperty("key")] public string DeformedID { get; private set; }
    }
}