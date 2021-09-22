using UnityEngine;

namespace Didimo.Builder
{
    public class DidimoBuildContext
    {
        public DidimoComponents DidimoComponents { get; private set; }

        public Transform RootTransform { get; private set; }
        public Transform MeshHierarchyRoot { get; set; }

        public float UnitsPerMeter { get; set; } = 1f;
        public string RootDirectory { get; set; }

        public static DidimoBuildContext CreateNew(DidimoComponents didimoComponents, string rootDirectory)
        {
            DidimoBuildContext context = new DidimoBuildContext {DidimoComponents = didimoComponents, RootTransform = didimoComponents.transform, RootDirectory = rootDirectory};

            didimoComponents.BuildContext = context;
            return context;
        }
    }
}