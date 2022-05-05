using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Didimo.Timeline
{
    [Serializable]
    public class DidimoAnimatorBehaviour : PlayableBehaviour
    {
        public  TextAsset      animationJson;
        private DidimoAnimator didimoAnimator;

        private TextAsset previousAnimationJson;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //base.ProcessFrame(playable, info, playerData);
            if (animationJson == null || string.IsNullOrEmpty(animationJson.text))
            {
                return;
            }

            didimoAnimator = playerData as DidimoAnimator;

            if (didimoAnimator == null)
            {
                return;
            }

            if (previousAnimationJson != null && previousAnimationJson != animationJson)
            {
                didimoAnimator.RemoveAnimation(previousAnimationJson.name);
            }

            if (!AnimationCache.HasAnimation(animationJson.name))
            {
                AnimationCache.Add(animationJson.name, DidimoAnimation.FromJSONContent(animationJson.name, animationJson.text));
            }

            didimoAnimator.interpolateBetweenFrames = false;
            didimoAnimator.PlayAnimation(animationJson.name);
            didimoAnimator.SetAnimationTime(animationJson.name, (float) playable.GetTime());

            if (!Application.isPlaying)
            {
                didimoAnimator.UpdatePose();
                didimoAnimator.DidimoComponents.PoseController.ForceUpdateAnimation();
                // No need to graph evaluate, as the timeline will do it
            }

            previousAnimationJson = animationJson;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            //base.OnBehaviourPause(playable, info);
            if (didimoAnimator == null)
            {
                return;
            }

            didimoAnimator.StopAllAnimations();
            didimoAnimator.DidimoComponents.PoseController.ResetAll();
            // _didimoAnimator.UpdatePose();
            // Need to call evaluate manually, because the timeline is no more and won't do it for us
            didimoAnimator.DidimoComponents.PoseController.ForceUpdateAnimation();
        }
    }
}