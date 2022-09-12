﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Didimo.GLTFUtility
{
	/// <summary> Contains methods for saving a gameobject as an asset </summary>
	public static class GLTFAssetUtility
	{
		public static void SaveToAsset(GameObject root, AnimationClip[] animations, AssetImportContext ctx, ImportSettings settings)
		{
#if UNITY_2018_2_OR_NEWER
			ctx.AddObjectToAsset("main", root);
			ctx.SetMainObject(root);
#else
			ctx.SetMainAsset("main obj", root);
#endif
			MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
			SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
			AddMeshes(filters, skinnedRenderers, ctx, settings.generateLightmapUVs);
			AddMaterials(renderers, skinnedRenderers, ctx);
			AddAnimations(animations, ctx, settings.animationSettings);

			if (settings.avatar != null && (settings.animationType == ImportSettings.AnimationType.Generic || settings.animationType == ImportSettings.AnimationType.Humanoid) &&
				settings.avatarDefinition == ImportSettings.AvatarDefinition.CreateFromThisModel)
			{
				if (string.IsNullOrEmpty(settings.avatar.name))
				{
					settings.avatar.name = "DidimoAvatar";
				}
				AddAsset(ctx, settings.avatar.name, settings.avatar);
			}
		}

		public static void AddMeshes(MeshFilter[] filters, SkinnedMeshRenderer[] skinnedRenderers, AssetImportContext ctx, bool generateLightmapUVs)
		{
			HashSet<Mesh> visitedMeshes = new HashSet<Mesh>();
			for (int i = 0; i < filters.Length; i++)
			{
				Mesh mesh = filters[i].sharedMesh;
				if (generateLightmapUVs) Unwrapping.GenerateSecondaryUVSet(mesh);
				if (visitedMeshes.Contains(mesh)) continue;
				ctx.AddAsset(mesh.name, mesh);
				visitedMeshes.Add(mesh);
			}
			for (int i = 0; i < skinnedRenderers.Length; i++)
			{
				Mesh mesh = skinnedRenderers[i].sharedMesh;
				if (visitedMeshes.Contains(mesh)) continue;
				ctx.AddAsset(mesh.name, mesh);
				visitedMeshes.Add(mesh);
			}
		}

		public static void AddAnimations(AnimationClip[] animations, AssetImportContext ctx, AnimationSettings settings)
		{
			if (animations == null) return;

			// Editor-only animation settings
			foreach (AnimationClip clip in animations)
			{
				AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
				clipSettings.loopTime = settings.looping;
				AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
			}

			HashSet<AnimationClip> visitedAnimations = new HashSet<AnimationClip>();
			for (int i = 0; i < animations.Length; i++)
			{
				AnimationClip clip = animations[i];
				if (visitedAnimations.Contains(clip)) continue;
				ctx.AddAsset(clip.name, clip);
				visitedAnimations.Add(clip);
			}
		}

		public static void AddMaterials(MeshRenderer[] renderers, SkinnedMeshRenderer[] skinnedRenderers, AssetImportContext ctx)
		{
			HashSet<Material> visitedMaterials = new HashSet<Material>();
			HashSet<Texture2D> visitedTextures = new HashSet<Texture2D>();
			for (int i = 0; i < renderers.Length; i++)
			{
				foreach (Material mat in renderers[i].sharedMaterials)
				{
					if (mat == GLTFMaterial.defaultMaterial) continue;
					if (visitedMaterials.Contains(mat)) continue;
					if (string.IsNullOrEmpty(mat.name)) mat.name = "material" + visitedMaterials.Count;
					ctx.AddAsset(mat.name, mat);
					visitedMaterials.Add(mat);

					// Add textures
					foreach (Texture2D tex in mat.AllTextures())
					{
						// Dont add asset textures
						//if (images[i].isAsset) continue;
						if (visitedTextures.Contains(tex)) continue;
						if (AssetDatabase.Contains(tex)) continue;
						if (string.IsNullOrEmpty(tex.name)) tex.name = "texture" + visitedTextures.Count;
						ctx.AddAsset(tex.name, tex);
						visitedTextures.Add(tex);
					}
				}
			}

			for (int i = 0; i < skinnedRenderers.Length; i++)
			{
				foreach (Material mat in skinnedRenderers[i].sharedMaterials)
				{
					if (visitedMaterials.Contains(mat)) continue;
					if (string.IsNullOrEmpty(mat.name)) mat.name = "material" + visitedMaterials.Count;
					ctx.AddAsset(mat.name, mat);
					visitedMaterials.Add(mat);

					// Add textures
					foreach (Texture2D tex in mat.AllTextures())
					{
						// Dont add asset textures
						//if (images[i].isAsset) continue;
						if (visitedTextures.Contains(tex)) continue;
						if (AssetDatabase.Contains(tex)) continue;
						if (string.IsNullOrEmpty(tex.name)) tex.name = "texture" + visitedTextures.Count;
						ctx.AddAsset(tex.name, tex);
						visitedTextures.Add(tex);
					}
				}
			}
		}

		public static void AddAsset(this AssetImportContext ctx, string identifier, Object obj)
		{
#if UNITY_2018_2_OR_NEWER
			ctx.AddObjectToAsset(identifier, obj);
#else
			ctx.AddSubAsset(identifier, obj);
#endif
		}

		public static IEnumerable<Texture2D> AllTextures(this Material mat)
		{
			int[] ids = mat.GetTexturePropertyNameIDs();
			for (int i = 0; i < ids.Length; i++)
			{
				Texture2D tex = mat.GetTexture(ids[i]) as Texture2D;
				if (tex != null) yield return tex;
			}
		}
	}
}