using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class DidimoDetailsResponse : DidimoResponse
    {
        private const string STATUS_PENDING    = "pending";
        private const string STATUS_PROCESSING = "processing";
        private const string STATUS_ERROR      = "error";
        private const string STATUS_DONE       = "done";

        [JsonProperty("key")] public string DidimoKey { get; private set; }
        [JsonProperty("input_type")] public string InputType { get; private set; }
        [JsonProperty("transfer_formats")] public Downloadable[] TransferFormats { get; private set; }
        [JsonProperty("artifacts")] public Downloadable[] Artifacts { get; private set; }
        [JsonProperty("percent")] public float Percent { get; private set; }
        [JsonProperty("status")] public string CurrentStatus { get; private set; }
        [JsonProperty("status_message")] public string StatusMessage { get; private set; }
        [JsonProperty("error_code")] public string ErrorCode { get; private set; }
        [JsonProperty("meta_data")] public MetaDataResponse[] MetaData { get; private set; }
        [JsonProperty("created_at")] public string CreatedAt { get; private set; }
        public bool IsDone => CurrentStatus == STATUS_ERROR || CurrentStatus == STATUS_DONE;

        public enum DownloadArtifactType
        {
            Side_png,
            Front_png,
            Corner_png,
            Deformer_dmx
        }

        public enum DownloadTransferFormatType
        {
            Fbx,
            Gltf,
            Package
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (Downloadable transferFormat in TransferFormats)
            {
                transferFormat.DidimoDetails = this;
            }

            foreach (Downloadable artifact in Artifacts)
            {
                artifact.DidimoDetails = this;
            }
        }

        public Downloadable GetDownloadableArtifact(DownloadArtifactType artifactType) { return Artifacts.FirstOrDefault(x => x.Name.ToLower() == artifactType.ToString().ToLower()); }

        public Downloadable GetDownloadableForTransferFormat(DownloadTransferFormatType downloadTransferFormatType)
        {
            return TransferFormats.FirstOrDefault(x => x.Name.ToLower() == downloadTransferFormatType.ToString().ToLower());
        }
    }
}