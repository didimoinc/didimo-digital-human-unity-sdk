using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Didimo.Core.Deformables
{
    public static class DeformableUtils
    {

        private const string PUBLIC_DATABASE   = "DeformableDatabase";
        private const string EXPERIMENTAL_DATABASE = "ExperimentalDeformableDatabase";

        private static List<Deformable> GetDeformablesFromDatabase(string databaseName)
        {
            List<Deformable> result = new List<Deformable>();
            DeformableDatabase deformableDatabase = Resources.Load<DeformableDatabase>(databaseName);
            if(deformableDatabase != null)
            {
                result = new List<Deformable>(deformableDatabase.Deformables);
                // Unity has a bug, where when we import multiple glTF didimos at the same time, the second call to Resources.Load might return missing reference exceptions
                // If we unload the asset, it doesn't happen anymore
                Resources.UnloadAsset(deformableDatabase);
            }

            return result;
        }

        public static List<Deformable> GetPublicDeformables() => GetDeformablesFromDatabase(PUBLIC_DATABASE);
        public static List<Deformable> GetExperimentalDeformables() => GetDeformablesFromDatabase(EXPERIMENTAL_DATABASE);

        public static List<Deformable> GetAllDeformables()
        {
            List<Deformable> deformables = GetPublicDeformables();
            deformables.AddRange(GetExperimentalDeformables());
            
            return deformables;
        }


#if UNITY_EDITOR
        private static List<string> GetDeformablePathsFromDeformables(List<Deformable> deformables)
        {
            return deformables.Select(deformable => AssetDatabase.GetAssetPath(deformable.GetComponentInChildren<MeshFilter>().sharedMesh)).ToList();
        }
        public static List<string> GetPublicDeformablePaths() => GetDeformablePathsFromDeformables(GetPublicDeformables());
        public static List<string> GetExperimentalDeformablePaths() => GetDeformablePathsFromDeformables(GetExperimentalDeformables());
        public static List<string> GetAllDeformablePaths() => GetDeformablePathsFromDeformables(GetAllDeformables());
#endif
    }
}