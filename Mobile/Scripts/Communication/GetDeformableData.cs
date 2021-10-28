#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class GetDeformableData : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        public delegate void GetDeformableDataSuccessDelegate(IntPtr obj, byte[] data);
#elif UNITY_IOS
        public delegate void GetDeformableDataSuccessDelegate(IntPtr obj, IntPtr data, int dataSize);
#endif
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$GetDeformableDataInterface") { }

            public void sendToUnity(string didimoKey, string deformableId, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    didimoKey,
                    deformableId,
                    (obj, bytes) =>
                    {
                        response.Call("onSuccess", bytes);
                    },
                    (obj, message) =>
                    {
                        CallOnError(response, message);
                    });
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerGetDeformableData(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, string didimoKey, string deformableId, GetDeformableDataSuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerGetDeformableData(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, string didimoKey, string deformableId, GetDeformableDataSuccessDelegate successDelegate, ErrorDelegate errorDelegate)
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
                    successDelegate(obj, data);
#elif UNITY_IOS
                    GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                    successDelegate(obj, pointer, data.Length);
                    pinnedArray.Free();
#endif
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    errorDelegate(obj, e.Message);
                }
            });
        }
    }
}

#endif