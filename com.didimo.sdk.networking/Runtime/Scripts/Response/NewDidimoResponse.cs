using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class NewDidimoResponse : DidimoResponse
    {
        [JsonProperty("key")] public string DidimoKey { get; private set; }
    }
}