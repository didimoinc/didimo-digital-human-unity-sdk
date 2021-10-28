#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Didimo.Oculus.Example
{
    public static class OculusAssemblyValidator
    {
        
        private const string OCULUS_DEFINE_NAME = "USING_OCULUS_INTEGRATION_PACKAGE";

        [DidReloadScripts]
        public static void OnInitialize()
        {
            // We can only check if Oculus' Package exists dynamically.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string[] oculusRequiredAssemblies = currentAssembly.GetReferencedAssemblies().
                                                                Where(e => e.Name.StartsWith("Oculus")).
                                                                Select(e => e.Name).ToArray();

            string[] currentAssemblies = System.AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToArray();

            if (oculusRequiredAssemblies.All(e => currentAssemblies.Contains(e)))
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
                Debug.LogWarning("Could not find required Oculus assemblies in project. Disabling Didimo.Oculus module");
                string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                string[] defines = definesString.Split(';');
                if (defines.Contains(OCULUS_DEFINE_NAME))
                {
                    List<string> cleanDefines = new List<string>(defines);
                    cleanDefines.Remove(OCULUS_DEFINE_NAME);
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, cleanDefines.ToArray());
                }
            }
        }
    }
}
#endif
