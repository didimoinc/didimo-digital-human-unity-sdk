using UnityEngine.Networking;

namespace Didimo
{
    public static class WebRequestExtensionMethods
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp) => new UnityWebRequestAwaiter(asyncOp);
    }
}