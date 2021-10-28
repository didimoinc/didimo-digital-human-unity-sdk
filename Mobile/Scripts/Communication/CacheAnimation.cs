#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class CacheAnimation : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$CacheAnimationInterface") { }

            public void sendToUnity(string id, string filePath, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    id,
                    filePath,
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
        protected override void RegisterNativeCall() { registerCacheAnimation(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, string id, string filePath, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerCacheAnimation(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, string id, string filePath, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    DidimoAnimation mocapAnimation = DidimoAnimation.FromJSON(id, filePath);
                    AnimationCache.Add(id, mocapAnimation);
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