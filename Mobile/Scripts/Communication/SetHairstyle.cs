#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class SetHairstyle : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetHairstyleInterface") { }

            public void sendToUnity(string didimoKey, string styleID, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    didimoKey,
                    styleID,
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
        protected override void RegisterNativeCall() { registerSetHairstyle(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, string didimoKey, string styleID, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetHairstyle(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, string didimoKey, string styleID, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
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
                    
                    successDelegate(obj);
                }
                catch (Exception e)
                {
                    errorDelegate(obj, e.Message);
                }
            });
        }
    }
}
#endif