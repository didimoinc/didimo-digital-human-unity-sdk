using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Didimo.GLTFUtility
{
	[Preserve]
	public static class GLTFImageCache
	{
		private static List<TextureReference> textures = new List<TextureReference>();

		private class TextureReference
		{
			public TextureReference(string path, Texture2D texture)
			{
				this.path = path;
				this.texture = new WeakReference<Texture2D>(texture);
			}

			public string                   path;
			public WeakReference<Texture2D> texture;
		}

		public static Texture2D GetTexture(string path)
		{
			TextureReference textureRef = textures.FirstOrDefault(tRef => path == tRef.path);
			if (textureRef == null) return null;

			if (textureRef.texture.TryGetTarget(out Texture2D texture))
			{
				return texture;
			}

			textures.RemoveAll(tr => tr.path == path);
			return null;
		}

		public static void AddTexture(string path, Texture2D texture)
		{
			TextureReference textureReference = textures.FirstOrDefault(rt => rt.path == path);
			if (textureReference.texture.TryGetTarget(out _))
			{
				Debug.LogWarning($"Adding texture cache for texture that already exists: {path}");
				textureReference.texture = new(texture);
			}
			else
			{
				textures.Add(new TextureReference(path, texture));
			}
		}
	}
}