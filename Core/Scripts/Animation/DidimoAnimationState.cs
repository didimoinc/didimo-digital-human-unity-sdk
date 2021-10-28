using System.Collections.Generic;

namespace Didimo
{
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
        

        public DidimoAnimationState(DidimoAnimation animation, DidimoAnimator player)
        {
            SourceAnimation = animation;
            Player = player;
            PlayHandler = new DidimoAnimationPlayHandler(this);
            WeightHandler = new DidimoAnimationWeightHandler(this);
            poses = new List<DidimoAnimation.PoseData>();
        }

        public void Play()
        {
            PlayHandler.Play();
            WeightHandler.FadeIn(0f);
        }

        public void Stop()
        {
            if (PlayHandler.TimeSource == DidimoAnimationPlayHandler.AnimationTimeSource.AudioTime) Player.AudioSource.Stop();
            PlayHandler.Stop();
            WeightHandler.FadeOut(0f);
        }

        public void CalculatePoseValues(bool interpolateBetweenFrames, DidimoPoseController.ESupportedMovements supportedMovements)
        {
            if (supportedMovements.Equals(DidimoPoseController.ESupportedMovements.None)) return;
            
            if (interpolateBetweenFrames)
            {
                PlayHandler.GetInterpolatedFrames(out int initialFrameIndex, out int finalFrameIndex, out float frameWeight);
                SourceAnimation.GetAnimationPoseValues(initialFrameIndex, finalFrameIndex, frameWeight, WeightHandler.Weight, supportedMovements, ref poses, out DidimoAnimation.SkeletonData skeletonData);
                SkeletonData = skeletonData;
            }
            else
            {
                int currentFrame =  PlayHandler.GetCurrentFrame();
                SourceAnimation.GetAnimationPoseValues(currentFrame, WeightHandler.Weight, supportedMovements, ref poses, out DidimoAnimation.SkeletonData skeletonData);
                SkeletonData = skeletonData;
            }
        }

        public static DidimoAnimationState CreateInstance(DidimoAnimation animation, DidimoAnimator player) => new DidimoAnimationState(animation, player);
    }
}