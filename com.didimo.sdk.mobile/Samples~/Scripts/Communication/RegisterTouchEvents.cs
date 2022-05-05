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
    public class RegisterTouchEvents : BiDirectionalNativeInterface
    {
        public delegate void TouchCallback(int x, int y, IntPtr objectPointer);

#if UNITY_ANDROID
        protected static void CallTouchCallback(AndroidJavaObject javaObject, int x, int y, string methodName) { javaObject.Call(methodName, x, y); }

        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$RegisterTouchEventsInterface") { }

            public void sendToUnity(AndroidJavaObject response)
            {
                CbMessage((x, y, javaObj) =>
                    {
                        CallTouchCallback(response, x, y, "doubleTapCallback");
                    },
                    (x, y, javaObj) =>
                    {
                        CallTouchCallback(response, x, y, "startDragCallback");
                    },
                    (x, y, javaObj) =>
                    {
                        CallTouchCallback(response, x, y, "endDragCallback");
                    },
                    IntPtr.Zero);
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerRegisterTouchEvents(CbMessage); }

        public delegate void InputDelegate(TouchCallback doubleTapCallback, TouchCallback startDragCallback, TouchCallback endDragCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerRegisterTouchEvents(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]

#endif
        private static void CbMessage(TouchCallback doubleTapCallback, TouchCallback startDragCallback, TouchCallback endDragCallback, IntPtr objectPointer)
        {
            TouchInputProcessor.CallbackEvents callbackEvents = new TouchInputProcessor.CallbackEvents(doubleTapCallback, startDragCallback, endDragCallback, objectPointer);
            TouchInputProcessor.Instance.Register(callbackEvents);
        }
    }
}
#endif