#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Core.Deformables;

namespace Didimo.Mobile.Communication
{
    public class SetHairColor : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$SetHairColorInterface") { }

            public void sendToUnity(string didimoKey, int colorPresetID, AndroidJavaObject response)
            {
                CbMessage(colorPresetID,
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
            }
        }

        protected override void RegisterNativeCall(AndroidJavaObject didimoUnityInterface) { didimoUnityInterface.Call("RegisterForCommunication", new MessageInterface()); }

#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerSetHairColor(CbMessage); }

        public delegate void InputDelegate(int colorPresetId, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerSetHairColor(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(int colorPresetID, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        if (didimo.Deformables.TryFind(out Hair hair))
                        {
                            hair.SetPreset(colorPresetID);
                        }
                        else
                        {
                            throw new Exception($"Unable to find deformable on didimo with id {didimoKey}");
                        }
                    }
                    else
                    {
                        throw new Exception($"Unable to find didimo with id {didimoKey}");
                    }

                    successCallback(objectPointer);
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