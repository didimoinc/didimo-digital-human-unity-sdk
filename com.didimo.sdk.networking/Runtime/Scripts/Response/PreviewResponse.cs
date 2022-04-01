using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class PreviewResponse : DidimoResponse
    {
        [JsonProperty("location")] public string Location { get; private set; }
    }
}