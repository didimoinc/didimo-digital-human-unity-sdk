using System;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public abstract class DeleteQuery<TResult> : Query<TResult>
    {
        protected override UnityWebRequest CreateRequest(Uri uri) => UnityWebRequest.Delete(uri);
    }
}