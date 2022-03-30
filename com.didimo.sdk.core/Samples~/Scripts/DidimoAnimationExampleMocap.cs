using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Example
{
    /// <summary>
    /// Example component to play a mocap animation on your didimo.
    /// Should be attached/added to the same object where the DidimoComponents component is.
    /// </summary>
    public class DidimoAnimationExampleMocap : DidimoBehaviour
    {

        [SerializeField]
        [Tooltip("Mocap JSON file that contains the animation data.")]
        protected TextAsset mocapData;

        [SerializeField]
        [Tooltip("Toggle on to indefinitely repeat the animation")]
        protected bool loopAnimation = false;

        [SerializeField]
        [Tooltip("Play the animation as part of Unity's Start method")]
        protected bool playAutomatically = false;


        [Header("Optional")]
        [SerializeField]
        [Tooltip("Optional audio file to be played along with the animation")]
        protected AudioClip mocapAudio;


        private string playingAnimationName;


        private void Start()
        {
            if (playAutomatically)
            {
                PlaySelectedMocapAnimation();
            }
        }

        /// <summary>
        /// Play, with fade in, the selected animation mocap.
        /// If mocap audio is provided, it will be played along with the animation.
        /// This method only works in PlayMode.
        /// </summary>
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
                DidimoAnimation mocapAnimation = DidimoAnimation
                    .FromJSONContent(playingAnimationName, mocapData.text, mocapAudio);

                mocapAnimation.WrapMode = loopAnimation ? WrapMode.Loop : WrapMode.Once;
                AnimationCache.Add(playingAnimationName, mocapAnimation);
            }

            // DidimoComponents.Animator.PlayAnimation(animationID);
            DidimoComponents.Animator.FadeInAnimation(playingAnimationName, 2f);
        }

        /// <summary>
        /// Stop, using a fade out, the animation currently playing.
        /// This method only works in PlayMode and if a animation has started playing.
        /// </summary>
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
