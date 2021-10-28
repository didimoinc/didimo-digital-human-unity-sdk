#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class TextToSpeech : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$TextToSpeechInterface") { }

            public void sendToUnity(string didimoKey, string dataPath, string clipPath, AndroidJavaObject response)
            {
                _ = CallAsync(IntPtr.Zero,
                    didimoKey,
                    dataPath,
                    clipPath,
                    (obj) =>
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
        protected override void RegisterNativeCall() { registerTextToSpeech(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, string didimoKey, string dataPath, string clipPath, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerTextToSpeech(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
        private static void CbMessage(IntPtr obj, string didimoKey, string dataPath, string clipPath, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            _ = CallAsync(obj, didimoKey, dataPath, clipPath, successDelegate, errorDelegate);
        }
#endif

        private static async Task CallAsync(IntPtr obj, string didimoKey, string dataPath, string clipPath, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            try
            {
                if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                {
                    await Speech.PhraseBuilder.Build(dataPath, clipPath, (phrase) => ThreadingUtility.WhenMainThread(() => didimo.Speech.Speak(phrase)));
                    successDelegate(obj);
                }
                else
                {
                    throw new Exception($"Unable to find didimo with id {didimoKey}");
                }
            }
            catch (Exception e)
            {
                errorDelegate(obj, e.Message);
            }
        }
    }
}
#endif