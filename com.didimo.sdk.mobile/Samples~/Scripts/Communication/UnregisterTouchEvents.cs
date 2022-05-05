#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Extensions;

namespace Didimo.Mobile.Communication
{
    public class UnregisterTouchEvents : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$UnregisterTouchEventsInterface") { }

            public void sendToUnity(string didimoKey, string dataPath, string clipPath, AndroidJavaObject response)
            {
                CbMessage(
                    (obj) =>
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
        
        protected override void RegisterNativeCall() { registerUnregisterTouchEvents(CbMessage); }

        public delegate void InputDelegate(SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerUnregisterTouchEvents(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            try
            {
                TouchInputProcessor.Instance.UnRegister(objectPointer);
                successCallback(objectPointer);
            }
            catch (Exception e)
            {
                errorCallback(objectPointer, e.Message);
            }
        }
    }
}
#endif