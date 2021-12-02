using System;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class DownloadResultQuery : DownloadQuery
    {
        public DownloadResultQuery(string uri) : base(uri) { }
    }
}