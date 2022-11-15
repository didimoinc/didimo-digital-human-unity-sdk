using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo
{
    public class UnityWebRequestAwaiter : INotifyCompletion
    {
        private readonly UnityWebRequestAsyncOperation _asyncOperation;
        public bool IsCompleted => _asyncOperation.isDone;
        public UnityWebRequestAwaiter( UnityWebRequestAsyncOperation asyncOperation ) => _asyncOperation = asyncOperation;
        public void OnCompleted( Action continuation ) => _asyncOperation.completed += _ => continuation();
        public UnityWebRequest GetResult() => _asyncOperation.webRequest;
    }
}