using System.Collections.Generic;
using System.Linq;
using DigitalSalmon.Extensions;
using UnityEngine;

namespace Didimo
{
    public class DidimoAnimator : DidimoBehaviour
    {
        private readonly Dictionary<string, DidimoAnimationState> idToAnimationInstance = new Dictionary<string, DidimoAnimationState>();
        private readonly List<DidimoAnimationState>               animationInstances    = new List<DidimoAnimationState>();

        private readonly DidimoAnimationBlender didimoAnimationBlender = new DidimoAnimationBlender();

        private AudioSource _audioSource;

        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                    if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
                    _audioSource.loop = false;
                }

                return _audioSource;
            }
        }

        public bool interpolateBetweenFrames;

        public bool IsPlaying => animationInstances.Any(a => a.IsActive);

        public void Update()
        {
            if (!DidimoComponents.PoseController.SupportsAnimation)
            {
                Debug.LogWarning("Disabling DidimoAnimator as didimo PoseController does not support animation.");
                enabled = false;
                return;
            }

            if (!IsPlaying) return;
            UpdatePose();
        }

        public void PlayExpression(string id)
        {
            if (id == "None") FadeOutAllAnimations();
            else CrossFadeAnimation(id);
        }

        /// <summary>
        /// Stops all other animations, and plays the animation with the given name
        /// </summary>
        /// <param name="id">The name of the animation to play</param>
        public void PlayAnimation(string id)
        {
            if (TryGetAnimation(id, out DidimoAnimationState animState))
            {
                PlayAnimation(animState);
            }
        }

        public bool FadeInAnimation(string id, float fadeTime = 0.3f, bool resume = false, bool linear = false)
        {
            if (!TryGetAnimation(id, out DidimoAnimationState animState)) return false;

            SetAnimationSources(animState);

            animState.WeightHandler.FadeIn(fadeTime, linear);
            animState.PlayHandler.Play(resume);
            
            return true;
        }

        public bool FadeOutAnimation(string id, float fadeTime = 0.3f, bool linear = false)
        {
            if (!TryGetAnimation(id, out DidimoAnimationState animState)) return false;
            animState.WeightHandler.FadeOut(fadeTime);

            return true;
        }

        public bool CrossFadeAnimation(string id, float fadeTime = 0.3f, bool resume = false, bool linear = false)
        {
            if (!FadeInAnimation(id, fadeTime, resume, linear)) return false;

            FadeOutAllAnimations(fadeTime, linear);

            return true;
        }

        public void SetAnimationTime(string id, float time)
        {
            if (TryGetAnimation(id, out DidimoAnimationState animState))
            {
                animState.PlayHandler.Seek(time);
            }
        }

        public void SetNormalizedTime(string id, float normalizedTime)
        {
            if (TryGetAnimation(id, out DidimoAnimationState animState))
            {
                animState.PlayHandler.SeekNormalized(normalizedTime);
            }
        }

        public void StopAllAnimations()
        {
            foreach (DidimoAnimationState animState in idToAnimationInstance.Values)
            {
                animState.WeightHandler.FadeOut(0);
            }
        }

        public void RemoveAnimation(string animationId)
        {
            DidimoAnimationState animationState = idToAnimationInstance[animationId];
            if (animationState != null)
            {
                idToAnimationInstance.Remove(animationId);
                animationInstances.Remove(animationState);
                Resources.UnloadUnusedAssets(); // This is probably useless, but leaving it in case.
            }
        }

        public void RemoveAllAnimations()
        {
            idToAnimationInstance.Clear();
            animationInstances.Clear();
            Resources.UnloadUnusedAssets(); // This is probably useless, but leaving it in case.
        }

        public void UpdatePose()
        {
            if (animationInstances.Count == 0)
            {
                return;
            }

            didimoAnimationBlender.Clear();
            foreach (DidimoAnimationState animInstance in animationInstances)
            {
                // Ignore animations that are not actively doing anything.
                if (!animInstance.PlayHandler.IsPlaying || !animInstance.WeightHandler.IsNonZero) continue;  
                
                animInstance.CalculatePoseValues(interpolateBetweenFrames, DidimoComponents.PoseController.SupportedMovements);
                didimoAnimationBlender.AddPoseData(animInstance.Poses);
                if (animInstance.SkeletonData != null) didimoAnimationBlender.AddSkeletonData(animInstance.SkeletonData);
            }

            foreach (DidimoAnimation.PoseData poseData in didimoAnimationBlender.ResolvePoseData())
            {
                DidimoComponents.PoseController.SetWeightForPose(poseData.Name, poseData.Value);
            }

            DidimoAnimation.SkeletonData resolvedSkeleton = didimoAnimationBlender.ResolveSkeletonData();
            DidimoComponents.PoseController.SetHeadPosition(resolvedSkeleton.HeadPosition);
            DidimoComponents.PoseController.SetHeadRotation(resolvedSkeleton.HeadRotation);
            DidimoComponents.PoseController.SetLeftEyeRotation(resolvedSkeleton.LeftEyeRotation);
            DidimoComponents.PoseController.SetRightEyeRotation(resolvedSkeleton.RightEyeRotation);
        }

        private bool TryGetAnimation(string id, out DidimoAnimationState state)
        {
            if (idToAnimationInstance.TryGetValue(id, out state))
            {
                return true;
            }

            if (!AnimationCache.TryGetInstance(id, out DidimoAnimation sourceAnimation))
            {
                Debug.LogWarning($"No cached animation with id {id}");
                return false;
            }

            state = DidimoAnimationState.CreateInstance(sourceAnimation, this);
            idToAnimationInstance.Add(id, state);
            animationInstances.Add(state);
            return true;
        }

        private void SetAnimationSources(DidimoAnimationState animationState)
        {
            animationState.PlayHandler.FrameSource = animationState.SourceAnimation.Timestamps.IsNullOrEmpty()
                ? DidimoAnimationPlayHandler.FrameCalculationSource.FPS
                : DidimoAnimationPlayHandler.FrameCalculationSource.Timestamps;

            animationState.PlayHandler.TimeSource = animationState.SourceAnimation.AudioClip == null
                ? DidimoAnimationPlayHandler.AnimationTimeSource.InternalTime
                : DidimoAnimationPlayHandler.AnimationTimeSource.AudioTime;
        }
        
        private void PlayAnimation(DidimoAnimationState animationState)
        {
            foreach (DidimoAnimationState animState in animationInstances)
            {
                animState.Stop();
            }

            SetAnimationSources(animationState);
            animationState.Play();
        }

        private void FadeOutAllAnimations(float fadeTime = 0.3f, bool linear = false)
        {
            foreach (DidimoAnimationState animationTrack in animationInstances.Where(animationTrack => !animationTrack.PlayHandler.IsStopped))
            {
                animationTrack.WeightHandler.FadeOut(fadeTime, linear);
            }
        }
    }
}