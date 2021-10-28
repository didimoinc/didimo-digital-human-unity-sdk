#if UNITY_ANDROID || UNITY_IOS
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Didimo.Mobile.Scripts
{
    public abstract class BiDirectionalNativeInterface
    {
        // Default SuccessDelegate

        public delegate void SuccessDelegate(IntPtr obj);

        // Default ErrorDelegate
        public delegate void ErrorDelegate(IntPtr obj, string msg);

#if UNITY_IOS
        protected abstract void RegisterNativeCall();

#elif UNITY_ANDROID
        protected abstract void RegisterNativeCall(AndroidJavaObject activity);

        private static AndroidJavaObject GetMainActivity()
        {
            string mainActivityStr = "com.unity3d.player.CommunicationActivity";
            AndroidJavaClass mainActivity = new AndroidJavaClass(mainActivityStr);
            AndroidJavaObject mainActivityObj = mainActivity.CallStatic<AndroidJavaObject>("LastInstance");

            return mainActivityObj;
        }

        protected static void CallOnSuccess(AndroidJavaObject javaObject) { javaObject.Call("onSuccess"); }

        protected static void CallOnError(AndroidJavaObject javaObject, string message) { javaObject.Call("onError", message); }

        private static AndroidJavaObject GetDidimoUnityInterface(AndroidJavaObject activity) => activity.Call<AndroidJavaObject>("getDidimoUnityInterface");
#endif
        [RuntimeInitializeOnLoadMethod]
        private static void OnRuntimeMethodLoad()
        {
            // Register with native side
#if !UNITY_EDITOR
			InitializeComms();
#endif
        }

        private static readonly HashSet<Type> nativeInterfaces = new HashSet<Type>
        {
            typeof(BuildDidimoFromDirectory),
            typeof(CacheAnimation),
            typeof(ClearAnimationCache),
            typeof(DestroyDidimo),
            typeof(PlayExpression),
            typeof(ResetCamera),
            typeof(ResetCamera),
            typeof(SetCamera),
            typeof(SetEyeColor),
            typeof(SetHairColor),
            typeof(SetHairstyle),
            typeof(SetOrbitControls),
            typeof(TextToSpeech),
            typeof(UpdateDeformable),
            typeof(GetDeformableData),
            typeof(GetCameraFrameImage)
        };

        private static void InitializeComms()
        {
#if UNITY_ANDROID
            AndroidJavaObject activity = GetMainActivity();
            AndroidJavaObject didimoUnityInterface = GetDidimoUnityInterface(activity);
#endif
            foreach (Type nativeInterfaceType in nativeInterfaces)
            {
                BiDirectionalNativeInterface instance = (BiDirectionalNativeInterface) Activator.CreateInstance(nativeInterfaceType);
#if UNITY_IOS
                instance.RegisterNativeCall();
#elif UNITY_ANDROID
                instance.RegisterNativeCall(didimoUnityInterface);
#endif
            }
        }
    }
}
#endif