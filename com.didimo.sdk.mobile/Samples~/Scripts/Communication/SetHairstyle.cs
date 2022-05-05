#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Core.Deformables;

namespace Didimo.Mobile.Communication
{
    public class SetHairstyle : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetHairstyleInterface") { }

            public void sendToUnity(string didimoKey, string styleID, AndroidJavaObject response)
            {
                CbMessage(didimoKey,
                    styleID,
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
        protected override void RegisterNativeCall() { registerSetHairstyle(CbMessage); }

        public delegate void InputDelegate(string styleID, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetHairstyle(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(string styleID, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (!DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        throw new Exception($"Could not find didimo with ID {didimoKey}");
                    }

                    if (string.IsNullOrEmpty(styleID))
                    {
                        didimo.Deformables.DestroyAll<Hair>();
                    }
                    else if (didimo.Deformables.TryCreate(styleID, out Hair hair))
                    {
                        hair.gameObject.SetActive(false);
                    }
                    else
                    {
                        throw new Exception($"Could not create hairstyle with ID {styleID}");
                    }

                    successCallback(objectPointer);
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