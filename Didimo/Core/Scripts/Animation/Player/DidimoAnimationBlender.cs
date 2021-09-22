using System.Collections.Generic;
using UnityEngine;

namespace Didimo
{
    public class DidimoAnimationBlender
    {
        // Pose data blending
        private readonly Dictionary<string, float>                     poseNameToValue = new Dictionary<string, float>();
        private readonly List<DidimoAnimation.PoseData>                resolvedPoses   = new List<DidimoAnimation.PoseData>();
        private readonly List<IReadOnlyList<DidimoAnimation.PoseData>> blendedPoses    = new List<IReadOnlyList<DidimoAnimation.PoseData>>();
        
        // Skeleton data blending
        private readonly List<DidimoAnimation.SkeletonData> blendedSkeleton = new List<DidimoAnimation.SkeletonData>();
        private readonly DidimoAnimation.SkeletonData resolvedSkeleton = new DidimoAnimation.SkeletonData();

        public void Clear()
        {
            blendedPoses.Clear();
            blendedSkeleton.Clear();
        }

        public void AddPoseData(IReadOnlyList<DidimoAnimation.PoseData> poses) { blendedPoses.Add(poses); }

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
                resolvedPoses.Add(new DidimoAnimation.PoseData {Name = kvp.Key, Value = kvp.Value});
            }

            return resolvedPoses;
        }
        
        public void AddSkeletonData(DidimoAnimation.SkeletonData skeleton) { blendedSkeleton.Add(skeleton); }
        
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