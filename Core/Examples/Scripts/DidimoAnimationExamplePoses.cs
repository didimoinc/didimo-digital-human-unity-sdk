using System.Collections;
using System.Collections.Generic;
using Didimo.Inspector;
using UnityEngine;

namespace Didimo.Example
{
    public class DidimoAnimationExamplePoses : DidimoBehaviour
    {
        [SerializeField]
        private bool updatePose;

        [SerializeField]
        private string poseName;

        [Range(0f, 1f)]
        [SerializeField]
        private float poseValue;

        [Button]
        private void SetPose()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (string.IsNullOrEmpty(poseName)) return;
            DidimoComponents.PoseController.SetWeightForPose(poseName, poseValue);
            if (!Application.isPlaying) ((LegacyAnimationPoseController) DidimoComponents.PoseController).ForceUpdateAnimation();
        }

        [Button]
        private void ResetPoses()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            DidimoComponents.PoseController.ResetAll();
            if (!Application.isPlaying) ((LegacyAnimationPoseController) DidimoComponents.PoseController).ForceUpdateAnimation();
        }

        private void Update()
        {
            if (updatePose) SetPose();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && updatePose) SetPose();
        }
    }
}