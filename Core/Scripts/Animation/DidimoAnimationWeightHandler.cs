using System.Collections;
using Didimo;
using UnityEngine;

namespace Didimo
{
    /// <summary>
    /// Class that handles the weight for the associated <c>DidimoAnimationState</c>.
    /// This is used for fade in and fade out effects for each animation.
    /// </summary>
    public class DidimoAnimationWeightHandler
    {
        public float Weight { get; private set; }

        public bool IsNonZero => Weight > 0;
        protected DidimoAnimationState AnimState { get; }
        protected Didimo.Core.Utility.Sequence Sequence { get; }

        private float time; // for non-linear fades

        public DidimoAnimationWeightHandler(DidimoAnimationState animState)
        {
            AnimState = animState;
            Sequence = new Didimo.Core.Utility.Sequence(AnimState.Player);
        }

        /// <summary>
        /// Fade in the weight the animation.
        /// </summary>
        /// <param name="duration">Duration of the fade in, in seconds</param>
        /// <param name="linear">True to fade in linearly. False for a smooth fade in</param>
        public void FadeIn(float duration = 0.3f, bool linear = false)
        {
            Sequence.Cancel();

            // If already active, don't force the weight to 0, otherwise the animation wouldn't be smooth.
            if (!AnimState.IsActive)
            {
                Weight = 0;
                time = 0;
            }

            if (duration > 0f)
            {
                Sequence.Coroutine(FadeInRoutine(duration, linear));
            }
            else
            {
                Weight = 1;
                time = 1;
            }
        }

        /// <summary>
        /// Fade out the weight of the animation.
        /// </summary>
        /// <param name="duration">Duration of the fade out, in seconds</param>
        /// <param name="linear">True to fade out linearly. False for a smooth fade out</param>
        public void FadeOut(float duration = 0.3f, bool linear = false)
        {
            Sequence.Cancel();
            if (duration > 0f)
            {
                Sequence.Coroutine(FadeOutRoutine(duration, linear));
            }
            else
            {
                Weight = 0;
                time = 0;
                AnimState.PlayHandler.Stop();
            }
        }

        
        /// <summary>
        /// Fade in routine to increase the animation weight to 1.
        /// </summary>
        /// <param name="duration">Duration of the fade in, in seconds</param>
        /// <param name="linear">True to fade in linearly. False for a smooth fade in</param>
        private IEnumerator FadeInRoutine(float duration = 0.3f, bool linear = false)
        {
            while (Weight < 1)
            {
                if (Weight < 1)
                {
                    time += Time.deltaTime / duration;
                    Weight = linear ? time : Mathf.SmoothStep(0, 1, time);
                }
                if (Weight >= 1) Weight = 1;
                yield return null;
            }

            Weight = 1;
            time = 1;
        }
        
        
        /// <summary>
        /// Fade out routine to decrease the animation weight to 0.
        /// </summary>
        /// <param name="duration">Duration of the fade out, in seconds</param>
        /// <param name="linear">True to fade out linearly. False for a smooth fade out</param>
        private IEnumerator FadeOutRoutine(float duration = 0.3f, bool linear = false)
        {
            while (Weight > 0)
            {

                if (Weight > 0)
                {
                    time -= Time.deltaTime / duration;
                    Weight = linear ? time : Mathf.SmoothStep(0, 1, time);
                }
                if (Weight <= 0) Weight = 0;
                yield return null;
            }

            Weight = 0;
            time = 0;
            AnimState.PlayHandler.Stop();
        }
    }
}
