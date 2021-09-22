using System.Collections.Generic;
using System.Linq;
using DigitalSalmon.Extensions;
using UnityEngine;

namespace Didimo
{
    public class LegacyAnimationPoseController : DidimoPoseController
    {
        public class DidimoFaceShape
        {
            public readonly string Name;

            public readonly AnimationState AnimationState;

            public DidimoFaceShape(string name, AnimationState animationState)
            {
                Name = name;
                AnimationState = animationState;
            }
        }

        public class TransformValues
        {
            public readonly Vector3    Position;
            public readonly Quaternion Rotation;
            public readonly Vector3    Scale;

            public TransformValues(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }
        }

        public const  string RESET_POSE_NAME     = "RESET_POSE";
        private const string DEFAULT_POSE_SOURCE = "ARKit";

        public override ESupportedMovements SupportedMovements => ESupportedMovements.Poses | ESupportedMovements.HeadRotation;

        [SerializeField, HideInInspector]
        protected Animation animationComponent;

        public  AnimationClip[] animationClips;
        public  AnimationClip   resetAnimationClip;
        public  Transform       headJoint;
        private TransformValues initialHeadJointTransform;


        [Range(0, 1)]
        public float headJointWeight = 1f;
        


        // We may not have a 1:1 match between user names and clip/pose names if something changes. Good to allow for config file (ex: eyeBlink_L -> eyeBlinkLeft)
        private readonly Dictionary<string, string>          namePoseAliases = new Dictionary<string, string>();
        private          Dictionary<string, DidimoFaceShape> nameToPoseMapping;
        private          DidimoFaceShape                     resetFaceShape;
        
        private          bool                                poseDataWasUpdated = false;
        private readonly HashSet<string>                     shapesToDisable     = new HashSet<string>();

        private Dictionary<string, DidimoFaceShape> NameToPoseMapping
        {
            get
            {
                if (nameToPoseMapping == null && animationClips.Length > 0 && resetAnimationClip != null)
                {
                    BuildController();
                }

                return nameToPoseMapping;
            }
        }

        /// <summary>
        /// <para>Build the Animation Player with the required animation clips and, if necessary/present, a configuration JSON.</para>
        /// </summary>
        /// <param name="poseAnimationClips">Collection of animation clips for the poses</param>
        /// <param name="resetPoseAnimationClip">Animation Clip for rest pose</param>
        /// <param name="headJointTransform">Transform for the head to move with skeleton animations. Can be null</param>
        /// <param name="animationConfigJson">JSON string with configuration of name mappings. Can be null</param>
        public virtual void BuildController(AnimationClip[] poseAnimationClips, AnimationClip resetPoseAnimationClip, Transform headJointTransform = null, string animationConfigJson = null)
        {
            animationClips = poseAnimationClips;
            resetAnimationClip = resetPoseAnimationClip;
            headJoint = headJointTransform;
            BuildController(animationConfigJson);
        }
        
        /// <summary>
        /// <para>Build the Animation Player with the existing animation clips and, if necessary/present, a configuration JSON.</para>
        /// </summary>
        /// <param name="animationConfigJson">JSON string with configuration of name mappings. Can be null</param>
        public virtual void BuildController(string animationConfigJson = null)
        {
            if (animationClips.IsNullOrEmpty() || resetAnimationClip == null)
            {
                Debug.LogWarning("Cannot build LegacyAnimationPoseController without providing animationClips and the resetAnimationClip");
                return;
            }
            
            initialHeadJointTransform = headJoint == null ? null : new TransformValues(headJoint.localPosition, headJoint.localRotation, headJoint.localScale);

            // For compatibility to ensure we always place animator at the correct level
            if (DidimoComponents == null)
            {
                ComponentUtility.GetOrAdd(gameObject, ref animationComponent);
            }
            // It's a didimo, but lost context reference when recompiling. Let's try our best to find it
            else if (DidimoComponents.BuildContext == null || DidimoComponents.BuildContext.MeshHierarchyRoot == null)
            {
                animationComponent = gameObject.GetComponentInChildren<Animation>();

                if (animationComponent == null)
                {
                    animationComponent = gameObject.AddComponent<Animation>();
                }
            }
            else
            {
                ComponentUtility.GetOrAdd(DidimoComponents.BuildContext.MeshHierarchyRoot.gameObject, ref animationComponent);
            }

            animationComponent.animatePhysics = false;
            animationComponent.playAutomatically = false;

            nameToPoseMapping = new Dictionary<string, DidimoFaceShape>(animationClips.Length);
            // animationClips = poseAnimationClips;
            // resetAnimationClip = resetPoseAnimationClip;

            // Create state for Reset Pose. Tip in Additive Animations: https://docs.unity3d.com/Manual/AnimationScripting.html
            animationComponent.AddClip(resetAnimationClip, RESET_POSE_NAME);
            AnimationState resetPoseState = animationComponent[RESET_POSE_NAME];
            
            resetPoseState.layer = 0;
            resetPoseState.blendMode = AnimationBlendMode.Blend;
            resetPoseState.wrapMode = WrapMode.Loop;
            resetPoseState.enabled = false;
            resetPoseState.speed = 0f;
            resetPoseState.weight = 1f;
            
            resetFaceShape = new DidimoFaceShape(RESET_POSE_NAME, resetPoseState);
            
            // Create states for all actual poses
            for (int i = 0; i < animationClips.Length; i++)
            {
                AnimationClip poseAnimationClip = animationClips[i];
                string poseAnimationName = poseAnimationClip.name;

                animationComponent.AddClip(poseAnimationClip, poseAnimationName);
                AnimationState poseState = animationComponent[poseAnimationName];
                poseState.layer = i + 1;
                poseState.blendMode = AnimationBlendMode.Additive;
                poseState.wrapMode = WrapMode.Clamp;
                poseState.enabled = false;
                poseState.speed = 0f;
                poseState.weight = 1f;
                
                DidimoFaceShape didimoFaceShape = new DidimoFaceShape(poseAnimationName, poseState);
                NameToPoseMapping.Add(poseAnimationName, didimoFaceShape);
            }
        }
        

        
        // Search for mapping of animation - Pose clip
        protected string GetMappedFaceShapeName(string controlNodeName) => namePoseAliases.TryGetValue(controlNodeName, out string faceShapeName) ? faceShapeName : controlNodeName;

