#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Core.Deformables;

namespace Didimo.Mobile.Communication
{
    public class GetDeformableData : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        public delegate void GetDeformableDataSuccessCallback(IntPtr obj, byte[] data);
#elif UNITY_IOS
        public delegate void GetDeformableDataSuccessCallback(IntPtr obj, IntPtr data, int dataSize);
#endif
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$GetDeformableDataInterface") { }

            public void sendToUnity(string didimoKey, string deformableId, AndroidJavaObject response)
            {
                CbMessage(didimoKey,
                    deformableId,
                    (obj, bytes) =>
                    {
                        response.Call("onSuccess", bytes);
                    },
                    (obj, message) =>
                    {
                        CallOnError(response, message);
                    },
                    IntPtr.Zero);
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerGetDeformableData(CbMessage); }

        public delegate void InputDelegate(string didimoKey, string deformableId, GetDeformableDataSuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerGetDeformableData(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(string didimoKey, string deformableId, GetDeformableDataSuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (!DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        throw new Exception($"Could not find didimo with ID: {didimoKey}");
                    }

                    if (!didimo.Deformables.TryFind(deformableId, out Deformable deformable))
                    {
                        throw new Exception($"Could not find deformable with ID: {deformableId}");
                    }

                    byte[] data = deformable.GetUndeformedMeshData();
#if UNITY_ANDROID
                    successCallback(objectPointer, data);
#elif UNITY_IOS
                    GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    successCallback(objectPointer, pointer, data.Length);
                    pinnedArray.Free();
#endif
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    errorCallback(objectPointer, e.Message);
                }
            });
        }
    }
}

#endif