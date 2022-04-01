using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class MetaDataResponse : DidimoResponse
    {
        [JsonProperty("name")] public string Name { get; private set; }
        [JsonProperty("value")] public string Value { get; private set; }
        [JsonProperty("definer")] public string Definer { get; private set; }
    }
}