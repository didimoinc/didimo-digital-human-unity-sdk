using System.Collections;
using DigitalSalmon;
using UnityEngine;

namespace Didimo
{
    public class DidimoAnimationWeightHandler
    {
        public float Weight { get; private set; }

        public bool IsNonZero => Weight > 0;
        protected DidimoAnimationState AnimState { get; }
        protected Sequence Sequence { get; }

        private float time; // for non-linear fades

        public DidimoAnimationWeightHandler(DidimoAnimationState animState)
        {
            AnimState = animState;
            Sequence = new Sequence(AnimState.Player);
        }

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
    }
}