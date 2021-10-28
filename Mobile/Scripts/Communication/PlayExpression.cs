#if UNITY_IOS || UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public class PlayExpression : BiDirectionalNativeInterface
    {
#if UNITY_ANDROID
        private class MessageInterface : AndroidJavaProxy
        {
            public MessageInterface() : base("com.unity3d.communication.DidimoUnityInterface$PlayExpressionInterface") { }

            public void sendToUnity(string didimoKey, string animationID, AndroidJavaObject response)
            {
                CbMessage(IntPtr.Zero,
                    didimoKey,
                    animationID,
                    obj =>
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
        protected override void RegisterNativeCall() { registerPlayExpression(CbMessage); }

        public delegate void InputDelegate(IntPtr obj, string didimoKey, string animationID, SuccessDelegate successDelegate, ErrorDelegate errorDelegate);

        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        private static extern void registerPlayExpression(InputDelegate cb);

        [MonoPInvokeCallback(typeof(InputDelegate))]
#endif
        private static void CbMessage(IntPtr obj, string didimoKey, string animationID, SuccessDelegate successDelegate, ErrorDelegate errorDelegate)
        {
            ThreadingUtility.WhenMainThread(() =>
            {
                try
                {
                    if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
                    {
                        didimo.Animator.PlayExpression(animationID);
                    }
                    else
                    {
                        throw new Exception($"Failed to locate didimo with ID {didimoKey}");
                    }

                    successDelegate(obj);
                }
                catch (Exception e)
                {
                    errorDelegate(obj, e.Message);
                }
            });
        }
    }
}
#endif