using System;
using System.Collections;
using DigitalSalmon;
using UnityEngine;

namespace Didimo
{
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
            get => AnimState.SourceAnimation.TotalAnimationTime == 0 ? 0 : SeekTime / AnimState.SourceAnimation.TotalAnimationTime;
            private set => SeekTime = value * AnimState.SourceAnimation.TotalAnimationTime;
        }

        protected DidimoAnimationState AnimState { get; }

        protected WrapMode WrapMode => AnimState.SourceAnimation.WrapMode;
        protected double TotalAnimationTime => AnimState.SourceAnimation.TotalAnimationTime;

        protected float DirectionMultiplier => direction == PlayDirection.Forward ? 1 : -1;

        private Sequence Sequence { get; }

        public DidimoAnimationPlayHandler(DidimoAnimationState animState)
        {
            AnimState = animState;
            Sequence = new Sequence(animState.Player);
        }

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

        public void Stop()
        {
            // SeekTime = 0;
            Sequence.Cancel();
            Status = PlayStatus.Stopped;
        }

        public void Seek(float seekTime)
        {
            SeekTime = seekTime;
            HandleWrapping();
        }

        public void SeekNormalized(float normalisedSeekTime)
        {
            NormalizedTime = normalisedSeekTime;
            HandleWrapping();
        }

        private void HandleWrapping()
        {
            switch (WrapMode)
            {
                case WrapMode.Default:
                case WrapMode.Once:
                    if (SeekTime > TotalAnimationTime && direction == PlayDirection.Forward || SeekTime < 0 && direction == PlayDirection.Backward)
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

        private void UpdateTime()
        {
            switch (TimeSource)
            {
                case AnimationTimeSource.AudioTime:
                    SeekTime = AnimState.Player.AudioSource.time;
                    if (SeekTime >= AnimState.Player.AudioSource.clip.length || SeekTime >= AnimState.SourceAnimation.TotalAnimationTime) AnimState.Stop();
                    return;
                case AnimationTimeSource.InternalTime:
                    SeekTime += Time.deltaTime * DirectionMultiplier * Speed;
                    HandleWrapping();
                    return;
                default:
                    throw new ArgumentException("Invalid enum value", nameof(TimeSource));
            }
        }

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

        public void GetInterpolatedFrames(out int initialFrameIndex, out int finalFrameIndex, out float weight)
        {
            switch (FrameSource)
            {
                case FrameCalculationSource.FPS:
                    AnimState.SourceAnimation.GetInterpolatedFrameNumbersFromFPS((float) SeekTime, out initialFrameIndex, out finalFrameIndex, out weight);
                    return;
                case FrameCalculationSource.Timestamps:
                    AnimState.SourceAnimation.GetInterpolatedFrameNumbersFromTimestamps((float) SeekTime, out initialFrameIndex, out finalFrameIndex, out weight);
                    return;
                default:
                    throw new ArgumentException("Invalid enum value", nameof(FrameSource));
            }
        }
    }
}