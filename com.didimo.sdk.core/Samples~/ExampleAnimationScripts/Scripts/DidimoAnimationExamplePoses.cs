using System.Collections;
using System.Collections.Generic;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Example
{
    /// <summary>
    /// Example component to trigger a specific pose on your didimo.
    /// Should be attached/added to the same object where the DidimoComponents component is.
    /// </summary>
    public class DidimoAnimationExamplePoses : DidimoBehaviour
    {
        [SerializeField]
        [Tooltip("Continuously update the pose in real-time without needing to press the 'Set Pose' button")]
        private bool updatePose;

        [SerializeField]
        [Tooltip("Name of the pose to trigger. Should match the animation clip name, e.g.: ARKit_eyeBlinkLeft")]
        private string poseName;

        [Range(0f, 1f)]
        [SerializeField]
        [Tooltip("Weight of the pose to trigger.")]
        private float poseValue;

        /// <summary>
        /// Set the weight for any facial pose. The name must exactly match
        /// the animation clip name for it to trigger.
        /// If updatePose is enabled, this function is continually called.
        /// This method works both in PlayMode and EditMode.
        /// </summary>
        [Button]
        private void SetPose()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (string.IsNullOrEmpty(poseName))
            {
                return;
            }

            DidimoComponents.PoseController.SetWeightForPose(poseName, poseValue);
            if (!Application.isPlaying)
            {
                ((LegacyAnimationPoseController) DidimoComponents.PoseController).ForceUpdateAnimation();
            }
        }

        /// <summary>
        /// Reset all the pose weights to their initial value.
        /// This returns the face back to its original neutral expression.
        /// </summary>
        [Button]
        private void ResetPoses()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            DidimoComponents.PoseController.ResetAll();
            if (!Application.isPlaying)
            {
                ((LegacyAnimationPoseController) DidimoComponents.PoseController).ForceUpdateAnimation();
            }
        }

        private void Update()
        {
            if (updatePose)
            {
                SetPose();
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying && updatePose)
            {
                SetPose();
            }
        }
    }
}