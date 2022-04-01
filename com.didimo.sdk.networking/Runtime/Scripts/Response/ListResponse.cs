using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class ListResponse : DidimoResponse
    {
        [JsonProperty("total_size")] public int NumPages { get; private set; }

        [JsonProperty("next_page_uri")] public string NextPageUri { get; private set; }

        [JsonProperty("previous_page_uri")] public string PreviousPageUri { get; private set; }

        [JsonProperty("didimos")] public DidimoDetailsResponse[] Didimos { get; private set; }
    }
}