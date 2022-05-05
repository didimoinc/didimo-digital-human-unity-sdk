#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Object = UnityEngine.Object;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class SetOrbitControlsEnabled : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetOrbitControlsEnabledInterface") { }

            public void sendToUnity(bool enabled, AndroidJavaObject response)
            {
                CbMessage(enabled,
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
        protected override void RegisterNativeCall() { registerSetOrbitControlsEnabled(CbMessage); }

        public delegate void InputDelegate(bool enabled, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetOrbitControlsEnabled(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(bool enabled, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    Camera cam = Camera.main;
                    if (!cam)
                    {
                        throw new Exception("Unable to get main camera");
                    }

                    DragOrbit dragOrbit = cam.GetComponent<DragOrbit>();
                    if (enabled && dragOrbit == null) dragOrbit = cam.gameObject.AddComponent<DragOrbit>();
                    if (!enabled && dragOrbit != null) Object.Destroy(dragOrbit);
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