        protected void SetPoseNormalizedWeight(ref DidimoFaceShape didimoFaceShape, float normalizedValue)
        {
            // Prepare for cleanup after sampling
            if (!poseDataWasUpdated)
            {
                poseDataWasUpdated = true;
                resetFaceShape.AnimationState.normalizedTime = 0f;
                resetFaceShape.AnimationState.enabled = true;
            }
            
            didimoFaceShape.AnimationState.normalizedTime = normalizedValue;
            didimoFaceShape.AnimationState.enabled = true;
            shapesToDisable.Add(didimoFaceShape.Name);
        }
        

        public override bool SetWeightForPose(string source, string poseName, float poseWeight)
        {
            return SetWeightForPose($"{source}_{poseName}", poseWeight);
        }
        
        public override bool SetWeightForPose(string poseName, float poseWeight)
        {
            string faceShapeName = GetMappedFaceShapeName(poseName);

            if (!NameToPoseMapping.TryGetValue(faceShapeName, out DidimoFaceShape faceShape))
            {
                // Debug.Log($"Couldn't find control name {controlNodeName}");
                return false;
            }
            
            SetPoseNormalizedWeight(ref faceShape, poseWeight);
            return true;
        }

        public override void ResetAllPoseWeights()
        {
            foreach (string faceShapeNames in NameToPoseMapping.Keys)
            {
                DidimoFaceShape faceShape = NameToPoseMapping[faceShapeNames];
                SetPoseNormalizedWeight(ref faceShape, 0f);
            }
        }
        
        
        public void CleanupEnabledAnimationStates()
        {
            // No new data
            if (!poseDataWasUpdated || shapesToDisable.Count == 0) return;
            
            // Cleanup: Disable reset pose + all previously enabled poses
            resetFaceShape.AnimationState.enabled = false;

            foreach (string shapeName in shapesToDisable)
            {
                if (!NameToPoseMapping.TryGetValue(shapeName, out DidimoFaceShape faceShape)) continue;
                faceShape.AnimationState.enabled = false;
            }
            shapesToDisable.Clear();
            poseDataWasUpdated = false;
        }
        
        
        // HEAD ROTATION METHODS
        public override bool SetHeadRotation(Quaternion rotation)
        {
            if (headJoint == null) return false;

            // We set the head rotation in rotation to the "idle" pose. i.e.: if Identity rotation, then the face is in the rest pose
            rotation = initialHeadJointTransform.Rotation * rotation;
            if (headJointWeight < 1) rotation = Quaternion.Lerp(initialHeadJointTransform.Rotation, rotation, headJointWeight);
            headJoint.localRotation = rotation;
            return true;
        }

        public override void ResetHeadRotation()
        {
            if (headJoint == null) return;
            headJoint.localRotation = initialHeadJointTransform.Rotation;
        }
        
        
        // General methods
        public void LateUpdate()
        {
            CleanupEnabledAnimationStates();
        }
        
        public override void ForceUpdateAnimation()
        {
            animationComponent.Sample();
            CleanupEnabledAnimationStates();
        }
    }
}