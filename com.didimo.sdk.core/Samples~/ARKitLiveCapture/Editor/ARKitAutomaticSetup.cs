using Didimo.Core.Utility;
#if USING_LIVE_CAPTURE
using Unity.LiveCapture;
using Unity.LiveCapture.ARKitFaceCapture;
#endif
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using Unity.EditorCoroutines.Editor;
using System.Collections;

namespace Didimo.LiveCapture.Editor
{
    /// <summary>
    /// Class to control the LiveCapture demo scene, where it adds
    /// required components and updates the component fields.
    /// </summary>
    [ExecuteInEditMode]
    public class ARKitAutomaticSetup : MonoBehaviour
    {
        // static AddRequest Request = null;
        /// <summary>
        /// Setup didimo to use the LiveCapture by adding the FaceActor component
        /// if necessary and mapping it to the FaceDevice component
        /// </summary>
        /// 
#if !USING_LIVE_CAPTURE
        [MenuItem("CONTEXT/DidimoComponents/Didimo/Install Live Capture for ARKit Setup")]
        public static void SetupLiveCapture()
        {
            Request = Client.Add("com.unity.live-capture@1.1.0");
            EditorCoroutineUtility.StartCoroutineOwnerless(Progress());
        }
#endif

        [MenuItem("CONTEXT/DidimoComponents/Didimo/ARKit Setup", true)]
#if USING_LIVE_CAPTURE
        public static bool SetupDidimoForLiveCapture_Active() => true;
#endif
#if !USING_LIVE_CAPTURE
        public static bool SetupDidimoForLiveCapture_Active() => false;
#endif
        [MenuItem("CONTEXT/DidimoComponents/Didimo/ARKit Setup")]
#if USING_LIVE_CAPTURE
        public static void SetupDidimoForLiveCapture()
        {
            string[] guids1 = AssetDatabase.FindAssets("TakeRecorder", new[] { "Packages/com.didimo.sdk.core/Samples/ARKitLiveCapture" });

            GameObject takeRecorderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids1[0]));

            DidimoComponents didimo = Selection.activeGameObject.GetComponent<DidimoComponents>();
            

            // Update the actor mapper + clear the cache
            FaceActor faceActor =  ComponentUtility.GetOrAdd<FaceActor>(didimo.gameObject);
            faceActor.SetMapper(ScriptableObject.CreateInstance<DidimoLiveCaptureMapper>());


            string takeRecorderName = "TakeRecorder";
            if (GameObject.Find(takeRecorderName) == null)
            {
                GameObject takeRecorder = Instantiate(takeRecorderPrefab, didimo.gameObject.transform.position, didimo.gameObject.transform.rotation);

                takeRecorder.name = takeRecorderName;
            }

            // Update the scene device to point to this actor
            FaceDevice faceDevice = FindObjectOfType<FaceDevice>();
            faceDevice.Actor = faceActor;
        }
#endif
#if !USING_LIVE_CAPTURE
        static IEnumerator Progress()
        {
            EditorUtility.DisplayDialog("Live Capture Installation", "Live Capture is being installed please wait for a few seconds", "Ok");
            while (!Request.IsCompleted)
            {
                yield return null;
            }
            if (Request.Status == StatusCode.Success)
            {
                EditorUtility.RequestScriptReload();
                EditorUtility.DisplayDialog("Live Capture Installation Succeded", "Live Capture is now installed, you can setup the ARKit in our didimos now", "Ok");
            }
        }
#endif
        public static void SetupAnimation(string path)
        {
#if USING_LIVE_CAPTURE
            TakeRecorder takeRecorder = FindObjectOfType<TakeRecorder>();
            if (takeRecorder != null && path.Contains(takeRecorder.GetActiveSlate().Directory))
            {
                AnimationClip anim = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                string newpath = path.Substring(0, path.LastIndexOf('.')) + ".json";
                LiveCaptureAnimationToJsonConverter.ConvertAndSaveToFile(anim, newpath);
            }
#endif
        }
    }
}