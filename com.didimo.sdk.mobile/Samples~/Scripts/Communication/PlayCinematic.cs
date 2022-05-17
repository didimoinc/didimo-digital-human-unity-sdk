#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;
using Object = UnityEngine.Object;
using Didimo.Core.Utility;
using Didimo.Mobile.Controller;

namespace Didimo.Mobile.Communication
{
    public class PlayCinematic : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$PlayCinematicInterface") { }

            public void sendToUnity(string cinematicID, string didimoKey, AndroidJavaObject response)
            {
                CbMessage(cinematicID,
                    didimoKey,
                    obj =>
                    {
                        CallOnSuccess(response);
                    },
                    (obj, progress) =>
                    {
                        CallOnProgress(response, progress);
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
        protected override void RegisterNativeCall() { registerPlayCinematic(CbMessage); }

        public delegate void InputDelegate(string cinematicID, string didimoKey, SuccessCallback successCallback, ProgressCallback progressCallback, ErrorCallback errorCallback,
            IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerPlayCinematic(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(string cinematicID, string didimoKey, SuccessCallback successCallback, ProgressCallback progressCallback, ErrorCallback errorCallback,
            IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    ARKitCaptureStreamController.RemoveForDidimo(didimoKey);
                    if (string.IsNullOrEmpty(cinematicID))
                    {
                        CinematicManager.Instance.StopCinematic();
                        DidimoLookAtController.Instance.EnableLookAt(true);
                    }
                    else
                    {
                        DidimoLookAtController.Instance.EnableLookAt(false);
                        CinematicManager.Instance.PlayCinematic(cinematicID,
                            animationTime =>
                            {
                                progressCallback(objectPointer, animationTime);
                                if (animationTime >= 1)
                                {
                                    DidimoLookAtController.Instance.EnableLookAt(true);
                                }
                            });

                        CinematicManager.Instance.RemapTimeline(didimoKey);
                        successCallback(objectPointer);
                    }
                }
                catch (Exception e)
                {
                    DidimoLookAtController.Instance.EnableLookAt(true);
                    errorCallback(objectPointer, e.Message);
                }
            });
        }
    }
}
#endif