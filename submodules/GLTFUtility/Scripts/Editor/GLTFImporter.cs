using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Didimo.GLTFUtility {
	//[ScriptedImporter(1, "gltf")]
	public class GLTFImporter : ScriptedImporter {

		public ImportSettings importSettings;

		public override void OnImportAsset(AssetImportContext ctx) {
			// Load asset
			if (importSettings == null) importSettings = new ImportSettings();
			Importer.ImportResult importResult = Importer.LoadFromFile(ctx.assetPath, importSettings, Format.GLTF);

			// Save asset
			GLTFAssetUtility.SaveToAsset(importResult.rootObject, importResult.animationClips, ctx, importSettings);
		}
	}
}