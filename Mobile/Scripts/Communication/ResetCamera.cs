#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class ResetCamera : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$ResetCameraInterface") { }

            public void sendToUnity(bool instant, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    instant,
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
        protected override void RegisterNativeCall() { registerResetCamera(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, bool instant, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerResetCamera(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, bool instant, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
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
                    if (dragOrbit == null)
                    {
                        throw new Exception("Unable to get DragOrbit component on main camera");
                    }

                    dragOrbit.ResetView(instant);
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