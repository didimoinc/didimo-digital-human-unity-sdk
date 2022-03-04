#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class SetupARKit : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetupARKitInterface") { }

            public void sendToUnity(string blendshapeNames, string didimoKey, AndroidJavaObject response)
            {
                CbMessage(blendshapeNames,
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

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerSetupARKit(CbMessage); }

        public delegate void InputDelegate(string blendshapeNames, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetupARKit(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(string blendshapeNames, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            try
            {
                string[] blendshapeNamesArray = blendshapeNames.Split(',');
                Debug.Log($"Blendshape names: {string.Join(", ", blendshapeNamesArray)}");
                if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                {
                    // TODO: Implement
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