using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Didimo.Speech;
using Didimo.Core.Deformables;
using Didimo.Core.Deformer;
using Didimo.Core.Utility;
using UnityEditor;

namespace Didimo.Builder
{
    // The constructed object that actually builds a didimo
    public abstract class BuildData
    {
        private const string DEFAULT_DIDIMO_GO_NAME = "Didimo";
        public string DidimoKey { get; }
        public string RootDirectory { get; }

        protected BuildData(string didimoKey, string didimoFilePath)
        {
            DidimoKey = didimoKey;
            RootDirectory = string.IsNullOrEmpty(didimoFilePath) ? string.Empty
                : IOUtility.SanitisePath(Path.GetDirectoryName(didimoFilePath));
        }

        public abstract Task<(bool success, DidimoComponents didimo)> Build(Configuration configuration);

        protected void OnBeforeBuild(Configuration configuration, out DidimoBuildContext context)
        {
            // Create the Didimo root object.
            Transform rootTransform = new GameObject(DEFAULT_DIDIMO_GO_NAME).transform;
            rootTransform.SetParent(configuration.Parent, false);

            // Add the didimo component.
            DidimoComponents didimoComponents = rootTransform.gameObject.AddComponent<DidimoComponents>();
            didimoComponents.DidimoKey = DidimoKey;
            didimoComponents.gameObject.SetActive(false);

            // Prepare a build context for the given didimo.
            context = DidimoBuildContext.CreateNew(didimoComponents, RootDirectory);
        }

        protected void OnAfterBuild(Configuration configuration, DidimoBuildContext context)
        {
            context.DidimoComponents.gameObject.AddComponent<DidimoSpeech>();
            DidimoDeformables deformables = context.DidimoComponents.gameObject.AddComponent<DidimoDeformables>();
            deformables.CacheHairOffsets();
            deformables.deformationFile = GetDeformationFile(context.RootDirectory);
            context.DidimoComponents.gameObject.AddComponent<DidimoMaterials>();
            context.DidimoComponents.gameObject.SetActive(true);
        }


        private static string FindDeformationFile(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory)) return null;
            // Find a file in the root folder that is a .dmx or .npz
            return Directory.EnumerateFiles(rootDirectory, "*.*", new EnumerationOptions {IgnoreInaccessible = true, MatchCasing = MatchCasing.CaseInsensitive}).
                             FirstOrDefault(s => s.EndsWith(".dmx", StringComparison.InvariantCultureIgnoreCase) ||
                                                 s.EndsWith(".npz", StringComparison.InvariantCultureIgnoreCase));
        }

        private static TextAsset GetDeformationFile(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory)) return null;

            string deformationFilePath = FindDeformationFile(rootDirectory);
            if (string.IsNullOrEmpty(deformationFilePath)) return null;

#if UNITY_EDITOR
            TextAsset deformationFile = AssetDatabase.LoadAssetAtPath<TextAsset>(deformationFilePath.Replace('\\', '/'));
            if (deformationFile != null) return deformationFile;
#endif
            if (File.Exists(deformationFilePath)) return new ByteAsset(File.ReadAllBytes(deformationFilePath));
            return null;
        }
    }
}