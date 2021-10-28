#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Didimo.Mobile.Scripts
{
    public class SetOrbitControls : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetOrbitControlsInterface") { }

            public void sendToUnity(bool enabled, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    enabled,
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
        protected override void RegisterNativeCall() { registerSetOrbitControls(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, bool enabled, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetOrbitControls(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, bool enabled, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
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