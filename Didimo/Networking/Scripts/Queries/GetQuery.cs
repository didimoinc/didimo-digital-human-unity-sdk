using System;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public abstract class GetQuery<TResult> : Query<TResult>
    {
        protected override UnityWebRequest CreateRequest(Uri uri) => UnityWebRequest.Get(uri);
    }
}