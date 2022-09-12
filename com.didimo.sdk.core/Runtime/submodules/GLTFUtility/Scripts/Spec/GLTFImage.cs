using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Didimo.GLTFUtility
{
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#image
	[Preserve]
	public class GLTFImage
	{
		/// <summary>
		/// The uri of the image.
		/// Relative paths are relative to the .gltf file.
		/// Instead of referencing an external file, the uri can also be a data-uri.
		/// The image format must be jpg or png.
		/// </summary>
		public string uri;

		/// <summary> Either "image/jpeg" or "image/png" </summary>
		public string mimeType;

		public int?   bufferView;
		public string name;

		protected static string ASSET_ROOT_PATH         = "Assets/";
		protected static string PACKAGES_ROOT_PATH      = "Packages/";
		protected static string PACKAGE_CACHE_ROOT_PATH = "Library/PackageCache/";

		public static readonly string GltfImageSharedPath = Path.Combine(Application.dataPath, "Didimo", "SharedAssets");

		public class ImportResult
		{
			public byte[] bytes;
			public string path;

			public ImportResult(byte[] bytes, string path = null)
			{
				this.bytes = bytes;
				this.path = path;
			}

			public IEnumerator CreateTextureAsync(bool linear, bool normalMap, Action<Texture2D> onFinish, Action<float> onProgress = null)
			{
				if(path == null && bytes == null) yield break;

				Texture2D texture = GLTFImageCache.GetTexture(path);
				if (texture != null)
				{
					// Should onProgress be called?
					onFinish(texture);
					yield break;
				}

				if (!string.IsNullOrEmpty(path))
				{
#if UNITY_EDITOR

					// Must be relative path with forward slashes
					string pathFormatted = path.Replace("\\", "/");

					List<string> rootPaths = new List<string> {ASSET_ROOT_PATH, PACKAGES_ROOT_PATH, PACKAGE_CACHE_ROOT_PATH};

					foreach (string rootPath in rootPaths)
					{
						string rootPathAbsl = Path.GetFullPath(rootPath).Replace("\\", "/");
						if (pathFormatted.StartsWith(rootPathAbsl))
						{
							// In case path contains "../"
							pathFormatted = Path.GetFullPath(pathFormatted);
							pathFormatted = pathFormatted.Substring(rootPathAbsl.Length);
							// Package Caches have name@hash, so we need to strip that out
							if (rootPath == PACKAGE_CACHE_ROOT_PATH)
							{
								string cachedPackageFolderName = pathFormatted.Split('/')[0];
								string packageFolderName = cachedPackageFolderName.Substring(0, cachedPackageFolderName.LastIndexOf('@'));
								pathFormatted = $"{PACKAGES_ROOT_PATH}/{packageFolderName}/" + pathFormatted.Substring(cachedPackageFolderName.Length);
							}
							else
							{
								pathFormatted = rootPath + pathFormatted;
							}

							// Load textures from asset database if we can
							Texture2D assetTexture = AssetDatabase.LoadAssetAtPath(pathFormatted, typeof(Texture2D)) as Texture2D;

							if (assetTexture != null)
							{
								TextureImporter textureImporter = AssetImporter.GetAtPath(pathFormatted) as TextureImporter;
								textureImporter!.sRGBTexture = !linear;
								textureImporter!.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
								// We only save them at the end, to prevent errors of loading this texture again after calling import
								// textureImporter.SaveAndReimport();
								AssetDatabase.WriteImportSettingsIfDirty(pathFormatted);

								onFinish(assetTexture);
								if (onProgress != null) onProgress(1f);
								yield break;
							}
						}
					}

#endif

#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
					if(!path.StartsWith("http")){
						path = "File://" + path;
					}
#endif
					// Don't use UnityWebRequestTexture. It has a bug where isDone is always false.
					using (UnityWebRequest uwr = UnityWebRequest.Get(new Uri(path)))
					{
						uwr.downloadHandler = new DownloadHandlerBuffer();
						uwr.SendWebRequest();
						float progress = 0;
						while (!uwr.isDone)
						{
							if (progress != uwr.downloadProgress)
							{
								progress = uwr.downloadProgress;
								if (onProgress != null) onProgress(uwr.downloadProgress);
							}

							yield return null;
						}

						if (onProgress != null) onProgress(1f);

						if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
						{
							Debug.LogError($"GLTFImage.cs ToTexture2D() ERROR: {uwr.error}");
						}
						else
						{
							Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
							bool isSharedTexture = path.StartsWith("http");
							tex.LoadImage(uwr.downloadHandler.data, !isSharedTexture);
							tex.name = Path.GetFileNameWithoutExtension(path);

							if (isSharedTexture)
							{
								GLTFImageCache.AddTexture(path, tex);
							}

							onFinish(tex);
						}

						uwr.Dispose();
					}
				}
				else
				{
					Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
					if (!tex.LoadImage(bytes))
					{
						Debug.Log("mimeType not supported");
						yield break;
					}
					else onFinish(tex);
				}
			}
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]>
		{
			public bool needsReimport;
#if UNITY_EDITOR
			private static string SharedPath(string imageUri)
			{
				List<char> invalidPathChars = Path.GetInvalidPathChars().ToList();
				invalidPathChars.Add(':');
				string result = Path.Combine(GltfImageSharedPath, string.Concat(imageUri.Split(invalidPathChars.ToArray())));
				return result.Replace('\\', '/').Replace("//", "/");
			}

#endif

			public ImportTask(List<GLTFImage> images, string directoryRoot, GLTFBufferView.ImportTask bufferViewTask) : base(bufferViewTask)
			{
				task = new Task(async () =>
				{
					// No images
					if (images == null) return;

					Result = new ImportResult[images.Count];
					for (int i = 0; i < images.Count; i++)
					{
						string fullUri = Path.Combine(directoryRoot, images[i].uri);
						if (!string.IsNullOrEmpty(images[i].uri))
						{
							if (images[i].uri.StartsWith("http"))
							{
								if (!Application.isPlaying)
								{
#if UNITY_EDITOR
									if (!string.IsNullOrEmpty(directoryRoot))
									{
										fullUri = SharedPath(images[i].uri);
										if (!File.Exists(fullUri))
										{
											using (UnityWebRequest webRequest = UnityWebRequest.Get(images[i].uri))
											{
												UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();
												while (!asyncOperation.isDone) { await Task.Delay(100); }

												Directory.CreateDirectory(Path.GetDirectoryName(fullUri)!);
												File.WriteAllBytes(fullUri, webRequest.downloadHandler.data);
												string assetPath = Path.GetDirectoryName(Application.dataPath);
												assetPath = fullUri.Remove(0, assetPath!.Length + 1);
												// images[i].uri = fullUri;
												needsReimport = true;
												AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
												AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
											}
										}
									}
#endif
								}
								else
								{
									Result[i] = new ImportResult(null, images[i].uri);
									continue;
								}
							}

							if (File.Exists(fullUri))
							{
								// If the file is found at fullUri, read it
								byte[] bytes = File.ReadAllBytes(fullUri);
								Result[i] = new ImportResult(bytes, fullUri);
								continue;
							}
							else if (images[i].uri.StartsWith("data:"))
							{
								// If the image is embedded, find its Base64 content and save as byte array
								string content = images[i].uri.Split(',').Last();
								byte[] imageBytes = Convert.FromBase64String(content);
								Result[i] = new ImportResult(imageBytes);
								continue;
							}
						}
						else if (images[i].bufferView.HasValue && !string.IsNullOrEmpty(images[i].mimeType))
						{
							GLTFBufferView.ImportResult view = bufferViewTask.Result[images[i].bufferView.Value];
							byte[] bytes = new byte[view.byteLength];
							view.stream.Position = view.byteOffset;
							view.stream.Read(bytes, 0, view.byteLength);
							Result[i] = new ImportResult(bytes);
							continue;
						}

						Result[i] = new ImportResult(null);
						Debug.LogWarning($"Couldn't find image {images[i].uri}");
					}
				});
			}
		}
	}
}