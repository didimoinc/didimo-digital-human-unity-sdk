#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class UpdateDeformable : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$UpdateDeformableInterface") { }

            public void sendToUnity(string didimoKey, string deformableId, byte[] deformedData, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    didimoKey,
                    deformableId,
                    deformedData,
                    obj =>
                    {
                        CallOnSuccess(response);
                    },
                    (obj, message) =>
                    {
                        CallOnError(response, message);
                    });
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerUpdateDeformable(CbMessage); }

#if UNITY_ANDROID
        public delegate void InputDelegate(IntPtr obj, string didimoKey, string deformableId, byte[] deformedData, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);
#elif UNITY_IOS
        public delegate void InputDelegate(IntPtr obj, string didimoKey, string deformableId, IntPtr deformedData, int dataSize, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);
#endif

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerUpdateDeformable(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
#if UNITY_ANDROID
        private static void CbMessage(IntPtr obj, string didimoKey, string deformableId, byte[] deformedData, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
#elif UNITY_IOS
        private static void CbMessage(IntPtr obj, string didimoKey, string deformableId, IntPtr data, int dataSize, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            // Copy the data ASAP, synchronously. If we wait to do it in the main thread, the data might have gone out of scope
            byte[] deformedData = new byte[dataSize];
            Marshal.Copy(data, deformedData, 0, dataSize);
#endif
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

                    deformable.SetDeformedMeshData(deformedData);
                    deformable.gameObject.SetActive(true);
                    successDelegate(obj);
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