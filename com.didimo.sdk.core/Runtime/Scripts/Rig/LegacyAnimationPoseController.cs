using System.Collections.Generic;
using System.Linq;
using Didimo.Extensions;
using UnityEngine;

namespace Didimo
{
    /// <summary>
    /// Pose Controller class for low-level control of the didimo's facial animations.
    /// This system is built on top of Unity's Legacy Animation system that provides full runtime support.
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public class LegacyAnimationPoseController : DidimoPoseController
    {
        /// <summary>
        /// Class that contains data the state of each pose and its name.
        /// </summary>
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

        private void Awake()
        {
            CacheInitialHeadJointTransform();
        }

        private void CacheInitialHeadJointTransform()
        {
            initialHeadJointTransform = headJoint == null
                ? null
                : new TransformValues(headJoint.localPosition, headJoint.localRotation, headJoint.localScale);
        }

        /// <summary>
        /// Class that contains the transform values of any game object.
        /// Used to keep reference to original transform values of bones so they can be reset
        /// and any change can be recalculated from the original transform.
        /// </summary>
        public class TransformValues
        {
            // public readonly Vector3    Position;
            public readonly Quaternion Rotation;
            // public readonly Vector3    Scale;

            public TransformValues(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                // Position = position;
                Rotation = rotation;
                // Scale = scale;
            }
        }

        public const string RESET_POSE_NAME = "RESET_POSE";
        private const string DEFAULT_POSE_SOURCE = "ARKit";

        public override ESupportedMovements SupportedMovements =>
            ESupportedMovements.Poses | ESupportedMovements.HeadRotation;

        public AnimationClip[] animationClips;
        public AnimationClip resetAnimationClip;
        public Transform headJoint;
        public bool headJointMovementEnabled = true;
        private TransformValues initialHeadJointTransform;


        [Range(0, 1)] public float headJointWeight = 1f;

        // We may not have a 1:1 match between user names and clip/pose names
        // if something changes. Good to allow for config file (ex: eyeBlink_L -> eyeBlinkLeft)
        private readonly Dictionary<string, string> namePoseAliases = new Dictionary<string, string>();
        private Dictionary<string, DidimoFaceShape> nameToPoseMapping;
        private DidimoFaceShape resetFaceShape;

        private bool poseDataWasUpdated = false;
        private readonly HashSet<string> shapesToDisable = new HashSet<string>();
        private bool resetHeadPose = false;


        public Dictionary<string, DidimoFaceShape> NameToPoseMapping
        {
            get
            {
                if ((nameToPoseMapping == null && animationClips.Length > 0 && resetAnimationClip != null))
                {
                    BuildController();
                }

                return nameToPoseMapping;
            }
        }

        /// <summary>
        /// List of all poses included in the didimo for animation
        /// </summary>
        /// <returns>List of all the poses</returns>
        public override IReadOnlyList<string> GetAllIncludedPoses() => NameToPoseMapping.Keys.ToList();

        /// <summary>
        /// Query if the pose with the given name is included in the didimo for animation.
        /// </summary>
        /// <param name="poseName">Name of the pose</param>
        /// <returns>True if the pose is included. False otherwise.</returns>
        public override bool IsPoseIncluded(string poseName) =>
            namePoseAliases.ContainsKey(poseName) || NameToPoseMapping.ContainsKey(poseName);


        /// <summary>
        /// Build the PoseController with the required animation clips and, if necessary/present, a configuration JSON.
        /// This optional configuration JSON should be a map between strings for names of the poses.
        /// </summary>
        /// <param name="poseAnimationClips">Collection of animation clips for the poses</param>
        /// <param name="resetPoseAnimationClip">Animation Clip for rest pose</param>
        /// <param name="headJointTransform">Transform for the head to move with
        /// skeleton animations. Can be null</param>
        /// <param name="animationConfigJson">JSON string with configuration of name mappings. Can be null</param>
        public virtual void BuildController(AnimationClip[] poseAnimationClips,
            AnimationClip resetPoseAnimationClip,
            Transform headJointTransform = null,
            string animationConfigJson = null)
        {
            animationClips = poseAnimationClips;
            resetAnimationClip = resetPoseAnimationClip;
            headJoint = headJointTransform;
            BuildController(animationConfigJson);
        }

        /// <summary>
        /// Build the PoseController with the required animation clips and, if necessary/present, a configuration JSON.
        /// This optional configuration JSON should be a map between strings for names of the poses.
        /// </summary>
        /// <param name="animationConfigJson">Path to JSON file with configuration of
        /// name mappings. Can be null</param>
        public virtual void BuildController(string animationConfigJson = null)
        {
            if (animationClips.IsNullOrEmpty() || resetAnimationClip == null)
            {
                Debug.LogWarning("Cannot build LegacyAnimationPoseController"
                                 + " without providing animationClips and the resetAnimationClip");
                return;
            }

            animationClips = animationClips.Where(clip => clip != null).ToArray();

            CacheInitialHeadJointTransform();

            DidimoComponents.Animation.animatePhysics = false;
            DidimoComponents.Animation.playAutomatically = false;
            nameToPoseMapping = new Dictionary<string, DidimoFaceShape>(animationClips.Length);

            // Create state for Reset Pose. Tip in Additive Animations:
            // https://docs.unity3d.com/Manual/AnimationScripting.html
            if (gameObject.scene.isLoaded)
            {
                // Only do this if this component is in a scene.
                // If we do it on a component from a prefab asset, Unity will have GC handle errors
                DidimoComponents.Animation.AddClip(resetAnimationClip, RESET_POSE_NAME);
                AnimationState resetPoseState = DidimoComponents.Animation[RESET_POSE_NAME];

                resetPoseState.layer = 0;
                resetPoseState.blendMode = AnimationBlendMode.Blend;
                resetPoseState.wrapMode = WrapMode.Loop;
                resetPoseState.enabled = false;
                resetPoseState.speed = 0f;
                resetPoseState.weight = 1f;

                resetFaceShape = new DidimoFaceShape(RESET_POSE_NAME, resetPoseState);
            }

            // Create states for all actual poses
            for (int i = 0; i < animationClips.Length; i++)
            {
                AnimationClip poseAnimationClip = animationClips[i];
                string poseAnimationName = poseAnimationClip.name;

                DidimoComponents.Animation.AddClip(poseAnimationClip, poseAnimationName);
                AnimationState poseState = null;

                if (gameObject.scene.isLoaded)
                {
                    // Only do this if this component is in a scene.
                    // If we do it on a component from a prefab asset, Unity will have GC handle errors
                    poseState = DidimoComponents.Animation[poseAnimationName];
                    poseState.layer = i + 1;
                    poseState.blendMode = AnimationBlendMode.Additive;
                    // Needs to be clamp forever, otherwise values >1 stop pose completely
                    poseState.wrapMode = WrapMode.ClampForever;
                    poseState.enabled = false;
                    poseState.speed = 0f;
                    poseState.weight = 1f;
                }

                DidimoFaceShape didimoFaceShape = new DidimoFaceShape(poseAnimationName, poseState);
                NameToPoseMapping.Add(poseAnimationName, didimoFaceShape);
            }
        }

        /// <summary>
        /// Find the mapped name of the pose name if a mapping was provided when building the controller.
        /// </summary>
        /// <param name="poseName">Name of the pose before the mapping.</param>
        /// <returns>Name of the pose after the mapping.</returns>
        protected string GetMappedFaceShapeName(string poseName)
            => namePoseAliases.TryGetValue(poseName, out string faceShapeName) ? faceShapeName : poseName;


        /// <summary>
        /// Set the normalized weight of a single pose.
        /// This updates the AnimationState time for the next computation of the animation shapes.
        /// The computation of the final face shape is automatically done at the end of the frame by Unity.
        /// This weight value is automatically clamped to [0-1] by the Animation system.
        /// </summary>
        /// <param name="didimoFaceShape">Reference to <c>DidimoFaceShape</c> for the pose.</param>
        /// <param name="normalizedValue">Normalized weight of the pose</param>
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


        /// <summary>
        /// Set the normalized weight for a single pose from a specific source.
        /// The sources are the prefix to each AnimationClip that is in
        /// your didimo, such as ARKit, defaultExpressions, etc.
        /// </summary>
        /// <param name="source">Source of the pose.</param>
        /// <param name="poseName">Name of the pose without source.</param>
        /// <param name="poseWeight">Weight of the pose</param>
        /// <returns>True if the pose was successfully found and its value set.
        /// False otherwise.</returns>
        public override bool SetWeightForPose(string source, string poseName, float poseWeight)
        {
            return SetWeightForPose($"{source}_{poseName}", poseWeight);
        }

        /// <summary>
        /// Set the normalized weight of a single pose.
        /// This weight value is automatically clamped to [0-1] by the Animation system.
        /// </summary>
        /// <param name="poseName">Full name of the pose (must include the prefix of its source)</param>
        /// <param name="poseWeight">Normalized weight of the pose</param>
        public override bool SetWeightForPose(string poseName, float poseWeight)
        {
            string faceShapeName = GetMappedFaceShapeName(poseName);

            if (!NameToPoseMapping.TryGetValue(faceShapeName, out DidimoFaceShape faceShape))
            {
                // Debug.Log($"Couldn't find control name {poseName}");
                return false;
            }

            SetPoseNormalizedWeight(ref faceShape, poseWeight);
            return true;
        }

        /// <summary>
        /// Get the normalized weight of a single pose
        /// Returns -1 if the pose was not found
        /// </summary>
        /// <param name="poseName">Name of the pose to query</param>
        /// <returns>Weight of the pose found. -1 if not found</returns>
        public override float GetPoseWeight(string poseName)
        {
            string faceShapeName = GetMappedFaceShapeName(poseName);
            if (!NameToPoseMapping.TryGetValue(faceShapeName, out DidimoFaceShape faceShape))
            {
                return -1f;
            }

            return faceShape.AnimationState.normalizedTime;
        }

        /// <summary>
        /// Reset all the weights for all poses back to 0, returning the face
        /// to its original neutral look before any animation was applied.
        /// </summary>
        public override void ResetAllPoseWeights()
        {
            foreach (string faceShapeNames in NameToPoseMapping.Keys)
            {
                DidimoFaceShape faceShape = NameToPoseMapping[faceShapeNames];
                SetPoseNormalizedWeight(ref faceShape, 0f);
            }
        }

        /// <summary>
        /// Cleanup the animation states after all the computations from sampling the current states.
        /// This method should only be called if you are manually forcing Unity's
        /// Legacy Animation component to sample.
        /// If you want to force the animation to be sampled, use <c>ForceUpdateAnimation</c> instead
        /// </summary>
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

        /// <summary>
        /// Set the rotation to the didimo's head joint.
        /// This rotation is applied in relation to the original rest pose.
        /// </summary>
        /// <param name="rotation">Rotation to set to the head</param>
        /// <returns>True if the rotation was applied successfully. False otherwise.</returns>
        public override bool SetHeadRotation(Quaternion rotation)
        {
            if (headJoint == null || initialHeadJointTransform == null) return false;
            if (!headJointMovementEnabled || headJointWeight == 0) return true;

            // We set the head rotation in relation to the "idle" pose.
            // i.e.: if Identity rotation, then the face is in the rest pose
            rotation = initialHeadJointTransform.Rotation * rotation;
            if (headJointWeight < 1)
            {
                rotation = Quaternion.Lerp(initialHeadJointTransform.Rotation, rotation, headJointWeight);
            }

            headJoint.localRotation = rotation;

            resetHeadPose = true;
            return true;
        }

        /// <summary>
        /// Reset the head rotation to the original rest pose.
        /// </summary>
        public override void ResetHeadRotation()
        {
            // No head joint
            if (headJoint == null || initialHeadJointTransform == null) return;

            // Not moved or movement was disabled
            if (!headJointMovementEnabled || !resetHeadPose || headJointWeight == 0) return;

            resetHeadPose = false;
            headJoint.localRotation = initialHeadJointTransform.Rotation;
        }


        // General methods
        private void LateUpdate()
        {
            CleanupEnabledAnimationStates();
        }

        /// <summary>
        /// Force the current pose to be sampled and updated by Unity's animation system,
        /// followed by a cleanup of the animation states that were used for the current update.
        /// This method does not need to be called in PlayMode.
        /// </summary>
        public override void ForceUpdateAnimation()
        {
            DidimoComponents.Animation.Sample();
            CleanupEnabledAnimationStates();
        }
    }
}