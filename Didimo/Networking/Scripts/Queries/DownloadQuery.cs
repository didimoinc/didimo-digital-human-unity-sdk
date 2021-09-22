using System;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class DownloadQuery : GetQuery<DownloadResponse>
    {
        // When we try to download a didimo, our server will redirect to the download link.
        // However, as we are setting an api key header on the initial request, Unity will also set this header for the redirected request.
        // This will cause the request to fail. To avoid this, we use the following header to ask the server to send us the link instead of a straight redirect.
        private const string AVOID_REDIRECT_HEADER = "DIDIMO-FOLLOW-URI";
        protected override string URL { get; }

        public DownloadQuery(string url)
        {
            URL = url;
        }

        protected override void BuildAdditionalHeaders(Uri uri, UnityWebRequest request)
        {
            base.BuildAdditionalHeaders(uri, request);
            request.SetRequestHeader(AVOID_REDIRECT_HEADER, "0");
        }
    }
}