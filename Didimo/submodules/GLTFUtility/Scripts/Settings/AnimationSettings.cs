using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.GLTFUtility {
	/// <summary> Defines how animations are imported </summary>
	[Serializable]
	public class AnimationSettings {
		public bool useLegacyClips;

		public bool looping;
		[Tooltip("Sample rate set on all imported animation clips.")]
		public float frameRate = 24;
		[Tooltip("When true, remove redundant keyframes from blend shape animations.")]
		public bool compressBlendShapeKeyFrames = true;

		[Tooltip("When true, compress the animation keyframes for position, rotation and scale by a given threshold")]
		public bool compressLinearAnimationKeyFrames = true;
		[Tooltip("Minimum translation required for the linear curve to not be discarded")]
		public float minimumTranslationThreshold = 0.001f;
		[Tooltip("Minimum rotation required for the linear curve to not be discarded, in degrees")]
		public float minimumRotationThreshold = 1f;
		[Tooltip("Minimum scale change required for the linear curve to not be discarded")]
		public float minimumScaleChangeThreshold = 0.01f;
	}
}