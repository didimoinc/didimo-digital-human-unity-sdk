using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class Downloadable
    {
        public enum ArtifactTypes
        {
            Side_png,
            Front_png,
            Corner_png,
            Deformer_dmx
        }

        public enum TransferFormatTypes
        {
            Fbx,
            Gltf,
            Package
        }

        public DidimoDetailsResponse DidimoDetails;
        [JsonProperty("__type")] public string Type { get; private set; }

        [JsonProperty("name")] public string Name { get; private set; }
        [JsonProperty("__links")] public Links Links { get; private set; }

        public async Task<(bool success, string path)> DownloadToDisk(bool unpack)
        {
            Task<DownloadResponse> getDownloadLinkTask = new DownloadQuery(Links.Self).ExecuteQuery();
            await getDownloadLinkTask;

            Task<(bool success, string path)> downloadTask = unpack
                ? DidimoDownloader.DownloadAndUnpack(DidimoDetails.DidimoKey + "_" + Name, getDownloadLinkTask.Result.Uri)
                : DidimoDownloader.DownloadToDisk(DidimoDetails.DidimoKey + "_" + Name, getDownloadLinkTask.Result.Uri);
            return await downloadTask;
        }

        public async Task<(bool success, byte[] data)> Download()
        {
            Task<DownloadResponse> getDownloadLinkTask = new DownloadQuery(Links.Self).ExecuteQuery();
            await getDownloadLinkTask;

            Task<(bool success, byte[] data)> downloadTask = DidimoDownloader.Download(getDownloadLinkTask.Result.Uri);
            return await downloadTask;
        }
    }
}