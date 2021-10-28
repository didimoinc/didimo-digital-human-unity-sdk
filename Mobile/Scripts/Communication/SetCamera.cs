#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class SetCamera : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetCameraInterface") { }

            public void sendToUnity(float[] position, float[] rotation, float fieldOfView, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    position,
                    rotation,
                    fieldOfView,
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
        protected override void RegisterNativeCall() { registerSetCamera(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, float[] position, float[] rotation, float fieldOfView, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetCamera(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, float[] position, float[] rotation, float fieldOfView, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    Camera cam = Camera.main;
                    if (cam == null)
                    {
                        Debug.LogWarning("Failed to SetCamera as no valid camera was found.");
                        return;
                    }

                    Vector3 unityPosition = new Vector3(position[0], position[1], position[2]);
                    Quaternion unityRotation = new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);

                    Transform transform = cam.transform;
                    transform.position = unityPosition;
                    transform.rotation = unityRotation;
                    cam.fieldOfView = fieldOfView;
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