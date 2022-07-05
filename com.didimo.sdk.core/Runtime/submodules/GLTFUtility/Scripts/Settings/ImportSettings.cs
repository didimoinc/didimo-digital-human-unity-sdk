using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Didimo.GLTFUtility
{
	[Serializable]
	public class ImportSettings
	{
		[HideInInspector]
		public bool needsReimportForTextures;
		public enum AnimationType
		{
			// None,
			Legacy,
			Generic,
			Humanoid
		}

		public enum AvatarDefinition
		{
			NoAvatar,
			CreateFromThisModel,
			CopyFromAnotherAvatar
		}

		[Serializable]
		public enum ModelImporterNormalsTangents
		{
			/// <summary>
			///   <para>Import vertex normals/tangents from model file (default). If they don't exist, they are recalculated</para>
			/// </summary>
			Import,

			/// <summary>
			///   <para>Calculate vertex normals/tangents.</para>
			/// </summary>
			Calculate,

			/// <summary>
			///   <para>Do not import vertex normals/tangents.</para>
			/// </summary>
			None
		}

		public bool materials = true;

		[FormerlySerializedAs("shaders")]
		public ShaderSettings shaderOverrides = new ShaderSettings();

		public AnimationSettings            animationSettings = new AnimationSettings();
		public bool                         generateLightmapUVs;
		public ModelImporterNormalsTangents normals;
		public ModelImporterNormalsTangents tangents;
		public Func<string, Shader>         shaderForName    = null;
		public Func<Material, bool>			postMaterialCreate = null;
		public AnimationType                animationType    = AnimationType.Generic;
		public AvatarDefinition             avatarDefinition = AvatarDefinition.CreateFromThisModel;
		public Avatar                       avatar;

		[HideInInspector]
		public bool isDidimo;

		[Tooltip("Interpolation mode applied to all keyframe tangents. Use Import From File when mixing modes within an animation.")]
		public InterpolationMode interpolationMode = InterpolationMode.ImportFromFile;
	}
}