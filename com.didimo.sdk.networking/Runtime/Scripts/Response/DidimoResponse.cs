using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class DidimoResponse
    {
        [JsonProperty("__type")] public string ResponseType { get; private set; }
    }
}