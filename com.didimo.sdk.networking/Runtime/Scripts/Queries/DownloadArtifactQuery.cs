using System;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class DownloadArtifactQuery : DownloadQuery
    {
     
        public DownloadArtifactQuery(string path) : base(path) { }
    }
}