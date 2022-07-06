#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Mobile.Communication
{
    public class SetHeadRotation : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetHeadRotationInterface") { }

            public void sendToUnity(float[] headRotation, string didimoKey, AndroidJavaObject response)
            {
                CbMessage(headRotation,
                    didimoKey,
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
        private static void CbMessage(float[] headRotation, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerSetHeadRotation(CbMessage); }

        public delegate void InputDelegate(IntPtr headRotation, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback,
            IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetHeadRotation(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
        private static void CbMessage(IntPtr headRotationPtr, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            float[] headRotation = new float[4];
            Marshal.Copy(headRotationPtr, headRotation, 0, 4);
#endif
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        Quaternion quaternion = new Quaternion(headRotation[0], headRotation[1], headRotation[2], headRotation[3]);
                        if (!didimo.PoseController.SetHeadRotation(quaternion))
                        {
                            throw new Exception($"Unable to set head rotation for didimo {didimoKey}");
                        }
                        successCallback(objectPointer);
                    }
                    else
                    {
                        throw new Exception($"Unable to find didimo with id {didimoKey}");
                    }
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