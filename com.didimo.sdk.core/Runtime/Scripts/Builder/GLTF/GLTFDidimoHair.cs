using System;
using System.Linq;
using Didimo.Core.Deformables;
using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo.Builder.GLTF
{
    public static class GLTFDidimoHair 
    {

        public static void ApplyHairMaterials(Importer.ImportResult gltfImportResult)
        {

            if (gltfImportResult.hairsObjects == null) return;
            
            foreach (GameObject gltfHairObject in gltfImportResult.hairsObjects)
            {
                Deformable hair = DeformableUtils.GetAllDeformables().FirstOrDefault(d => d.name == gltfHairObject.name);

                if (hair == null)
                {
                    Debug.LogWarning($"Could not find deformable named {gltfHairObject.name}, skipping updating materials.");
                    return;
                }

                Renderer[] databaseRenderers = hair.GetComponentsInChildren<Renderer>();
                Renderer[] gltfRenderers = gltfHairObject.GetComponentsInChildren<Renderer>();

                if (databaseRenderers.Length != gltfRenderers.Length)
                {
                    throw new Exception($"Number of renderers differs for deformable database asset and gltf hair asset, for object {gltfHairObject.name}");
                }

                for (int rendererIdx = 0; rendererIdx < databaseRenderers.Length; rendererIdx++)
                {
                    if (databaseRenderers[rendererIdx].sharedMaterials.Length != gltfRenderers[rendererIdx].sharedMaterials.Length)
                    {
                        throw new Exception("Number of materials differs for deformable database asset and gltf hair asset");
                    }

                    for(int matIdx = 0; matIdx < databaseRenderers[rendererIdx].sharedMaterials.Length; matIdx++)
                    {
                        // We're assuming the order of the materials is consistent
                        Material databaseMaterial = databaseRenderers[rendererIdx].sharedMaterials[matIdx];
                        Material gltfMaterial = gltfRenderers[rendererIdx].sharedMaterials[matIdx];
                        Debug.Log($"Replacing material {gltfMaterial.name} for mesh {databaseRenderers[rendererIdx].name}");

                        gltfMaterial.shader = databaseMaterial.shader;
                        gltfMaterial.CopyPropertiesFromMaterial(databaseMaterial);
                    }
                }
            }
        }
    }
}