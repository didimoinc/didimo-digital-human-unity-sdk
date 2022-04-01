using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class Links
    {
        [JsonProperty("self")] public string Self { get; private set; }
    }
}