#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Object = UnityEngine.Object;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class SetViewRect : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$DestroyDidimoInterface") { }

            public void sendToUnity(float x, float y, float width, float height, AndroidJavaObject response)
            {
                CbMessage(x,
                    y,
                    width,
                    height,
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
        protected override void RegisterNativeCall() { registerSetViewRect(CbMessage); }

        public delegate void InputDelegate(float x, float y, float width, float height, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetViewRect(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(float x, float y, float width, float height, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    Camera.main!.rect = new Rect(x, y, width, height);
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