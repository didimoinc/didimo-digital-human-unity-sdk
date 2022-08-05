#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Core.Deformables;

namespace Didimo.Mobile.Communication
{
    public class CanDeformAssets : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        public delegate void CanDeformAssetsSuccessCallback(bool isAbleToDeform, IntPtr obj);
#elif UNITY_IOS
        public delegate void CanDeformAssetsSuccessCallback(bool isAbleToDeform, IntPtr objectPointer);
#endif

#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$CanDeformAssetsInterface") { }

            public void sendToUnity(string didimoKey, string styleID, AndroidJavaObject response)
            {
                CbMessage(didimoKey,
                    (obj, isAbleToDeform) =>
                    {
                        response.Call("onSuccess", isAbleToDeform);
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

        protected override void RegisterNativeCall() { registerCanDeformAssets(CbMessage); }

        public delegate void InputDelegate(string didimoKey, CanDeformAssetsSuccessCallback canDeformAssetsSuccessCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerCanDeformAssets(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(string didimoKey, CanDeformAssetsSuccessCallback canDeformAssetsSuccessCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (!DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        throw new Exception($"Could not find didimo with ID {didimoKey}");
                    }

                    canDeformAssetsSuccessCallback(didimo.Deformables.Deformer.IsAbleToDeform(), objectPointer);
                }
                catch (Exception e)
                {
                    errorCallback(objectPointer, e.Message);
                }
            });
        }
    }
}
#endif