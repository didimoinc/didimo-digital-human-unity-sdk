using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Didimo.Core.Utility;
using Didimo.Extensions;

namespace Didimo
{
    /// <summary>
    /// Class that provides high-level control of the didimo to play
    /// animations such as mocap or expressions.
    /// </summary>
    public class DidimoAnimator : DidimoBehaviour
    {
        private readonly Dictionary<string, DidimoAnimationState>
            idToAnimationInstance = new Dictionary<string, DidimoAnimationState>();
        private readonly List<DidimoAnimationState> animationInstances = new List<DidimoAnimationState>();

        private readonly DidimoAnimationBlender didimoAnimationBlender = new DidimoAnimationBlender();

        private AudioSource audioSource;

        public AudioSource AudioSource
        {
            get
            {
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                    if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.loop = false;
                }

                return audioSource;
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

        /// <summary>
        /// Play an expression from the <c>ExpressionDatabase</c>
        /// </summary>
        /// <param name="id">Name of the expression.</param>
        public void PlayExpression(string id)
        {
            if (id == "None") FadeOutAllAnimations();
            else CrossFadeAnimation(id);
        }

        /// <summary>
        /// Stops all other animations, and plays the animation with the given name
        /// </summary>
        /// <param name="id">Name of the animation to play</param>
        public void PlayAnimation(string id)
        {
            if (TryGetAnimation(id, out DidimoAnimationState animState))
            {
                PlayAnimation(animState);
            }
        }

        /// <summary>
        /// Fade in a mocap animation. This does not stop the other already playing animations.
        /// Can be used to resume an animation that was paused from by the <c>FadeOutAnimation</c> method.
        /// </summary>
        /// <param name="id">Name of the animation to play</param>
        /// <param name="fadeTime">Duration of the fade in</param>
        /// <param name="resume">True to resume playing from where the animation last stopped.
        /// False to restart the animation</param>
        /// <param name="linear">True to fade in linearly. False for a smooth fade in</param>
        /// <returns>True if the animation was found and the fade in started. False otherwise</returns>
        public bool FadeInAnimation(string id, float fadeTime = 0.3f, bool resume = false, bool linear = false)
        {
            if (!TryGetAnimation(id, out DidimoAnimationState animState)) return false;

            SetAnimationSources(animState);

            animState.WeightHandler.FadeIn(fadeTime, linear);
            animState.PlayHandler.Play(resume);

            return true;
        }

        /// <summary>
        /// Fade out a currently playing mocap animation. This can be used to pause an animation
        /// which can then be resumed by using the <c>FadeInAnimation</c> method.
        /// </summary>
        /// <param name="id">Name of the animation to fade out</param>
        /// <param name="fadeTime">Duration of the fade out</param>
        /// <param name="linear">True to fade out linearly. False for a smooth fade out</param>
        /// <returns>True if the animation was found and the fade out started. False otherwise</returns>
        public bool FadeOutAnimation(string id, float fadeTime = 0.3f, bool linear = false)
        {
            if (!TryGetAnimation(id, out DidimoAnimationState animState)) return false;
            animState.WeightHandler.FadeOut(fadeTime);

            return true;
        }

        /// <summary>
        /// Fade out all currently playing animations and fade in a new animation.
        /// </summary>
        /// <param name="id">Name of the animation to fade in</param>
        /// <param name="fadeTime">Duration of the fade out and fade in</param>
        /// <param name="resume">True to resume playing from where the animation last stopped.
        /// False to restart the animation</param>
        /// <param name="linear">True to fade linearly. False for a smooth fade</param>
        /// <returns>True if the animation to fade in was found and the fade in started. False otherwise</returns>
        public bool CrossFadeAnimation(string id, float fadeTime = 0.3f, bool resume = false, bool linear = false)
        {
            FadeOutAllAnimations(fadeTime, linear);
            if (!FadeInAnimation(id, fadeTime, resume, linear)) return false;
            return true;
        }

        /// <summary>
        /// Fade out all active playing animations.
        /// </summary>
        /// <param name="fadeTime">Duration of the fade out</param>
        /// <param name="linear">True to fade out linearly. False for a smooth fade out</param>
        public void FadeOutAllAnimations(float fadeTime = 0.3f, bool linear = false)
        {
            foreach (DidimoAnimationState animationTrack in animationInstances
                .Where(animationTrack => !animationTrack.PlayHandler.IsStopped))
            {
                animationTrack.WeightHandler.FadeOut(fadeTime, linear);
            }
        }

        /// <summary>
        /// Set the time, in seconds, for any animation. To set the normalized time,
        /// please use the <c>SetNormalizedTime</c> method instead.
        /// </summary>
        /// <param name="id">Name of the animation</param>
        /// <param name="time">Time, in seconds, to set for the animation</param>
        public void SetAnimationTime(string id, float time)
        {
            if (TryGetAnimation(id, out DidimoAnimationState animState))
            {
                animState.PlayHandler.Seek(time);
            }
        }


        /// <summary>
        /// Set the normalized seconds for any animation. To set the time in seconds,
        /// please use the <c>SetAnimationTime</c> method instead.
        /// </summary>
        /// <param name="id">Name of the animation</param>
        /// <param name="normalizedTime">Value between 0 (start) and 1 (end) for the normalized animation time.</param>
        public void SetNormalizedTime(string id, float normalizedTime)
        {
            if (TryGetAnimation(id, out DidimoAnimationState animState))
            {
                animState.PlayHandler.SeekNormalized(normalizedTime);
            }
        }

        /// <summary>
        /// Stops all currently playing animations.
        /// </summary>
        public void StopAllAnimations()
        {
            foreach (DidimoAnimationState animState in idToAnimationInstance.Values)
            {
                animState.WeightHandler.FadeOut(0);
            }
        }

        /// <summary>
        /// Remove an animation from the list of added/played animations by this didimo.
        /// This animation will still be accessible from the <c>AnimationCache</c>.
        /// </summary>
        /// <param name="animationId">Name of the animation to remove</param>
        public void RemoveAnimation(string animationId)
        {
            DidimoAnimationState animationState = idToAnimationInstance[animationId];
            if (animationState != null)
            {
                idToAnimationInstance.Remove(animationId);
                animationInstances.Remove(animationState);
                // This is probably useless, but leaving it in case.
                Resources.UnloadUnusedAssets();
            }
        }

        /// <summary>
        /// Remove all animation from the list of added/played animations by this didimo.
        /// All animations will still be accessible from the <c>AnimationCache</c>.
        /// </summary>
        public void RemoveAllAnimations()
        {
            idToAnimationInstance.Clear();
            animationInstances.Clear();
            // This is probably useless, but leaving it in case.
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Update the pose of the didimo by retrieving all the currently playing animations,
        /// and blending them appropriately.
        /// This method should automatically be called by Unity if there is any active animation.
        /// </summary>
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

                animInstance.CalculatePoseValues(
                    interpolateBetweenFrames,
                    DidimoComponents.PoseController.SupportedMovements);
                didimoAnimationBlender.AddPoseData(animInstance.Poses);
                if (animInstance.SkeletonData != null)
                {
                    didimoAnimationBlender.AddSkeletonData(animInstance.SkeletonData);
                }
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

        /// <summary>
        /// Get a <c>DidimoAnimationState</c> for an animation. This searches the
        /// added/played animations by this <c>DidimoAnimator</c> component and,
        /// if not found, also searches the <c>AnimationCache</c>.
        /// The <paramref name="state"/> contains information for each instance of
        /// an animation that exists.
        /// </summary>
        /// <param name="id">Name of the animation to find.</param>
        /// <param name="state"><c>DidimoAnimationState</c> object from the queried animation name</param>
        /// <returns>True if the animation was found. False otherwise</returns>
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

        /// <summary>
        /// Set the proper animation data sources for the instance of an animation.
        /// If there is an audio clip, the source time will be from the playing audio clip
        /// to ensure it is synced with the audio.
        /// If there are timestamps, the frame number will calculated from these to sync better
        /// with the original animation capture.
        /// </summary>
        /// <param name="animationState">Instance/State data of the animation to update sources</param>
        private void SetAnimationSources(DidimoAnimationState animationState)
        {
            animationState.PlayHandler.FrameSource = animationState.SourceAnimation.Timestamps.IsNullOrEmpty()
                ? DidimoAnimationPlayHandler.FrameCalculationSource.FPS
                : DidimoAnimationPlayHandler.FrameCalculationSource.Timestamps;

            animationState.PlayHandler.TimeSource = animationState.SourceAnimation.AudioClip == null
                ? DidimoAnimationPlayHandler.AnimationTimeSource.InternalTime
                : DidimoAnimationPlayHandler.AnimationTimeSource.AudioTime;
        }

        /// <summary>
        /// Start playing a new animation and stop playing all other animations.
        /// </summary>
        /// <param name="animationState">Instance/State data of the animation to play</param>
        private void PlayAnimation(DidimoAnimationState animationState)
        {
            foreach (DidimoAnimationState animState in animationInstances)
            {
                animState.Stop();
            }

            SetAnimationSources(animationState);
            animationState.Play();
        }
    }
}
