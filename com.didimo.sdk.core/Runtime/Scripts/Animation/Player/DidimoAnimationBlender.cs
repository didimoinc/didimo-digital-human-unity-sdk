using System.Collections.Generic;
using UnityEngine;

namespace Didimo
{
    /// <summary>
    /// Class to blend between multiple animations being played or faded at any frame.
    /// This will blend the pose weights and the additional skeleton joint movements.
    /// </summary>
    public class DidimoAnimationBlender
    {
        // Pose data blending
        private readonly Dictionary<string, float> poseNameToValue = new Dictionary<string, float>();
        private readonly List<DidimoAnimation.PoseData> resolvedPoses = new List<DidimoAnimation.PoseData>();
        private readonly List<IReadOnlyList<DidimoAnimation.PoseData>>
            blendedPoses = new List<IReadOnlyList<DidimoAnimation.PoseData>>();

        // Skeleton data blending
        private readonly List<DidimoAnimation.SkeletonData> blendedSkeleton = new List<DidimoAnimation.SkeletonData>();
        private readonly DidimoAnimation.SkeletonData resolvedSkeleton = new DidimoAnimation.SkeletonData();

        public void Clear()
        {
            blendedPoses.Clear();
            blendedSkeleton.Clear();
        }

        /// <summary>
        /// Add pose weights to the list of poses to be blended for the current frame.
        /// </summary>
        /// <param name="poses">List of weights for each activated pose</param>
        public void AddPoseData(IReadOnlyList<DidimoAnimation.PoseData> poses) { blendedPoses.Add(poses); }

        /// <summary>
        /// Blend the weights between all the added poses for the current frame.
        /// </summary>
        /// <returns>Resulting list of blended weights for each pose</returns>
        public IReadOnlyList<DidimoAnimation.PoseData> ResolvePoseData()
        {
            poseNameToValue.Clear();
            resolvedPoses.Clear();
            foreach (IReadOnlyList<DidimoAnimation.PoseData> blendedPose in blendedPoses)
            {
                foreach (DidimoAnimation.PoseData poseData in blendedPose)
                {
                    if (!poseNameToValue.ContainsKey(poseData.Name)) poseNameToValue.Add(poseData.Name, 0);
                    poseNameToValue[poseData.Name] += poseData.Value;
                }
            }

            foreach (KeyValuePair<string, float> kvp in poseNameToValue)
            {
                resolvedPoses.Add(new DidimoAnimation.PoseData { Name = kvp.Key, Value = kvp.Value });
            }

            return resolvedPoses;
        }


        /// <summary>
        /// Add skeleton joint transformations to the list of transformations
        /// to be blended for the current frame.
        /// </summary>
        /// <param name="skeleton">List of joint transformations</param>
        public void AddSkeletonData(DidimoAnimation.SkeletonData skeleton) { blendedSkeleton.Add(skeleton); }


        /// <summary>
        /// Blend the transformations between all the added skeleton
        /// transformations for the current frame.
        /// </summary>
        /// <returns>Resulting blended skeleton transformation</returns>
        public DidimoAnimation.SkeletonData ResolveSkeletonData()
        {
            resolvedSkeleton.HeadPosition = Vector3.zero;
            resolvedSkeleton.HeadRotation = Quaternion.identity;
            resolvedSkeleton.LeftEyeRotation = Quaternion.identity;
            resolvedSkeleton.RightEyeRotation = Quaternion.identity;

            foreach (DidimoAnimation.SkeletonData skeletonBlendedData in blendedSkeleton)
            {
                resolvedSkeleton.HeadPosition += skeletonBlendedData.HeadPosition;
                resolvedSkeleton.HeadRotation *= skeletonBlendedData.HeadRotation;
                resolvedSkeleton.LeftEyeRotation *= skeletonBlendedData.LeftEyeRotation;
                resolvedSkeleton.RightEyeRotation *= skeletonBlendedData.RightEyeRotation;
            }

            return resolvedSkeleton;
        }
    }
}