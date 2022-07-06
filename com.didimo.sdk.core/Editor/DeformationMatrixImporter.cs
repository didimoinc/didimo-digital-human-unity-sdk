using System.IO;
using UnityEditor.AssetImporters;
using Didimo.Core.Deformer;

namespace Didimo.Core.Editor
{
    [ScriptedImporter(1, new string[] {"npz", "dmx"}, importQueueOffset: 50)]
    public class DeformationMatrixImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            byte[] data = File.ReadAllBytes(ctx.assetPath);
            ByteAsset byteAsset = new (data);
            ctx.AddObjectToAsset("DeformationByteData", byteAsset);
            ctx.SetMainObject(byteAsset);
        }
    }
}