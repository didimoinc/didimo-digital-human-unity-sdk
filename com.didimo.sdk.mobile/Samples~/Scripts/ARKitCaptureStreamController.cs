using System;
using System.Collections;
using System.Collections.Generic;
using Didimo;
using UnityEngine;

namespace Didimo.Mobile
{
    public class ARKitCaptureStreamController : MonoBehaviour
    {
        private string[]         poseNames;
        private DidimoComponents components;

        public static Dictionary<string, ARKitCaptureStreamController> controllers = new Dictionary<string, ARKitCaptureStreamController>();

        private void Awake() { components = GetComponent<DidimoComponents>(); }

        public void StreamValues(float[] values)
        {
            if (poseNames == null)
            {
                throw new Exception("Must first register pose names with method RegisterPoseNames.");
            }

            if (values.Length != poseNames.Length)
            {
                throw new Exception("Pose names doesn't match number of values provided.");
            }

            for (int i = 0; i < values.Length; i++)
            {
                components.PoseController.SetWeightForPose("ARKit", poseNames[i], values[i]);
            }
        }

        public void RegisterPoseNames(string[] pn) { poseNames = pn; }

        public static ARKitCaptureStreamController AddToDidimo(string didimoKey)
        {
            if (DidimoCache.TryFindDidimo(didimoKey, out DidimoComponents didimo))
            {
                ARKitCaptureStreamController controller = Didimo.Core.Utility.ComponentUtility.GetOrAdd<ARKitCaptureStreamController>(didimo.gameObject);
                controllers.Add(didimoKey, controller);
                return controller;
            }
            else
            {
                throw new Exception($"Unable to find didimo with id {didimoKey}");
            }
        }

        public static ARKitCaptureStreamController GetForDidimo(string didimoKey) { return controllers[didimoKey]; }

        public static bool RemoveForDidimo(string didimoKey) { return controllers.Remove(didimoKey); }
    }
}