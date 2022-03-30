#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class SetEyeColor : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetEyeColorInterface") { }

            public void sendToUnity(string didimoKey, int eyeColorID, AndroidJavaObject response)
            {
                CbMessage(eyeColorID,
                    didimoKey,
                    obj =>
                    {
                        CallOnSuccess(response);
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
        protected override void RegisterNativeCall() { registerSetEyeColor(CbMessage); }

        public delegate void InputDelegate(int eyeColorID, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetEyeColor(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(int eyeColorID, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        didimo.IrisController.SetPreset(eyeColorID);
                    }
                    else
                    {
                        throw new Exception($"Unable to find didimo with id {didimoKey}");
                    }

                    successCallback(objectPointer);
                }
                catch (Exception e)
                {
                    errorCallback(objectPointer, e.Message + e.StackTrace);
                }
            });
        }
    }
}
#endif