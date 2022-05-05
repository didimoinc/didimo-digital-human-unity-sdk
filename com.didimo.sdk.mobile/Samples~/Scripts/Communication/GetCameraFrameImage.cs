#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Core.Deformables;

namespace Didimo.Mobile.Communication
{
    public class GetCameraFrameImage : BiDirectionalNativeInterface
    {
        private static readonly Vector2Int FRAME_RESOLUTION = new Vector2Int(1080, 1350);
        private static readonly string     OVERLAY_LOCATION = "screenshot_overlay";

#if UNITY_ANDROID
        public delegate void GetCameraFrameImageSuccessCallback(IntPtr obj, byte[] pngImageData);
#elif UNITY_IOS
        public delegate void GetCameraFrameImageSuccessCallback(IntPtr pngImageData, int dataSize, IntPtr objectPointer);
#endif

#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$GetCameraFrameImageInterface") { }

            public void sendToUnity(bool withDidimoWatermark, AndroidJavaObject response)
            {
                CbMessage((obj, pngImage) =>
                    {
                        response.Call("onSuccess", pngImage);
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
        protected override void RegisterNativeCall() { registerGetCameraFrameImage(CbMessage); }

        public delegate void InputDelegate(GetCameraFrameImageSuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerGetCameraFrameImage(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(GetCameraFrameImageSuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    CameraUtility.GetNextCameraFrameImage(image =>
                        {
                            byte[] data = image.EncodeToPNG();
#if UNITY_ANDROID
                            successCallback(objectPointer, data);
#elif UNITY_IOS
                            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
                            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                            successCallback(pointer, data.Length, objectPointer);
                            pinnedArray.Free();
#endif
                        },
                        FRAME_RESOLUTION.x,
                        FRAME_RESOLUTION.y,
                        // This isn't included in the default "didimo resources" scriptable object, so we aren't forced to keep a 5MB image in memory
                        Resources.Load<Texture2D>(OVERLAY_LOCATION));
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