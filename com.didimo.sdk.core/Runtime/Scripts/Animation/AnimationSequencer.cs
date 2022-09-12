using Didimo.Core.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Didimo.Core.Animation
{
    /// <summary>
    /// Example component that can be used to loop and fade between multiple different animations on your didimo.
    /// Should be attached/added to the same object where the DidimoComponents component is.
    /// </summary>
    public class AnimationSequencer : DidimoBehaviour
    {
        private const float MIN_ANIM_DURATION = 0f;

        [SerializeField]
        private List<TextAsset> MocapAnimations = new List<TextAsset>();

        private List<DidimoAnimation> didimoAnimations;

        [SerializeField]
        [Tooltip("Minimum duration of an animation.")]
        private float minAnimationDuration = 2;

        [SerializeField]
        [Tooltip("Maximum duration of an animation. If 0, will play the whole animation and then will fade to the next (random) one.")]
        private float maxAnimationDuration = 5;

        [SerializeField]
        [Tooltip("Duration of the fade between animations.")]
        private float crossFadeDuration = 0.3f;

        private float currentTime       = 0;
        private float animationDuration = 0;
        private int   currentAnimation  = -1;

        public void AddMocapAnimation(TextAsset t)
        {
            MocapAnimations.Add(t);
        }

        private void Start()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning($"DidimoAnimation component not found. Disabling {GetType().Name}.");
                enabled = false;
                return;
            }

            didimoAnimations = new List<DidimoAnimation>();
            foreach (TextAsset mocapAnimationJson in MocapAnimations)
            {
                if (!AnimationCache.TryGet(mocapAnimationJson.name, out DidimoAnimation mocapAnimation))
                {
                    mocapAnimation = DidimoAnimation.FromJSONContent(mocapAnimationJson.name, mocapAnimationJson.text);
                    mocapAnimation.WrapMode = WrapMode.Loop;
                    AnimationCache.Add(mocapAnimationJson.name, mocapAnimation);
                }
                didimoAnimations.Add(mocapAnimation);
            }
        }

        private void Update()
        {
            if (currentTime >= animationDuration)
            {
                currentTime = 0;
                if (currentAnimation != -1)
                {
                    DidimoComponents.Animator.FadeOutAnimation(MocapAnimations[currentAnimation].name, crossFadeDuration);
                }

                currentAnimation = Random.Range(0, didimoAnimations.Count);
                if (maxAnimationDuration > 0)
                {
                    animationDuration = Random.Range(minAnimationDuration, maxAnimationDuration);
                }
                else
                {
                    animationDuration = (float) didimoAnimations[currentAnimation].TotalAnimationTime;
                }
                
                DidimoComponents.Animator.FadeInAnimation(MocapAnimations[currentAnimation].name, crossFadeDuration, true);
            }

            currentTime += Time.deltaTime;
        }

        private void OnValidate()
        {
            if (minAnimationDuration <= MIN_ANIM_DURATION)
            {
                minAnimationDuration = MIN_ANIM_DURATION;
            }

            if (maxAnimationDuration <= MIN_ANIM_DURATION)
            {
                maxAnimationDuration = MIN_ANIM_DURATION;
            }

            if (minAnimationDuration > maxAnimationDuration)
            {
                minAnimationDuration = maxAnimationDuration;
            }
        }
    }
}