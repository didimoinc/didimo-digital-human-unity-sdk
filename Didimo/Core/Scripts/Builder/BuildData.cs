using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Didimo.Speech;

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
            RootDirectory = IOUtility.SanitisePath(Path.GetDirectoryName(didimoFilePath));
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
            context.DidimoComponents.gameObject.AddComponent<DidimoDeformables>();
            context.DidimoComponents.gameObject.AddComponent<DidimoMaterials>();

            context.DidimoComponents.gameObject.SetActive(true);
        }
    }
}