using System.Collections;
using System.Collections.Generic;
using Didimo.Inspector;
using UnityEngine;

namespace Didimo.Example
{
    public class DidimoAnimationExampleMocap : DidimoBehaviour
    {
        [SerializeField]
        protected TextAsset mocapData;
        [SerializeField]
        protected bool loopAnimation = false;

        [Header("Optional")]
        [SerializeField, Tooltip("Optional audio file to be played along with the animation")]
        protected AudioClip mocapAudio;


        private string playingAnimationName;

        [Button]
        private void PlaySelectedMocapAnimation()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (mocapData == null)
            {
                Debug.LogWarning("Mocap Data not set");
                return;
            }

            playingAnimationName = mocapData.name;

            if (!AnimationCache.HasAnimation(playingAnimationName))
            {
                DidimoAnimation mocapAnimation = DidimoAnimation.FromJSONContent(playingAnimationName, mocapData.text, mocapAudio);
                mocapAnimation.WrapMode = loopAnimation ? WrapMode.Loop : WrapMode.Once;
                AnimationCache.Add(playingAnimationName, mocapAnimation);
            }

            // DidimoComponents.Animator.PlayAnimation(animationID);
            DidimoComponents.Animator.FadeInAnimation(playingAnimationName, 2f);
        }

        [Button]
        private void StopMocapAnimation()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (string.IsNullOrEmpty(playingAnimationName))
            {
                Debug.LogWarning("No animation has been played");
                return;
            }

            DidimoComponents.Animator.FadeOutAnimation(playingAnimationName, 1f);
        }
    }
}