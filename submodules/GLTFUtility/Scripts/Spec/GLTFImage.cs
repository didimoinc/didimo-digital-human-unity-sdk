using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace Didimo.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#image
	[Preserve] public class GLTFImage {
		/// <summary>
		/// The uri of the image.
		/// Relative paths are relative to the .gltf file.
		/// Instead of referencing an external file, the uri can also be a data-uri.
		/// The image format must be jpg or png.
		/// </summary>
		public string uri;
		/// <summary> Either "image/jpeg" or "image/png" </summary>
		public string mimeType;
		public int? bufferView;
		public string name;

		public class ImportResult {
			public byte[] bytes;
			public string path;

			public ImportResult(byte[] bytes, string path = null) {
				this.bytes = bytes;
				this.path = path;
			}

			public IEnumerator CreateTextureAsync(bool linear, bool normalMap, Action<Texture2D> onFinish, Action<float> onProgress = null) {
				if (!string.IsNullOrEmpty(path)) {

#if UNITY_EDITOR
					// Must be relative path with forward slashes
					string pathFormatted = path.Replace("\\", "/");
					if (pathFormatted.StartsWith(Application.dataPath))
					{
						pathFormatted = "Assets/" + path.Substring(Application.dataPath.Length);

						// Load textures from asset database if we can
						Texture2D assetTexture = AssetDatabase.LoadAssetAtPath(pathFormatted, typeof(Texture2D)) as Texture2D;

						if (assetTexture != null) {
							TextureImporter textureImporter = AssetImporter.GetAtPath(pathFormatted) as TextureImporter;
							textureImporter!.sRGBTexture = !linear;
							textureImporter!.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
							// We only save them at the end, to prevent errors of loading this texture again after calling import

							onFinish(assetTexture);
							if (onProgress != null) onProgress(1f);
							yield break;
						}
					}
#endif


#if !UNITY_EDITOR && ( UNITY_ANDROID || UNITY_IOS )
					path = "File://" + path;
#endif
					using(UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(new Uri(path), true)) {
						UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();
						float progress = 0;
						// This is a workaround to prevent getting an issue on the downloadhandler for textures
						// where Unity gets stuck with isDone=false, even when it has all the data read
						while (!uwr.isDone && uwr.downloadProgress < 1f) {
							if (progress != uwr.downloadProgress) {
								if (onProgress != null) onProgress(uwr.downloadProgress);
							}
							yield return null;
						}

						if (onProgress != null) onProgress(1f);

						if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)  {
							Debug.LogError($"GLTFImage.cs ToTexture2D() ERROR: {uwr.error}");
						} else {
							// Continuation of the workaround. GetContent would check for isDone
							// So we do the same as that function without checking the isDone
							Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
							tex.LoadImage(uwr.downloadHandler.data, true);
							tex.name = Path.GetFileNameWithoutExtension(path);
							onFinish(tex);
						}
						uwr.Dispose();
					}
				} else {
					Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
					if (!tex.LoadImage(bytes)) {
						Debug.Log("mimeType not supported");
						yield break;
					} else onFinish(tex);
				}
			}
		}

		public class ImportTask : Importer.ImportTask<ImportResult[]> {
			public ImportTask(List<GLTFImage> images, string directoryRoot, GLTFBufferView.ImportTask bufferViewTask) : base(bufferViewTask) {
				task = new Task(() => {
					// No images
					if (images == null) return;

					Result = new ImportResult[images.Count];
					for (int i = 0; i < images.Count; i++) {
						string fullUri = directoryRoot + images[i].uri;
						if (!string.IsNullOrEmpty(images[i].uri)) {
							if (File.Exists(fullUri)) {
								// If the file is found at fullUri, read it
								byte[] bytes = File.ReadAllBytes(fullUri);
								Result[i] = new ImportResult(bytes, fullUri);
							} else if (images[i].uri.StartsWith("data:")) {
								// If the image is embedded, find its Base64 content and save as byte array
								string content = images[i].uri.Split(',').Last();
								byte[] imageBytes = Convert.FromBase64String(content);
								Result[i] = new ImportResult(imageBytes);
							}
						} else if (images[i].bufferView.HasValue && !string.IsNullOrEmpty(images[i].mimeType)) {
							GLTFBufferView.ImportResult view = bufferViewTask.Result[images[i].bufferView.Value];
							byte[] bytes = new byte[view.byteLength];
							view.stream.Position = view.byteOffset;
							view.stream.Read(bytes, 0, view.byteLength);
							Result[i] = new ImportResult(bytes);
						} else {
							Debug.Log("Couldn't find texture at " + fullUri);
						}
					}
				});
			}
		}
	}
}