#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Didimo.Oculus.Example
{
    /// <summary>
    /// Class that validates if the Oculus Integration package is installed.
    /// If this is the case, enables the rest of the files on this Oculus module
    /// to be compiled.
    /// </summary>
    public static class OculusAssemblyValidator
    {

        private const string OCULUS_DEFINE_NAME = "USING_OCULUS_INTEGRATION_PACKAGE";
        // ASMDef does not keep their references/names properly if they don't exist
        private static readonly string[] REQUIRED_OCULUS_ASSEMBLIES =
        {
            "Oculus.VR",
            "Oculus.LipSync"
        };

        [DidReloadScripts]
        public static void OnInitialize()
        {
            // We can only check if Oculus' Package exists dynamically.
            string[] currentAssemblies = System.AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToArray();

            if (REQUIRED_OCULUS_ASSEMBLIES.All(e => currentAssemblies.Contains(e)))
            {
                // Add version defines named USING_OCULUS_INTEGRATION_PACKAGE
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                if (!defines.Contains(OCULUS_DEFINE_NAME))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, new []{defines, OCULUS_DEFINE_NAME});
                }
            }
            else
            {
                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                string[] defines = definesString.Split(';');
                if (defines.Contains(OCULUS_DEFINE_NAME))
                {
                    Debug.LogWarning("Could not find required Oculus assemblies in project. Disabling Didimo.Oculus module");
                    List<string> cleanDefines = new List<string>(defines);
                    cleanDefines.Remove(OCULUS_DEFINE_NAME);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, cleanDefines.ToArray());
                }
            }
        }
    }
}
#endif
