#if UNITY_ANDROID || UNITY_IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Mobile.Communication
{
    public abstract class BiDirectionalNativeInterface
    {
        // Default SuccessDelegate

        public delegate void SuccessCallback(IntPtr objectPointer);

        // Default ErrorDelegate
        public delegate void ErrorCallback(IntPtr objectPointer, string msg);

        // Default ProgressDelegate
        public delegate void ProgressCallback(IntPtr objectPointer, float progress);

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
        protected static void CallOnProgress(AndroidJavaObject javaObject, float progress) { javaObject.Call("onProgress"); }

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

        public static IEnumerable<Type> GetAllNativeInterfaces()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BiDirectionalNativeInterface)));
        }

        private static void InitializeComms()
        {
#if UNITY_ANDROID
            AndroidJavaObject activity = GetMainActivity();
            AndroidJavaObject didimoUnityInterface = GetDidimoUnityInterface(activity);
#endif

            // Get all communication interfaces
            IEnumerable<Type> nativeInterfaces = GetAllNativeInterfaces();

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