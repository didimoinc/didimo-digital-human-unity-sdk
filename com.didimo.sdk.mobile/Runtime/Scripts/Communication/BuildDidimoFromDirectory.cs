#if UNITY_IOS || UNITY_ANDROID
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Didimo.Builder;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class BuildDidimoFromDirectory : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$BuildDidimoFromDirectoryInterface") { }

            public void sendToUnity(string didimoDirectory, string didimoKey, AndroidJavaObject response)
            {
                ThreadingUtility.WhenMainThread(() =>
                {
                    _ = CallAsync(didimoDirectory,
                        didimoKey,
                        obj =>
                        {
                            CallOnSuccess(response);
                        },
                        (obj, message) =>
                        {
                            CallOnError(response, message);
                        },
                        IntPtr.Zero);
                });
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerBuildDidimoFromDirectory(CbMessage); }

        public delegate void InputDelegate(string didimoDirectory, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [MonoPInvokeCallback(typeof(InputDelegate))]
        private static void CbMessage(string didimoDirectory, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            _ = CallAsync(didimoDirectory, didimoKey, successCallback, errorCallback, objectPointer);
        }

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerBuildDidimoFromDirectory(InputDelegate cb);

#endif
        private static async Task CallAsync(string didimoDirectory, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            try
            {
                Task<DidimoComponents> task = DidimoLoader.LoadDidimoInFolder(didimoKey, didimoDirectory);
                await task;
                DidimoCache.Add(task.Result);
                successCallback(objectPointer);
            }
            catch (Exception e)
            {
                errorCallback(objectPointer, e.ToString());
            }
        }
    }
}
#endif