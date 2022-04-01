using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class ProfileResponse : DidimoResponse
    {
        [JsonProperty("points")] public int Points { get; private set; }

        [JsonProperty("tier_code")] public string TierCode { get; private set; }

        [JsonProperty("tier_label")] public string TierLabel { get; private set; }

        [JsonProperty("next_expiration_date")] public string NextExpirationDateStr { get; private set; }

        [JsonProperty("next_expiration_points")]
        public string NextExpirationPoints { get; private set; }

        [JsonProperty("available_features")] public string[] AvailableFeatures { get; private set; }
    }
}