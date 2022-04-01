using System;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public abstract class JsonPostQuery<TData, TResult> : Query<TResult>
    {
        protected abstract TData Data { get; }
        protected override UnityWebRequest CreateRequest(Uri uri) => UnityWebRequest.Post(uri, GetSerializedData());

        protected virtual string GetSerializedData() => JsonConvert.SerializeObject(Data);
    }
}