#if UNITY_IOS || UNITY_ANDROID
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Didimo.Builder;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class BuildDidimoFromDirectory : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$BuildDidimoFromDirectoryInterface") { }

            public void sendToUnity(string didimoPath, string didimoKey, AndroidJavaObject response)
            {
                ThreadingUtility.WhenMainThread(() =>
                {
                    _ = CallAsync(IntPtr.Zero,
                        didimoPath,
                        didimoKey,
                        obj =>
                        {
                            CallOnSuccess(response);
                        },
                        (obj, message) =>
                        {
                            CallOnError(response, message);
                        });
                });
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerBuildDidimo(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, string didimoPath, string didimoKey, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [MonoPInvokeCallback(typeof(InputDelegate))]
        private static void CbMessage(IntPtr obj, string path, string didimoKey, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
#pragma warning disable 4014
            CallAsync(obj, path, didimoKey, successDelegate, errorDelegate);
#pragma warning restore 4014
        }

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerBuildDidimo(InputDelegate cb);

#endif
        private static async Task CallAsync(IntPtr obj, string path, string didimoKey, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            try
            {
                Task<DidimoComponents> task = DidimoLoader.LoadDidimoInFolder(didimoKey, path);
                await task;
                DidimoCache.Add(task.Result);
                successDelegate(obj);
            }
            catch (Exception e)
            {
                errorDelegate(obj, e.ToString());
            }
        }
    }
}
#endif