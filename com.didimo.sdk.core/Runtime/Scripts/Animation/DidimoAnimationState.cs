using System.Collections.Generic;

namespace Didimo
{
    /// <summary>
    /// Class that holds that instance/state of a <c>DidimoAnimation</c> to be played by any
    /// didimo using the <c>DidimoAnimator</c> component. This keeps track of the current time and weight
    /// for this specific animation instance.
    /// </summary>
    public class DidimoAnimationState
    {
        private List<DidimoAnimation.PoseData> poses;

        public DidimoAnimationPlayHandler PlayHandler { get; }
        public DidimoAnimationWeightHandler WeightHandler { get; }

        public DidimoAnimation SourceAnimation { get; }
        public DidimoAnimator Player { get; }


        public IReadOnlyList<DidimoAnimation.PoseData> Poses => poses;
        public DidimoAnimation.SkeletonData SkeletonData { get; private set; }

        public bool IsActive => PlayHandler.IsPlaying || WeightHandler.IsNonZero;


        /// <summary>
        /// Build an animation state from the source animation data
        /// and the associated <c>DidimoAnimator</c> component.
        /// </summary>
        /// <param name="animation">Source animation data.</param>
        /// <param name="player">Component of the didimo that will play the animation.</param>
        public DidimoAnimationState(DidimoAnimation animation, DidimoAnimator player)
        {
            SourceAnimation = animation;
            Player = player;
            PlayHandler = new DidimoAnimationPlayHandler(this);
            WeightHandler = new DidimoAnimationWeightHandler(this);
            poses = new List<DidimoAnimation.PoseData>();
        }

        /// <summary>
        /// Start playing this animation with no fade in effect.
        /// </summary>
        public void Play()
        {
            PlayHandler.Play();
            WeightHandler.FadeIn(0f);
        }

        /// <summary>
        /// Stop playing this animation with no fade out effect.
        /// </summary>
        public void Stop()
        {
            if (PlayHandler.TimeSource == DidimoAnimationPlayHandler.AnimationTimeSource.AudioTime)
            {
                Player.AudioSource.Stop();
            }
            PlayHandler.Stop();
            WeightHandler.FadeOut(0f);
        }

        /// <summary>
        /// Calculate the pose weight values and skeleton transforms (if available) for the animation.
        /// For a more accurate animation or for effects such as slow-mo, enable
        /// <paramref name="interpolateBetweenFrames"/>
        /// on the <c>DidimoAnimator</c> component of your didimo.
        /// </summary>
        /// <param name="interpolateBetweenFrames">Calculate the poses using
        /// interpolation to get the pose values between frames.</param>
        /// <param name="supportedMovements">Supported skeleton movements for
        /// this didimo and <c>PoseController</c></param>
        public void CalculatePoseValues(bool interpolateBetweenFrames,
            DidimoPoseController.ESupportedMovements supportedMovements)
        {
            if (supportedMovements.Equals(DidimoPoseController.ESupportedMovements.None))
            {
                return;
            }

            if (interpolateBetweenFrames)
            {
                PlayHandler.GetInterpolatedFrames(out int initialFrameIndex,
                    out int finalFrameIndex, out float frameWeight);
                SourceAnimation.GetAnimationPoseValues(initialFrameIndex,
                    finalFrameIndex, frameWeight, WeightHandler.Weight,
                    supportedMovements, ref poses, out DidimoAnimation.SkeletonData skeletonData);
                SkeletonData = skeletonData;
            }
            else
            {
                int currentFrame =  PlayHandler.GetCurrentFrame();
                SourceAnimation.GetAnimationPoseValues(currentFrame, WeightHandler.Weight,
                    supportedMovements, ref poses, out DidimoAnimation.SkeletonData skeletonData);
                SkeletonData = skeletonData;
            }
        }

        /// <summary>
        /// Create an instance/state of an animation to be played from a didimo <paramref name="animation"/>.
        /// </summary>
        /// <param name="animation">Source animation data.</param>
        /// <param name="player">Component of the didimo that will play the animation.</param>
        /// <returns>Created <c>DidimoAnimationState</c> instance of the source animation.</returns>
        public static DidimoAnimationState CreateInstance(DidimoAnimation animation, DidimoAnimator player)
        {
            return new DidimoAnimationState(animation, player);
        }
    }
}