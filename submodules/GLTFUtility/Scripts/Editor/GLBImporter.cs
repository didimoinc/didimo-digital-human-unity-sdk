
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Didimo.GLTFUtility {
	//[ScriptedImporter(1, "glb")]
	public class GLBImporter : GLTFImporter {

		public override void OnImportAsset(AssetImportContext ctx) {
			// Load asset
			if (importSettings == null) importSettings = new ImportSettings();
			Importer.ImportResult importResult = Importer.LoadFromFile(ctx.assetPath, importSettings, Format.GLB);
			// Save asset
			GLTFAssetUtility.SaveToAsset(importResult.rootObject, importResult.animationClips, ctx, importSettings);
		}
	}
}