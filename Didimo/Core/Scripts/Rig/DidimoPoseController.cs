using UnityEngine;
using System;

namespace Didimo
{
    public abstract class DidimoPoseController : DidimoBehaviour
    {
        [Flags]
        public enum ESupportedMovements: short
        {
            None  = 0,
            Poses = 1,
            HeadTranslation   = 2,
            HeadRotation = 4,
            EyeRotation  = 8
        }

        public abstract ESupportedMovements SupportedMovements { get; }

        public bool SupportsAnimation => !SupportedMovements.Equals(ESupportedMovements.None);
        

        // Control for face poses
        public virtual bool SetWeightForPose(string source, string poseName, float poseWeight) => false;
        public virtual bool SetWeightForPose(string poseName, float poseWeight) => false;
        public virtual bool OffsetWeightForPose(string poseName, float poseWeight) => false;
        public virtual void ResetAllPoseWeights() { }
        
        
        // Control for head/skeleton movement
        public virtual bool SetHeadPosition(Vector3 position) => false;
        public virtual bool OffsetHeadPosition(Vector3 deltaPosition) => false;
        public virtual void ResetHeadPosition() { }
        

        public virtual bool SetHeadRotation(Quaternion rotation) => false;
        public virtual bool OffsetHeadRotation(Quaternion rotation) => false;
        public virtual void ResetHeadRotation() { }
        

        public virtual bool SetLeftEyeRotation(Quaternion rotation) => false;
        public virtual bool OffsetLeftEyeRotation(Quaternion rotation) => false;
        public virtual void ResetLeftEyeRotation() { }

        public virtual bool SetRightEyeRotation(Quaternion rotation) => false;
        public virtual bool OffsetRightEyeRotation(Quaternion rotation) => false;
        public virtual void ResetRightEyeRotation() { }

        public virtual void ResetEyesRotation()
        {
            ResetLeftEyeRotation();
            ResetRightEyeRotation();
        }

        public virtual void ResetHeadTransform()
        {
            ResetHeadPosition();
            ResetHeadRotation();
            ResetEyesRotation();
        }


        // General functions
        public virtual void ResetAll()
        {
            ResetAllPoseWeights();
            ResetHeadTransform();
        }
        
        public abstract void ForceUpdateAnimation();
    }
}