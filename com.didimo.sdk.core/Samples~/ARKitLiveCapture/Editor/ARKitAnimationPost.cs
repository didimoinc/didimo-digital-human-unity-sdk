using UnityEditor;

namespace Didimo.LiveCapture.Editor
{
    public class ARKitAnimationPost : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                if (path.Contains(".anim"))
                {
                    ARKitAutomaticSetup.SetupAnimation(path);
                }
            }
        }
    }
}