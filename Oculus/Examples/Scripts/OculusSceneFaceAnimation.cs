using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Oculus.Example
{
    /// <summary>
    /// Class that plays a mocap animation on the didimo.
    /// You can view this script in action on the OculusTestApplication scene.
    /// </summary>
    public class OculusSceneFaceAnimation : DidimoBehaviour
    {
#if USING_OCULUS_INTEGRATION_PACKAGE

        private const string    ANIMATION_NAME = "OculusMocap";
        public        TextAsset mocapFile;

        private void OnEnable()
        {
            if (!AnimationCache.HasAnimation(ANIMATION_NAME))
            {
                DidimoAnimation mocapAnimation = DidimoAnimation.FromJSONContent(ANIMATION_NAME, mocapFile.text);
                mocapAnimation.WrapMode = WrapMode.Loop;
                AnimationCache.Add(ANIMATION_NAME, mocapAnimation);
            }

            DidimoComponents.Animator.StopAllAnimations();
            DidimoComponents.Animator.PlayAnimation(ANIMATION_NAME);
        }

        private void OnDisable()
        {
            DidimoComponents.PoseController.ResetAll();
            DidimoComponents.Animator.StopAllAnimations();
        }
#endif
    }
}
