using System;
using System.Collections;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo
{

    /// <summary>
    /// Class that handles the time and frame number for the associated <c>DidimoAnimationState</c>.
    /// This is used to handle play, resume and stop state of the animations.
    /// </summary>
    public class DidimoAnimationPlayHandler
    {
        public enum PlayStatus
        {
            None,
            Playing,
            Seek,
            Stopped
        }

        public enum PlayDirection
        {
            Forward,
            Backward
        }

        public enum AnimationTimeSource
        {
            InternalTime,
            AudioTime
        }

        public enum FrameCalculationSource
        {
            FPS,
            Timestamps
        }

        protected PlayDirection direction = PlayDirection.Forward;

        public AnimationTimeSource    TimeSource  = AnimationTimeSource.InternalTime;
        public FrameCalculationSource FrameSource = FrameCalculationSource.FPS;

        public double SeekTime { get; private set; }

        public double Speed { get; set; } = 1;

        public PlayStatus Status { get; private set; } = PlayStatus.Stopped;
        public PlayDirection Direction => direction;

        public bool IsStopped => Status == PlayStatus.Stopped;
        public bool IsPlaying => Status == PlayStatus.Playing;

        public double NormalizedTime
        {
            get => AnimState.SourceAnimation.TotalAnimationTime == 0 ? 0
                : SeekTime / AnimState.SourceAnimation.TotalAnimationTime;
            private set => SeekTime = value * AnimState.SourceAnimation.TotalAnimationTime;
        }

        protected DidimoAnimationState AnimState { get; }

        protected WrapMode WrapMode => AnimState.SourceAnimation.WrapMode;
        protected double TotalAnimationTime => AnimState.SourceAnimation.TotalAnimationTime;

        protected float DirectionMultiplier => direction == PlayDirection.Forward ? 1 : -1;

        private Didimo.Core.Utility.Sequence Sequence { get; }

        public DidimoAnimationPlayHandler(DidimoAnimationState animState)
        {
            AnimState = animState;
            Sequence = new Sequence(animState.Player);
        }

        /// <summary>
        /// Start or continue playing the animation.
        /// </summary>
        /// <param name="resume">True to resume the animation from where it last stopped. False to restart.</param>
        public void Play(bool resume = false)
        {
            if (!resume)
            {
                SeekTime = 0;
            }

            Sequence.Cancel();
            Sequence.Coroutine(PlayRoutine());

            Status = PlayStatus.Playing;
        }

        /// <summary>
        /// Stop playing the animation. The animation time is not changed so it can be resumed later.
        /// </summary>
        public void Stop()
        {
            // SeekTime = 0;
            Sequence.Cancel();
            Status = PlayStatus.Stopped;
        }

        /// <summary>
        /// Change the animation time to the specified <paramref name="seekTime"/> value.
        /// </summary>
        /// <param name="seekTime">Time of the animation, in seconds.</param>
        public void Seek(float seekTime)
        {
            SeekTime = seekTime;
            HandleWrapping();
        }

        /// <summary>
        /// Change the normalized animation time to the specified <paramref name="normalisedSeekTime"/> value.
        /// </summary>
        /// <param name="normalisedSeekTime">Normalized value of the animation.</param>
        public void SeekNormalized(float normalisedSeekTime)
        {
            NormalizedTime = normalisedSeekTime;
            HandleWrapping();
        }

        /// <summary>
        /// Handle the wrapping of the animation for each WrapMode that is available.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Unhandled WrapMode set.</exception>
        private void HandleWrapping()
        {
            switch (WrapMode)
            {
                case WrapMode.Default:
                case WrapMode.Once:
                    if (SeekTime > TotalAnimationTime && direction == PlayDirection.Forward
                        || SeekTime < 0 && direction == PlayDirection.Backward)
                    {
                        AnimState.Stop();
                    }

                    break;
                case WrapMode.Loop:
                    if (SeekTime > TotalAnimationTime)
                    {
                        SeekTime -= TotalAnimationTime;
                    }

                    if (SeekTime < 0)
                    {
                        SeekTime += TotalAnimationTime;
                    }

                    break;
                case WrapMode.PingPong:
                    if (SeekTime > TotalAnimationTime)
                    {
                        direction = PlayDirection.Backward;
                        SeekTime = TotalAnimationTime - (SeekTime - TotalAnimationTime);
                    }

                    if (SeekTime < 0)
                    {
                        direction = PlayDirection.Forward;
                        SeekTime = Math.Abs(SeekTime);
                    }

                    break;
                case WrapMode.ClampForever:
                    if (SeekTime > TotalAnimationTime)
                    {
                        SeekTime = TotalAnimationTime;
                    }

                    if (SeekTime < 0)
                    {
                        SeekTime = 0;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Set the time source if an AudioClip is available and keep
        /// updating the time of the animation until it has ended.
        /// </summary>
        private IEnumerator PlayRoutine()
        {
            if (TimeSource == AnimationTimeSource.AudioTime)
            {
                AnimState.Player.AudioSource.clip = AnimState.SourceAnimation.AudioClip;
                AnimState.Player.AudioSource.Play();
            }

            while (true)
            {
                yield return null;
                UpdateTime();
            }
        }

        /// <summary>
        /// Update the time of the animation through the AudioClip or Unity's deltaTime.
        /// </summary>
        /// <exception cref="ArgumentException"><c>TimeSource</c> value not handled.</exception>
        private void UpdateTime()
        {
            switch (TimeSource)
            {
                case AnimationTimeSource.AudioTime:
                    SeekTime = AnimState.Player.AudioSource.time;
                    if (SeekTime >= AnimState.Player.AudioSource.clip.length
                      || SeekTime >= AnimState.SourceAnimation.TotalAnimationTime) AnimState.Stop();
                    return;
                case AnimationTimeSource.InternalTime:
                    SeekTime += Time.deltaTime * DirectionMultiplier * Speed;
                    HandleWrapping();
                    return;
                default:
                    throw new ArgumentException("Invalid enum value", nameof(TimeSource));
            }
        }

        /// <summary>
        /// Get the frame number that corresponds to the current SeekTime.
        /// </summary>
        /// <returns>Frame index of the animation.</returns>
        /// <exception cref="ArgumentException"><c>FrameSource</c> value not handled.</exception>
        public int GetCurrentFrame()
        {
            switch (FrameSource)
            {
                case FrameCalculationSource.FPS:
                    return AnimState.SourceAnimation.GetFrameNumberFromFPS((float) SeekTime);
                case FrameCalculationSource.Timestamps:
                    return AnimState.SourceAnimation.GetFrameNumberFromTimestamps((float) SeekTime);
                default:
                    throw new ArgumentException("Invalid enum value", nameof(FrameSource));
            }
        }

        /// <summary>
        /// Get the interpolated frame numbers and interpolation weight that
        /// correspond to the current SeekTime.
        /// </summary>
        /// <param name="initialFrameIndex">Initial frame that matches the given time</param>
        /// <param name="finalFrameIndex">Final frame that matches the given time</param>
        /// <param name="weight">Interpolation weight between the initial and final frames</param>
        /// <exception cref="ArgumentException"><c>FrameSource</c> value not handled.</exception>
        public void GetInterpolatedFrames(out int initialFrameIndex, out int finalFrameIndex, out float weight)
        {
            switch (FrameSource)
            {
                case FrameCalculationSource.FPS:
                    AnimState.SourceAnimation.GetInterpolatedFrameNumbersFromFPS(
                        (float) SeekTime, out initialFrameIndex, out finalFrameIndex, out weight);
                    return;
                case FrameCalculationSource.Timestamps:
                    AnimState.SourceAnimation.GetInterpolatedFrameNumbersFromTimestamps(
                        (float) SeekTime, out initialFrameIndex, out finalFrameIndex, out weight);
                    return;
                default:
                    throw new ArgumentException("Invalid enum value", nameof(FrameSource));
            }
        }
    }
}
