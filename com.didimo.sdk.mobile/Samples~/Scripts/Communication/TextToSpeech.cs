#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class TextToSpeech : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$TextToSpeechInterface") { }

            public void sendToUnity(string didimoKey, string dataPath, string clipPath, AndroidJavaObject response)
            {
                _ = CallAsync(didimoKey,
                    dataPath,
                    clipPath,
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

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerTextToSpeech(CbMessage); }

        public delegate void InputDelegate(string didimoKey, string dataPath, string clipPath, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerTextToSpeech(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
        private static void CbMessage(string didimoKey, string dataPath, string clipPath, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            _ = CallAsync(didimoKey, dataPath, clipPath, successCallback, errorCallback, objectPointer);
        }
#endif

        private static async Task CallAsync(string didimoKey, string dataPath, string clipPath, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            try
            {
                if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                {
                    await Speech.PhraseBuilder.Build(dataPath, clipPath, (phrase) => ThreadingUtility.WhenMainThread(() => didimo.Speech.Speak(phrase)));
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
        }
    }
}
#endif