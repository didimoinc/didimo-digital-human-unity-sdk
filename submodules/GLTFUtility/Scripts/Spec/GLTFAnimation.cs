using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Didimo.GLTFUtility.Converters;
using UnityEngine;
using UnityEngine.Scripting;

namespace Didimo.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#animation
	/// <summary> Contains info for a single animation clip </summary>
	[Preserve] public class GLTFAnimation {
		/// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
		[JsonProperty(Required = Required.Always)] public Channel[] channels;
		[JsonProperty(Required = Required.Always)] public Sampler[] samplers;
		public string name;

#region Classes
		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#animation-sampler
		[Preserve] public class Sampler {
			/// <summary> The index of an accessor containing keyframe input values, e.g., time. </summary>
			[JsonProperty(Required = Required.Always)] public int input;
			/// <summary> The index of an accessor containing keyframe output values. </summary>
			[JsonProperty(Required = Required.Always)] public int output;
			/// <summary> Valid names include: "LINEAR", "STEP", "CUBICSPLINE" </summary>
			[JsonConverter(typeof(EnumConverter))] public InterpolationMode interpolation = InterpolationMode.LINEAR;
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#channel
		/// <summary> Connects the output values of the key frame animation to a specific node in the hierarchy </summary>
		[Preserve] public class Channel {
			/// <summary> Target sampler index </summary>
			[JsonProperty(Required = Required.Always)] public int sampler;
			/// <summary> Target sampler index </summary>
			[JsonProperty(Required = Required.Always)] public Target target;
		}

		// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#target
		/// <summary> Identifies which node and property to animate </summary>
		[Preserve] public class Target {
			/// <summary> Target node index.</summary>
			public int? node;
			/// <summary> Which property to animate. Valid names are: "translation", "rotation", "scale", "weights" </summary>
			[JsonProperty(Required = Required.Always)] public string path;
		}

		[Preserve] public class ImportResult {
			public AnimationClip clip;
		}
#endregion

		public ImportResult Import(GLTFAccessor.ImportResult[] accessors, GLTFNode.ImportResult[] nodes, ImportSettings importSettings, bool isDidimo, ResetAnimationImportResult resetResult) {
			bool multiRoots = nodes.Count(x => x.IsRoot) > 1;

			ImportResult result = new ImportResult();
			result.clip = new AnimationClip();
			result.clip.name = name;
			result.clip.frameRate = importSettings.animationSettings.frameRate;

			// Didimos require legacy animations
			result.clip.legacy = isDidimo || importSettings.animationSettings.useLegacyClips;

			for (int i = 0; i < channels.Length; i++) {
				Channel channel = channels[i];
				if (samplers.Length <= channel.sampler) {
					Debug.LogWarning($"GLTFUtility: Animation channel points to sampler at index {channel.sampler} which doesn't exist. Skipping animation clip.");
					continue;
				}
				Sampler sampler = samplers[channel.sampler];

				// Get interpolation mode
				InterpolationMode interpolationMode = importSettings.interpolationMode;
				if (interpolationMode == InterpolationMode.ImportFromFile) {
					interpolationMode = sampler.interpolation;
				}
				if (interpolationMode == InterpolationMode.CUBICSPLINE) Debug.LogWarning("Animation interpolation mode CUBICSPLINE not fully supported, result might look different.");

				string relativePath = "";

				GLTFNode.ImportResult node = nodes[channel.target.node.Value];
				while (node != null && !node.IsRoot) {
					if (string.IsNullOrEmpty(relativePath)) relativePath = node.transform.name;
					else relativePath = node.transform.name + "/" + relativePath;

					if (node.parent.HasValue) node = nodes[node.parent.Value];
					else node = null;
				}

				// If file has multiple root nodes, a new parent will be created for them as a final step of the import process. This parent messes up the curve relative paths.
				// Add node.transform.name to path if there are multiple roots. This is not the most elegant fix but it works.
				// See GLTFNodeExtensions.GetRoot
				if (multiRoots) relativePath = node.transform.name + "/" + relativePath;

				System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
				float[] keyframeInput = accessors[sampler.input].ReadFloat().ToArray();
				switch (channel.target.path) {
					case "translation":
						Vector3[] pos = accessors[sampler.output].ReadVec3().ToArray();
						AnimationCurve posX = new AnimationCurve();
						AnimationCurve posY = new AnimationCurve();
						AnimationCurve posZ = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							posX.AddKey(CreateKeyframe(k, keyframeInput, pos, x => -x.x, interpolationMode));
							posY.AddKey(CreateKeyframe(k, keyframeInput, pos, x => x.y, interpolationMode));
							posZ.AddKey(CreateKeyframe(k, keyframeInput, pos, x => x.z, interpolationMode));
						}

						if (importSettings.animationSettings.compressLinearAnimationKeyFrames
							&& (interpolationMode != InterpolationMode.LINEAR
								|| IsTransformAnimationCurveUseful(posX, posY, posZ, importSettings.animationSettings.minimumTranslationThreshold)))
						{
							result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
							result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
							result.clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);

							// If Didimo, create reset pose
							if (isDidimo && !resetResult.visitedProperties.Contains($"{relativePath}/{channel.target.path}"))
							{
								if (posX.length > 0 && posY.length > 0 && posZ.length > 0)
								{
									// Rest Pose is the 1st keyframe of each clip
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", new AnimationCurve(posX[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", new AnimationCurve(posY[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localPosition.z",  new AnimationCurve(posZ[0]));
								}
								resetResult.visitedProperties.Add($"{relativePath}/{channel.target.path}");
							}
						}

						break;
					case "rotation":
						Vector4[] rot = accessors[sampler.output].ReadVec4().ToArray();
						AnimationCurve rotX = new AnimationCurve();
						AnimationCurve rotY = new AnimationCurve();
						AnimationCurve rotZ = new AnimationCurve();
						AnimationCurve rotW = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							// The Animation window in Unity shows keyframes incorrectly converted to euler. This is only to deceive you. The quaternions underneath work correctly
							rotX.AddKey(CreateKeyframe(k, keyframeInput, rot, x => x.x, interpolationMode));
							rotY.AddKey(CreateKeyframe(k, keyframeInput, rot, x => -x.y, interpolationMode));
							rotZ.AddKey(CreateKeyframe(k, keyframeInput, rot, x => -x.z, interpolationMode));
							rotW.AddKey(CreateKeyframe(k, keyframeInput, rot, x => x.w, interpolationMode));
						}

						if (importSettings.animationSettings.compressLinearAnimationKeyFrames
							&& (interpolationMode != InterpolationMode.LINEAR
								|| IsRotationAnimationCurveUseful(rotX, rotY, rotZ, rotW, importSettings.animationSettings.minimumRotationThreshold)))
						{
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
							result.clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);

							// If Didimo, create reset pose
							if (isDidimo && !resetResult.visitedProperties.Contains($"{relativePath}/{channel.target.path}"))
							{
								if (rotX.length > 0 && rotY.length > 0 && rotZ.length > 0 && rotW.length > 0)
								{
									// Rest Pose is the 1st keyframe of each clip
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", new AnimationCurve(rotX[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", new AnimationCurve(rotY[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", new AnimationCurve(rotZ[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", new AnimationCurve(rotW[0]));
								}

								resetResult.visitedProperties.Add($"{relativePath}/{channel.target.path}");
							}
						}
						break;
					case "scale":
						Vector3[] scale = accessors[sampler.output].ReadVec3().ToArray();
						AnimationCurve scaleX = new AnimationCurve();
						AnimationCurve scaleY = new AnimationCurve();
						AnimationCurve scaleZ = new AnimationCurve();
						for (int k = 0; k < keyframeInput.Length; k++) {
							scaleX.AddKey(CreateKeyframe(k, keyframeInput, scale, x => x.x, interpolationMode));
							scaleY.AddKey(CreateKeyframe(k, keyframeInput, scale, x => x.y, interpolationMode));
							scaleZ.AddKey(CreateKeyframe(k, keyframeInput, scale, x => x.z, interpolationMode));
						}

						if (importSettings.animationSettings.compressLinearAnimationKeyFrames
							&& (interpolationMode != InterpolationMode.LINEAR
								|| IsTransformAnimationCurveUseful(scaleX, scaleY, scaleZ, importSettings.animationSettings.minimumScaleChangeThreshold)))
						{
							result.clip.SetCurve(relativePath, typeof(Transform), "localScale.x", scaleX);
							result.clip.SetCurve(relativePath, typeof(Transform), "localScale.y", scaleY);
							result.clip.SetCurve(relativePath, typeof(Transform), "localScale.z", scaleZ);

							// If Didimo, create reset pose
							if (isDidimo && !resetResult.visitedProperties.Contains($"{relativePath}/{channel.target.path}"))
							{
								if (scaleX.length > 0 && scaleY.length > 0 && scaleZ.length > 0)
								{
									// Rest Pose is the 1st keyframe of each clip
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localScale.x", new AnimationCurve(scaleX[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localScale.y", new AnimationCurve(scaleY[0]));
									resetResult.clip.SetCurve(relativePath, typeof(Transform), "localScale.z", new AnimationCurve(scaleZ[0]));
								}

								resetResult.visitedProperties.Add($"{relativePath}/{channel.target.path}");
							}
						}

						break;
					case "weights":
						// From: https://github.com/Siccity/GLTFUtility/pull/76/commits
						GLTFNode.ImportResult skinnedMeshNode = nodes[channel.target.node.Value];
						SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshNode.transform.GetComponent<SkinnedMeshRenderer>();

						int numberOfBlendshapes = skinnedMeshRenderer.sharedMesh.blendShapeCount;
						AnimationCurve[] blendshapeCurves = new AnimationCurve[numberOfBlendshapes];
						for (int j = 0; j < numberOfBlendshapes; j++) {
							blendshapeCurves[j] = new AnimationCurve();
						}

						float[] weights = accessors[sampler.output].ReadFloat().ToArray();
						float[] previouslyKeyedValues = new float[numberOfBlendshapes];

						// Reference for my future self:
						// keyframeInput.Length = number of keyframes
						// keyframeInput[ k ] = timestamp of keyframe
						// weights.Length = number of keyframes * number of blendshapes
						// weights[ j ] = actual animated weight of a specific blend shape
						// (index into weights[] array accounts for keyframe index and blend shape index)

						for (int k = 0; k < keyframeInput.Length; k++) {
							for (int j = 0; j < numberOfBlendshapes; j++) {
								int weightIndex = k * numberOfBlendshapes + j;
								float weightValue = weights[weightIndex];

								bool addKey = true;
								if (importSettings.animationSettings.compressBlendShapeKeyFrames) {
									if (k == 0 || !Mathf.Approximately(weightValue, previouslyKeyedValues[j])) {
										previouslyKeyedValues[j] = weightValue;
										addKey = true;
									}
									else {
										addKey = false;
									}
								}

								if (addKey) {
									blendshapeCurves[j].AddKey(CreateKeyframeForMorphWeights(keyframeInput[k], weightValue, interpolationMode));
								}
							}
						}

						for (int j = 0; j < numberOfBlendshapes; j++) {
							string propertyName = $"blendShape.{skinnedMeshRenderer.sharedMesh.GetBlendShapeName(j)}";
							if (!isDidimo || interpolationMode != InterpolationMode.LINEAR || IsAnimationCurveUseful(blendshapeCurves[j]))
							{
								result.clip.SetCurve(relativePath, typeof(SkinnedMeshRenderer), propertyName, blendshapeCurves[j]);

								// If Didimo, create reset pose
								if (isDidimo && !resetResult.visitedProperties.Contains($"{relativePath}/{propertyName}/{channel.target.path}"))
								{
									if (blendshapeCurves[j].length > 0)
									{
										// Rest Pose is the 1st keyframe of each clip
										resetResult.clip.SetCurve(relativePath, typeof(SkinnedMeshRenderer), propertyName, new AnimationCurve(blendshapeCurves[j][0]));
									}
									resetResult.visitedProperties.Add($"{relativePath}/{propertyName}/{channel.target.path}");
								}
							}
						}
						break;
				}
			}
			return result;
		}

		public static Keyframe CreateKeyframe<T>(int index, float[] timeArray, T[] valueArray, Func<T, float> getValue, InterpolationMode interpolationMode) {
			float time = timeArray[index];
			Keyframe keyframe;
#pragma warning disable CS0618
			if (interpolationMode == InterpolationMode.STEP) {
				keyframe = new Keyframe(time, getValue(valueArray[index]), float.PositiveInfinity, float.PositiveInfinity, 1, 1);
			} else if (interpolationMode == InterpolationMode.CUBICSPLINE) {
				// @TODO: Find out what the right math is to calculate the tangent/weight values.
				float inTangent = getValue(valueArray[index * 3]);
				float outTangent = getValue(valueArray[(index * 3) + 2]);
				keyframe = new Keyframe(time, getValue(valueArray[(index * 3) + 1]), inTangent, outTangent, 1, 1);
			} else { // LINEAR
				float currentValue = getValue(valueArray[index]);

				//Calculate in tangent (left/from previous keyframe)
				float inTangent;
				if (index == 0) inTangent = 0;
				else inTangent = (currentValue - getValue(valueArray[index - 1])) / (time - timeArray[index - 1]);

				//Calculate out tangent (right/to next keyframe)
				float outTangent;
				if (index == timeArray.Length - 1) outTangent = 0;
				else outTangent = (getValue(valueArray[index + 1]) - currentValue) / (timeArray[index + 1] - time);

				keyframe = new Keyframe(time, currentValue, inTangent, outTangent);
				// keyframe.weightedMode = WeightedMode.None;
			}
			return keyframe;
		}


		public static Keyframe CreateKeyframeForMorphWeights(float time, float value, InterpolationMode interpolationMode) {
			Keyframe keyframe;
#pragma warning disable CS0618
			if (interpolationMode == InterpolationMode.STEP) {
				keyframe = new Keyframe(time, value, float.PositiveInfinity, float.PositiveInfinity, 1, 1);
			}
			else if (interpolationMode == InterpolationMode.CUBICSPLINE) {
				// @TODO: Find out what the right math is to calculate the tangent/weight values.
				/*
				float inTangent = getValue(valueArray[index * 3]);
				float outTangent = getValue(valueArray[(index * 3) + 2]);
				keyframe = new Keyframe(time, getValue(valueArray[(index * 3) + 1]), inTangent, outTangent, 1, 1);
				*/
				// For now, just let Unity do whatever it does by default :)
				keyframe = new Keyframe(time, value);
			}
			else { // LINEAR
				// NOTE: THIS IS NOT LINEAR! This sets horizontal tangents
				keyframe = new Keyframe(time, value, 0, 0);
				keyframe.weightedMode = WeightedMode.None;
			}
#pragma warning restore CS0618
			return keyframe;
		}

		public static bool IsAnimationCurveUseful(AnimationCurve animationCurve, float differenceEpsilon=0.01f)
		{
			if (animationCurve.length == 0) return false;

			float referenceValue = animationCurve[0].value;
			// Only useful if any significant change in relation to rest pose
			return animationCurve.keys.Any(keyframe => Mathf.Abs(keyframe.value - referenceValue) > differenceEpsilon);
		}

		public static bool IsTransformAnimationCurveUseful(AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float differenceEpsilon = 0.01f)
		{
			if (curveX.length < 1 || curveY.length < 1 || curveZ.length < 1) return false;

			Vector3 restPosePosition = new Vector3(curveX[0].value, curveY[0].value, curveZ[0].value);
			for (int keyIx = 1; keyIx < curveX.length; keyIx++)
			{
				Vector3 deformedPosition = new Vector3(curveX[keyIx].value, curveY[keyIx].value, curveZ[keyIx].value);
				if (IsVector3Useful(deformedPosition - restPosePosition, differenceEpsilon)) return true;
			}
			return false;
		}

		public static bool IsVector3Useful(Vector3 vector, float epsilon)
		{
			return Mathf.Abs(vector.x) > epsilon || Mathf.Abs(vector.y) > epsilon || Mathf.Abs(vector.z) > epsilon;
		}

		public static bool IsRotationAnimationCurveUseful(AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, AnimationCurve curveW, float differenceEpsilon = 1f)
		{
			if (curveX.length != curveY.length || curveX.length != curveZ.length || curveX.length != curveW.length)
			{
				throw new ArgumentException("Quaternion animation curves don't have the same length");
			}

			if (curveX.length < 1) return false;

			Quaternion restPoseRotation = new Quaternion(curveX[0].value, curveY[0].value, curveZ[0].value, curveW[0].value);
			for (int keyIx = 1; keyIx < curveX.length; keyIx++)
			{
				Quaternion deformedRotation = new Quaternion(curveX[keyIx].value, curveY[keyIx].value, curveZ[keyIx].value, curveW[keyIx].value);
				if (Quaternion.Angle(deformedRotation, restPoseRotation) > differenceEpsilon) return true;
			}
			return false;
		}

		[Preserve] public class ResetAnimationImportResult : ImportResult
		{
			public readonly HashSet<string> visitedProperties = new HashSet<string>();
		}
	}

	public static class GLTFAnimationExtensions {
		public static GLTFAnimation.ImportResult[] Import(this List<GLTFAnimation> animations, GLTFAccessor.ImportResult[] accessors, GLTFNode.ImportResult[] nodes, ImportSettings importSettings, bool isDidimo, out GLTFAnimation.ResetAnimationImportResult resetAnimation)
		{
			resetAnimation = null;
			if (animations == null) return null;

			GLTFAnimation.ImportResult[] results = new GLTFAnimation.ImportResult[animations.Count];
			if (isDidimo)
			{
				resetAnimation = new GLTFAnimation.ResetAnimationImportResult();
				resetAnimation.clip = new AnimationClip();
				resetAnimation.clip.name = "RESET_POSE";
				resetAnimation.clip.legacy = true; // importSettings.animationSettings.useLegacyClips;
				resetAnimation.clip.frameRate = importSettings.animationSettings.frameRate;
			}

			for (int i = 0; i < animations.Count; i++) {
				results[i] = animations[i].Import(accessors, nodes, importSettings, isDidimo, resetAnimation);
				if (string.IsNullOrEmpty(results[i].clip.name)) results[i].clip.name = "animation" + i;
			}

			return results;
		}
	}
}