#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public class StreamARKit : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$StreamARKitInterface") { }

            public void sendToUnity(float[] blendshapeWeights, string didimoKey, AndroidJavaObject response)
            {
                CbMessage(blendshapeWeights,
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
        private static void CbMessage(float[] blendshapeWeights, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
#elif UNITY_IOS
        protected override void RegisterNativeCall() { registerStreamARKit(CbMessage); }

        public delegate void InputDelegate(IntPtr blendshapeWeights, int blendshapeCount, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerStreamARKit(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
        private static void CbMessage(IntPtr blendshapeWeightsPtr, int blendshapeCount, string didimoKey, SuccessCallback successCallback, ErrorCallback errorCallback, IntPtr objectPointer)
        {
            float[] blendshapeWeights = new float[blendshapeCount];
            Marshal.Copy(blendshapeWeightsPtr, blendshapeWeights, 0, blendshapeCount);
#endif
        
            try
            {
                Debug.Log($"Blendshape weights: {string.Join(", ", blendshapeWeights)}");

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