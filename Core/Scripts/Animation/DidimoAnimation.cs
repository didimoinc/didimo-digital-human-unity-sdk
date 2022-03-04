using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Mocap;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    /// <summary>
    /// Class that contains the information of an animation that can be
    /// played by any didimo using the <c>DidimoAnimator</c> component.
    /// To play an animation, instances of <c>DidimoAnimationState</c> are created
    /// with reference to this source animation where the data is stored and shared
    /// between all didimos.
    /// </summary>
    public class DidimoAnimation
    {
        public const int DEFAULT_FPS = 60;

        public class PoseData
        {
            public string Name;
            public float  Value;
        }

        public class SkeletonData
        {
            public const string HEAD_POSITION_KEY      = "headPosition";
            public const string HEAD_ROTATION_KEY      = "headRotation";
            public const string LEFT_EYE_ROTATION_KEY  = "leftEyeRotation";
            public const string RIGHT_EYE_ROTATION_KEY = "rightEyeRotation";

            public Vector3    HeadPosition     = Vector3.zero;
            public Quaternion HeadRotation     = Quaternion.identity;
            public Quaternion LeftEyeRotation  = Quaternion.identity;
            public Quaternion RightEyeRotation = Quaternion.identity;
        }

        public List<string>      PoseNames { get; private set; }
        public List<List<float>> PoseValues { get; private set; }

        public Dictionary<string, List<float[]>> SkeletonValues { get; private set; }

        // Timestamps and AudioClip are optional for the animation
        public List<float> Timestamps;
        public AudioClip   AudioClip;

        public string AnimationName { get; private set; }
        public int FPS { get; private set; }
        public WrapMode WrapMode { get; set; }
        public double TotalAnimationTime { get; private set; }
        public int FrameCount { get; private set; }

        public bool HasSkeletonAnimation => SkeletonValues != null && SkeletonValues.Count > 0;

        /// <summary>
        /// Get the normalized time equivalent of a time for this animation.
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <returns>Normalized time value</returns>
        public float GetNormalizedTime(float animationTime)
        {
            return TotalAnimationTime == 0f ? 0f : (float) (animationTime / TotalAnimationTime);
        }

        /// <summary>
        /// Get the corresponding frame number for an animation time.
        /// If the <paramref name="animationTime"/> lands between two frames, the earlier one is returned.
        /// If this animations contains Timestamps, they are used to calculate the
        /// frame. Otherwise it is approximated using the FPS value.
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <returns>Number of the frame that corresponds to <paramref name="animationTime"/></returns>
        public int GetFrameNumber(float animationTime)
        {
            if (Timestamps == null) return GetFrameNumberFromFPS(animationTime);
            return GetFrameNumberFromTimestamps(animationTime);
        }

        /// <summary>
        /// Get the corresponding frame number for an animation time,
        /// using the FPS value to approximate the result.
        /// If the <paramref name="animationTime"/> lands between two frames, the earlier one is returned.
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <returns>Number of the frame that corresponds to <paramref name="animationTime"/></returns>
        public int GetFrameNumberFromFPS(float animationTime)
        {
            float normalisedTime = GetNormalizedTime(animationTime);
            return (int) (normalisedTime * (FrameCount - 1));
        }

        /// <summary>
        /// Get the corresponding frame number for an animation time,
        /// using the Timestamps value to calculate the frame.
        /// If the <paramref name="animationTime"/> lands between two frames, the earlier one is returned.
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <returns>Number of the frame that corresponds to <paramref name="animationTime"/></returns>
        public int GetFrameNumberFromTimestamps(float animationTime)
        {
            float normalisedTime = GetNormalizedTime(animationTime);
            int approximateFrame = Mathf.Clamp((int) (normalisedTime * (FrameCount - 1)), 0, FrameCount - 1);

            if (animationTime < Timestamps[approximateFrame])
            {
                // Too far ahead on the timestamps, go back
                for (int frame = approximateFrame; frame >= 0; frame--)
                {
                    if (animationTime >= Timestamps[frame]) return frame;
                }
            }
            else if (animationTime > Timestamps[approximateFrame])
            {
                // Too far back, go forward
                for (int frame = approximateFrame; frame >= FrameCount; frame++)
                {
                    if (animationTime <= Timestamps[frame]) return frame - 1;
                }
            }

            return approximateFrame;
        }

        /// <summary>
        /// Get the two corresponding frames that best match the given <paramref name="animationTime"/>
        /// as well as the interpolation value/weight for these frames.
        /// The interpolation weight is a value between 0 and 1, where
        ///   weight=0 means that the <paramref name="animationTime"/> matches the <paramref name="initialFrameIndex"/>
        ///   weight=1 means that the <paramref name="animationTime"/> matches the <paramref name="finalFrameIndex"/>
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <param name="initialFrameIndex">Initial frame that matches the given time</param>
        /// <param name="finalFrameIndex">Final frame that matches the given time</param>
        /// <param name="weight">Interpolation value between the initial and final frames</param>
        public void GetInterpolatedFrameNumbers(
            float animationTime, out int initialFrameIndex, out int finalFrameIndex, out float weight)
        {
            if (Timestamps == null)
            {
                GetInterpolatedFrameNumbersFromFPS(animationTime,
                    out initialFrameIndex, out finalFrameIndex, out weight);
            }
            else
            {
                GetInterpolatedFrameNumbersFromTimestamps(animationTime,
                    out initialFrameIndex, out finalFrameIndex, out weight);
            }
        }


        /// <summary>
        /// Get the two corresponding frames that best match the given <paramref name="animationTime"/>,
        /// using the FPS to estimate the frames, as well as the interpolation value/weight for these frames.
        /// The interpolation weight is a value between 0 and 1, where
        ///   weight=0 means that the <paramref name="animationTime"/> matches the <paramref name="initialFrameIndex"/>
        ///   weight=1 means that the <paramref name="animationTime"/> matches the <paramref name="finalFrameIndex"/>
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <param name="initialFrameIndex">Initial frame that matches the given time</param>
        /// <param name="finalFrameIndex">Final frame that matches the given time</param>
        /// <param name="weight">Interpolation value between the initial and final frames</param>
        public void GetInterpolatedFrameNumbersFromFPS(float animationTime,
            out int initialFrameIndex, out int finalFrameIndex, out float weight)
        {
            float normalisedTime = GetNormalizedTime(animationTime);
            initialFrameIndex = (int) (normalisedTime * (FrameCount - 1));
            if (initialFrameIndex == FrameCount - 1)
            {
                finalFrameIndex = initialFrameIndex;
                weight = 1;
                return;
            }

            finalFrameIndex = initialFrameIndex + 1;
            // inverse lerp: weight = (animationTime - initialFrameIndex / FPS)
            // / (finalFrameIndex / FPS - initialFrameIndex / FPS);
            weight = 1 - (animationTime * FPS - initialFrameIndex);
        }

        /// <summary>
        /// Get the two corresponding frames that best match the given <paramref name="animationTime"/>,
        /// using the Timestamps to estimate the frames, as well as the interpolation value/weight for these frames.
        /// The interpolation weight is a value between 0 and 1, where
        ///   weight=0 means that the <paramref name="animationTime"/> matches the <paramref name="initialFrameIndex"/>
        ///   weight=1 means that the <paramref name="animationTime"/> matches the <paramref name="finalFrameIndex"/>
        /// </summary>
        /// <param name="animationTime">Time in seconds</param>
        /// <param name="initialFrameIndex">Initial frame that matches the given time</param>
        /// <param name="finalFrameIndex">Final frame that matches the given time</param>
        /// <param name="weight">Interpolation value between the initial and final frames</param>
        public void GetInterpolatedFrameNumbersFromTimestamps(float animationTime,
            out int initialFrameIndex, out int finalFrameIndex, out float weight)
        {
            float normalisedTime = GetNormalizedTime(animationTime);
            int approximateFrame = Mathf.Clamp((int) (normalisedTime * (FrameCount - 1)), 0, FrameCount - 1);

            if (animationTime == Timestamps[approximateFrame])
            {
                initialFrameIndex = approximateFrame;
                finalFrameIndex = initialFrameIndex;
                weight = 1;
                return;
            }

            if (animationTime < Timestamps[approximateFrame])
            {
                // Too far ahead on the timestamps, go back
                for (int frame = approximateFrame; frame >= 0; frame--)
                {
                    if (animationTime >= Timestamps[frame])
                    {
                        initialFrameIndex = frame;
                        finalFrameIndex = initialFrameIndex + 1;
                        weight = (animationTime - Timestamps[initialFrameIndex])
                            / (Timestamps[finalFrameIndex] - Timestamps[initialFrameIndex]);
                        return;
                    }
                }

                initialFrameIndex = 0;
                finalFrameIndex = 0;
                weight = 1;
                return;
            }

            // Too far back, go forward
            for (int frame = approximateFrame; frame < FrameCount; frame++)
            {
                if (animationTime <= Timestamps[frame])
                {
                    initialFrameIndex = frame - 1;
                    if (initialFrameIndex == FrameCount - 1)
                    {
                        finalFrameIndex = initialFrameIndex;
                        weight = 1;
                        return;
                    }

                    finalFrameIndex = frame;
                    //TODO: Remove repeating code
                    weight = (animationTime - Timestamps[initialFrameIndex])
                        / (Timestamps[finalFrameIndex] - Timestamps[initialFrameIndex]);
                    return;
                }
            }

            initialFrameIndex = FrameCount - 1;
            finalFrameIndex = FrameCount - 1;
            weight = 1;
        }


        /// <summary>
        /// Get the information for the poses and their weights for a specific frame.
        /// Weight is used as a multiplier for the weights of all pose weights.
        /// The information is added to the <paramref name="poses"/> list.
        /// </summary>
        /// <param name="frame">Index of the animation frame.</param>
        /// <param name="weight">Weight multiplier for the pose weights.</param>
        /// <param name="poses">List of <c>PoseData</c> to be filled with the new pose information.</param>
        private void GetPosesForFrame(int frame, float weight, ref List<PoseData> poses)
        {
            poses.Clear();
            for (int i = 0; i < PoseNames.Count; i++)
            {
                string poseName = PoseNames[i];
                float poseValue = PoseValues[i][frame] * weight;

                poses.Add(new PoseData {Name = poseName, Value = poseValue});
            }
        }

        /// <summary>
        /// Get the information for the skeleton transforms for a specific frame.
        /// Weight is used as a multiplier the skeleton transforms.
        /// The information is added to the <paramref name="skeletonData"/> object.
        /// </summary>
        /// <param name="frame">Index of the animation frame.</param>
        /// <param name="weight">Weight multiplier for the skeleton transforms.</param>
        /// <param name="supportedMovements">List of supported movements to query from the animation.</param>
        /// <param name="skeletonData"><c>SkeletonData</c> object to be filled
        /// with the the new transform information.</param>
        private void GetSkeletonForFrame(int frame, float weight,
            DidimoPoseController.ESupportedMovements supportedMovements, out SkeletonData skeletonData)
        {
            skeletonData = new SkeletonData();
            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.HeadTranslation)
                && SkeletonValues.TryGetValue(SkeletonData.HEAD_POSITION_KEY, out List<float[]> headPositions))
            {
                Vector3 headPosition = new Vector3(
                        headPositions[frame][0], headPositions[frame][1], headPositions[frame][2]);
                skeletonData.HeadPosition = headPosition * weight;
            }

            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.HeadRotation)
                && SkeletonValues.TryGetValue(SkeletonData.HEAD_ROTATION_KEY, out List<float[]> headRotations))
            {
                Quaternion headRotation = Quaternion.Euler(headRotations[frame][0],
                headRotations[frame][1], headRotations[frame][2]);
                skeletonData.HeadRotation = Quaternion.Lerp(Quaternion.identity, headRotation, weight);
            }

            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.EyeRotation))
            {
                if (SkeletonValues.TryGetValue(SkeletonData.LEFT_EYE_ROTATION_KEY,
                    out List<float[]> leftEyeRotations))
                {
                    Quaternion leftEyeRotation = Quaternion.Euler(leftEyeRotations[frame][0],
                    leftEyeRotations[frame][1], leftEyeRotations[frame][2]);
                    skeletonData.LeftEyeRotation = Quaternion.Lerp(Quaternion.identity, leftEyeRotation, weight);
                }

                if (SkeletonValues.TryGetValue(SkeletonData.RIGHT_EYE_ROTATION_KEY,
                    out List<float[]> rightEyeRotations))
                {
                    Quaternion rightEyeRotation = Quaternion.Euler(
                            rightEyeRotations[frame][0], rightEyeRotations[frame][1], rightEyeRotations[frame][2]);
                    skeletonData.RightEyeRotation = Quaternion.Lerp(Quaternion.identity, rightEyeRotation, weight);
                }
            }
        }

        /// <summary>
        /// Get the information for the poses and their weights as well as skeleton transforms for a specific frame.
        /// Weight is used as a multiplier for the weights of all pose weights and skeleton transformations.
        /// The information is added to the <paramref name="poses"/> list and the
        /// <paramref name="skeletonData"/> object.
        /// </summary>
        /// <param name="frame">Index of the animation frame.</param>
        /// <param name="weight">Weight multiplier for the pose weights and skeleton transforms.</param>
        /// <param name="supportedMovements">List of supported skeleton movements to query from the animation.</param>
        /// <param name="poses">List of <c>PoseData</c> to be filled with the new pose information.</param>
        /// <param name="skeletonData"><c>SkeletonData</c> object to be filled
        /// with the the new transform information.</param>
        public void GetAnimationPoseValues(int frame, float weight,
            DidimoPoseController.ESupportedMovements supportedMovements,
            ref List<PoseData> poses, out SkeletonData skeletonData)
        {
            skeletonData = null;
            if (frame < 0 || frame >= FrameCount)
            {
                Debug.LogWarning("Invalid frame index: " + frame);
                return;
            }

            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.Poses))
            {
                GetPosesForFrame(frame, weight, ref poses);
            }
            if (HasSkeletonAnimation)
            {
                GetSkeletonForFrame(frame, weight, supportedMovements, out skeletonData);
            }
        }


        /// <summary>
        /// Get the information for the poses and their weights for the interpolated weighted frames.
        /// Weight is used as a multiplier for the weights of all pose weights.
        /// The information is added to the <paramref name="poses"/> list.
        /// </summary>
        /// <param name="initialFrame">Initial index of the animation frame.</param>
        /// <param name="finalFrame">Final index of the animation frame.</param>
        /// <param name="frameWeight">Interpolation weight of the frames.</param>
        /// <param name="weight">Weight multiplier for the pose weights and skeleton transforms.</param>
        /// <param name="poses">List of <c>PoseData</c> to be filled with the new pose information.</param>
        private void GetPosesForInterpolatedFrame(int initialFrame, int finalFrame, float frameWeight, float weight,
            ref List<PoseData> poses)
        {
            poses.Clear();
            for (int i = 0; i < PoseValues.Count; i++)
            {
                string poseName = PoseNames[i];
                float poseValue = (PoseValues[i][initialFrame]
                    * frameWeight + PoseValues[i][finalFrame] * (1 - frameWeight)) * weight;

                poses.Add(new PoseData {Name = poseName, Value = poseValue});
            }
        }


        /// <summary>
        /// Get the information for the skeleton transforms for the interpolated weighted frames.
        /// Weight is used as a multiplier for the skeleton transformations.
        /// The information is added to the <paramref name="skeletonData"/> object.
        /// </summary>
        /// <param name="initialFrame">Initial index of the animation frame.</param>
        /// <param name="finalFrame">Final index of the animation frame.</param>
        /// <param name="frameWeight">Interpolation weight of the frames.</param>
        /// <param name="weight">Weight multiplier for the pose weights and skeleton transforms.</param>
        /// <param name="supportedMovements">List of supported skeleton movements to query from the animation.</param>
        /// <param name="skeletonData"><c>SkeletonData</c> object to be filled with the the
        /// new transform information.</param>
        private void GetSkeletonForInterpolatedFrame(int initialFrame, int finalFrame, float frameWeight, float weight,
            DidimoPoseController.ESupportedMovements supportedMovements, out SkeletonData skeletonData)
        {
            skeletonData = new SkeletonData();
            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.HeadTranslation)
                && SkeletonValues.TryGetValue(SkeletonData.HEAD_POSITION_KEY, out List<float[]> headPositions))
            {
                Vector3 initalHeadPosition = new Vector3(
                    headPositions[initialFrame][0], headPositions[initialFrame][1], headPositions[initialFrame][2]);
                Vector3 finalHeadPosition = new Vector3(
                    headPositions[finalFrame][0], headPositions[finalFrame][1], headPositions[finalFrame][2]);
                Vector3 headPostion = Vector3.Lerp(initalHeadPosition, finalHeadPosition, frameWeight);
                skeletonData.HeadPosition = headPostion * weight;
            }

            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.HeadRotation)
                && SkeletonValues.TryGetValue(SkeletonData.HEAD_ROTATION_KEY, out List<float[]> headRotations))
            {
                Quaternion initialHeadRotation = Quaternion.Euler(
                    headRotations[initialFrame][0], headRotations[initialFrame][1], headRotations[initialFrame][2]);
                Quaternion finalHeadRotation = Quaternion.Euler(
                    headRotations[finalFrame][0], headRotations[finalFrame][1], headRotations[finalFrame][2]);
                // We can use lerp+normalize (nlerp) if we assumed they are close
                Quaternion headRotation = Quaternion.Slerp(initialHeadRotation, finalHeadRotation, frameWeight);
                skeletonData.HeadRotation = Quaternion.Slerp(Quaternion.identity, headRotation, weight);
            }

            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.EyeRotation))
            {
                if (SkeletonValues.TryGetValue(SkeletonData.LEFT_EYE_ROTATION_KEY, out List<float[]> leftEyeRotations))
                {
                    Quaternion initialLeftEyeRotation = Quaternion.Euler(
                        leftEyeRotations[initialFrame][0],
                        leftEyeRotations[initialFrame][1],
                        leftEyeRotations[initialFrame][2]);
                    Quaternion finalLeftEyeRotation = Quaternion.Euler(
                        leftEyeRotations[finalFrame][0],
                        leftEyeRotations[finalFrame][1],
                        leftEyeRotations[finalFrame][2]);
                    Quaternion leftEyeRotation = Quaternion.Slerp(
                        initialLeftEyeRotation, finalLeftEyeRotation, frameWeight);
                    skeletonData.LeftEyeRotation = Quaternion.Slerp(Quaternion.identity, leftEyeRotation, weight);
                }

                if (SkeletonValues.TryGetValue(
                    SkeletonData.RIGHT_EYE_ROTATION_KEY,
                    out List<float[]> rightEyeRotations))
                {
                    Quaternion initialRightEyeRotation =
                        Quaternion.Euler(
                            rightEyeRotations[initialFrame][0],
                            rightEyeRotations[initialFrame][1],
                            rightEyeRotations[initialFrame][2]);
                    Quaternion finalRightEyeRotation = Quaternion.Euler(
                        rightEyeRotations[finalFrame][0],
                        rightEyeRotations[finalFrame][1],
                        rightEyeRotations[finalFrame][2]);
                    Quaternion rightEyeRotation = Quaternion.Slerp(
                        initialRightEyeRotation, finalRightEyeRotation, frameWeight);
                    skeletonData.RightEyeRotation = Quaternion.Slerp(
                        Quaternion.identity, rightEyeRotation, weight);
                }
            }
        }


        /// <summary>
        /// Get the information for the poses and their weights as well as skeleton transforms for
        /// the interpolated weighted frames.
        /// Weight is used as a multiplier for the weights of all pose weights and skeleton transformations.
        /// The information is added to the <paramref name="poses"/> list and
        /// the <paramref name="skeletonData"/> object.
        /// </summary>
        /// <param name="initialFrame">Initial index of the animation frame.</param>
        /// <param name="finalFrame">Final index of the animation frame.</param>
        /// <param name="frameWeight">Interpolation weight of the frames.</param>
        /// <param name="weight">Weight multiplier for the pose weights and skeleton transforms.</param>
        /// <param name="supportedMovements">List of supported skeleton movements to query from the animation.</param>
        /// <param name="poses">List of <c>PoseData</c> to be filled with the new pose information.</param>
        /// <param name="skeletonData"><c>SkeletonData</c> object to be filled
        /// with the the new transform information.</param>
        public void GetAnimationPoseValues(int initialFrame, int finalFrame, float frameWeight,
            float weight, DidimoPoseController.ESupportedMovements supportedMovements,
            ref List<PoseData> poses, out SkeletonData skeletonData)
        {
            skeletonData = null;
            if (initialFrame < 0 || finalFrame < 0 || initialFrame >= FrameCount || finalFrame >= FrameCount)
            {
                Debug.LogError($"Invalid frame index range: [{initialFrame}:{finalFrame}]");
                return;
            }

            if (supportedMovements.HasFlag(DidimoPoseController.ESupportedMovements.Poses))
            {
                GetPosesForInterpolatedFrame(initialFrame, finalFrame, frameWeight, weight, ref poses);
            }
            if (HasSkeletonAnimation)
            {
                GetSkeletonForInterpolatedFrame(initialFrame, finalFrame,
                    frameWeight, weight, supportedMovements, out skeletonData);
            }
        }

        /// <summary>
        /// Build a <c>DidimoAnimation</c> object with pose weights.
        /// The FPS of the animation is assumed to be <c>DEFAULT_FPS</c>.
        /// </summary>
        /// <param name="animationName">Name of the animation.</param>
        /// <param name="poses">Dictionary of poses and their weights for each frame.</param>
        /// <returns><c>DidimoAnimation</c> object created.</returns>
        public static DidimoAnimation FromPosesDictionary(string animationName, Dictionary<string, List<float>> poses)
        {
            DidimoAnimation animation = new DidimoAnimation();
            animation.AnimationName = animationName;

            animation.PoseNames = poses.Keys.ToList();
            animation.PoseValues = poses.Values.ToList();

            animation.FrameCount = poses.Values.First().Count;
            animation.FPS = DEFAULT_FPS;

            animation.TotalAnimationTime = (double) animation.FrameCount / animation.FPS;
            return animation;
        }

        /// <summary>
        /// Build a <c>DidimoAnimation</c> object with pose weights.
        /// </summary>
        /// <param name="animationName">Name of the animation.</param>
        /// <param name="poses">Dictionary of poses and their weights for each frame.</param>
        /// <param name="fps">FPS value of the animation. Can be null.</param>
        /// <param name="animationLength">Length of the animation.</param>
        /// <param name="frameCount">Number of frames of the animation.</param>
        /// <param name="audioClip">Optional associated AudioClip. Default is null. </param>
        /// <returns><c>DidimoAnimation</c> object created.</returns>
        public static DidimoAnimation FromPosesDictionary(string animationName,
        Dictionary<string, List<float>> poses, int? fps, float animationLength,
        int frameCount, AudioClip audioClip=null)
        {
            DidimoAnimation animation = new DidimoAnimation();
            animation.AnimationName = animationName;

            animation.PoseNames = poses.Keys.ToList();
            animation.PoseValues = poses.Values.ToList();

            animation.FrameCount = frameCount;
            animation.FPS = fps ?? DEFAULT_FPS;
            animation.TotalAnimationTime = animationLength;

            animation.AudioClip = audioClip;
            return animation;
        }

        /// <summary>
        /// Build a <c>DidimoAnimation</c> object with pose weights and timestamps.
        /// </summary>
        /// <param name="animationName">Name of the animation.</param>
        /// <param name="poses">Dictionary of poses and their weights for each frame.</param>
        /// <param name="timestamps">List of timestamps for each frame.</param>
        /// <param name="fps">FPS value of the animation. Can be null.</param>
        /// <param name="animationLength">Length of the animation.</param>
        /// <param name="frameCount">Number of frames of the animation.</param>
        /// <param name="audioClip">Optional associated AudioClip. Default is null. </param>
        /// <returns><c>DidimoAnimation</c> object created.</returns>
        public static DidimoAnimation FromPosesDictionary(string animationName,
        Dictionary<string, List<float>> poses, List<float> timestamps, int? fps,
        float animationLength, int frameCount, AudioClip audioClip=null)
        {
            DidimoAnimation animation = new DidimoAnimation();
            animation.AnimationName = animationName;

            animation.PoseNames = poses.Keys.ToList();
            animation.PoseValues = poses.Values.ToList();
            animation.Timestamps = timestamps;

            animation.FrameCount = frameCount;
            animation.FPS = fps ?? DEFAULT_FPS;
            animation.TotalAnimationTime = animationLength;

            animation.AudioClip = audioClip;
            return animation;
        }

        /// <summary>
        /// Build a <c>DidimoAnimation</c> object with pose weights, skeleton data and timestamps.
        /// </summary>
        /// <param name="animationName">Name of the animation.</param>
        /// <param name="poses">Dictionary of poses and their weights for each frame.</param>
        /// <param name="skeleton">Dictionary of skeleton transforms and their values for each frame.</param>
        /// <param name="timestamps">List of timestamps for each frame.</param>
        /// <param name="fps">FPS value of the animation. Can be null.</param>
        /// <param name="animationLength">Length of the animation.</param>
        /// <param name="frameCount">Number of frames of the animation.</param>
        /// <param name="audioClip">Optional associated AudioClip. Default is null. </param>
        /// <returns><c>DidimoAnimation</c> object created.</returns>
        public static DidimoAnimation FromPosesDictionary(string animationName,
            Dictionary<string, List<float>> poses,
            Dictionary<string, List<float[]>> skeleton,
            List<float> timestamps, int? fps,
            float animationLength, int frameCount,
            AudioClip audioClip=null)
        {
            DidimoAnimation animation = new DidimoAnimation();
            animation.AnimationName = animationName;

            animation.PoseNames = poses.Keys.ToList();
            animation.PoseValues = poses.Values.ToList();
            animation.SkeletonValues = skeleton;
            animation.Timestamps = timestamps;

            animation.FrameCount = frameCount;
            animation.FPS = fps ?? DEFAULT_FPS;
            animation.TotalAnimationTime = animationLength;

            animation.AudioClip = audioClip;
            return animation;
        }

        /// <summary>
        /// Build a <c>DidimoAnimation</c> from a JSON string, such as the content of the JSON file.
        /// Optionally, can also include the associated audio clip.
        /// </summary>
        /// <param name="animationID">Name of the animation.</param>
        /// <param name="fileText">JSON string of the animation.</param>
        /// <param name="audioClip">Optional associated AudioClip. Default is null. </param>
        /// <returns><c>DidimoAnimation</c> object created from the JSON file.</returns>
        public static DidimoAnimation FromJSONContent(string animationID, string fileText, AudioClip audioClip=null)
        {
            DidimoAnimationJson didimoAnimationJson = JsonConvert.DeserializeObject<DidimoAnimationJson>(fileText);
            Dictionary<string, List<float>> renamedMocapValues = new Dictionary<string, List<float>>(
                didimoAnimationJson.FrameCount);
            foreach (KeyValuePair<string, List<float>> mocapValue in didimoAnimationJson.MocapValues)
            {
                renamedMocapValues.Add($"{didimoAnimationJson.Source}_{mocapValue.Key}", mocapValue.Value);
            }

            return FromPosesDictionary(animationID, renamedMocapValues,
                didimoAnimationJson.SkeletonValues, didimoAnimationJson.Timestamps,
                didimoAnimationJson.FPS, didimoAnimationJson.AnimationLength, didimoAnimationJson.FrameCount,
                audioClip);
         }

        /// <summary>
        /// Build a <c>DidimoAnimation</c> from a JSON file.
        /// Optionally, can also include the associated audio clip.
        /// </summary>
        /// <param name="animationID">Name of the animation.</param>
        /// <param name="filePath">Path to the JSON animation file.</param>
        /// <param name="audioClip">Optional associated AudioClip. Default is null. </param>
        /// <returns><c>DidimoAnimation</c> object created from the JSON file.</returns>
        public static DidimoAnimation FromJSON(string animationID, string filePath, AudioClip audioClip=null)
        {
            string fileText = File.ReadAllText(filePath);
            return FromJSONContent(animationID, fileText, audioClip);
        }
    }
}