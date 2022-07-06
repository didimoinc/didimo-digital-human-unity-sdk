using Didimo.Builder;
using Didimo.Core.Config;
using Didimo.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Didimo.Core.Config.ShaderResources;

namespace Didimo.Core.Editor
{
    public static class EditorMaterialUtility
    {
        public static string GetMeshFilePath(Mesh mesh)
        {
            string meshFileNameFull = AssetDatabase.GetAssetPath(mesh);
            if (meshFileNameFull != "")
            {
                var meshFilePath = Path.GetDirectoryName(meshFileNameFull);
                return meshFilePath;
            }
            else
            {
                Directory.CreateDirectory("\\assets\\temp\\");
                return "Assets\\temp\\";
            }
        }

        public static Material[] GetAppropriateMaterialList(GameObject[] objects, Renderer comp, int MaterialSwitcherIdx, bool generateIfNotPresent, bool forceCreateMaterialEvenIfPresent)
        {
            DidimoMaterialSwitcher dms = comp.GetComponent<DidimoMaterialSwitcher>();
            if (dms)
            {
                //here we try and grab an input set that's appropraite for atlasing - we prefer the combined set for this job and if that's not created already, it should be.
                if ((generateIfNotPresent) && (!dms.HasValidEntry(MaterialSwitcherIdx)))
                {
                    if (MaterialSwitcherIdx == DidimoMaterialSwitcher.COMBINED_MATERIAL_SET)
                    {
                        GenerateMergedTextureMaterialsForSelected(objects, DidimoMaterialSwitcher.COMBINED_MATERIAL_SET, forceCreateMaterialEvenIfPresent);
                    }
                }
                return dms.MaterialSets[MaterialSwitcherIdx].materials;
            }
            else //If no material switchers are present, just grab the renderer's material list
            {
                return comp.sharedMaterials;
            }
        }

        public static void ForceReloadOfShaderOnMaterial(Material mat)
        {
            var path = AssetDatabase.GetAssetPath(mat.shader);            
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);            
        }

        public static void ForceReloadOfShadersOnObjects(GameObject[] gameObjects)
        {
            Material [] materials = MaterialUtility.GetUniqueMaterialList(gameObjects);
            foreach (var m in materials)
                ForceReloadOfShaderOnMaterial(m);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// This creates a single merged, atlased material from several input materials. It requires materials to previously have been merged (if it detects they aren't, that function is called)
        /// 
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="materialSwitcherSlot"></param>
        public static void GenerateMergedAndAtlasedTextureMaterialsForSelected(GameObject[] objects, int materialSwitcherSlot = DidimoMaterialSwitcher.COMBINED_ATLASED_MATERIAL_SET)
        {
            ShaderResources shaderResources = ResourcesLoader.ShaderResources();
            Material[] altasMaterialSlots = new Material[16];
            int i = 0;
            Dictionary<String, int> UniqueMaterialNames = new Dictionary<String, int>();
            foreach (var ob in objects)
            {
                Renderer r = ob.GetComponent<Renderer>();
                if (r != null)
                {
                    Material m = r.sharedMaterial;
                }
            }

            string[] matGUIDS = UnityEditor.AssetDatabase.FindAssets("AtlasedCombinedSkinMaterial");
            if (matGUIDS.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(matGUIDS[0]);
                Material mergedAtlasedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                MaterialUtility.EnsureMaterialSwitcher(objects, true);
                if (mergedAtlasedMaterial)
                {
                    foreach (GameObject go in objects)
                    {
                        var smrlist = MaterialUtility.AddToArrayIfNotNullAndUnique(go.GetComponentsInChildren<SkinnedMeshRenderer>(), go.GetComponent<SkinnedMeshRenderer>());
                        var mrlist = MaterialUtility.AddToArrayIfNotNullAndUnique(go.GetComponentsInChildren<MeshRenderer>(), go.GetComponent<MeshRenderer>());

                        //do we have an instancing helper?
                        DidimoInstancingHelper dih = Didimo.Core.Utility.ComponentUtility.FindParentThatImplements<DidimoInstancingHelper>(go);
                        foreach (var smr in smrlist)
                        {
                            Material[] ml = GetAppropriateMaterialList(objects, smr, DidimoMaterialSwitcher.COMBINED_MATERIAL_SET, true, false);
                            MaterialUtility.MergedAtlasedInner(smr, ml.ToArray(), dih, i, mergedAtlasedMaterial, altasMaterialSlots, materialSwitcherSlot);
                        }
                        foreach (var mr in mrlist)
                        {
                            Material[] ml = GetAppropriateMaterialList(objects, mr, DidimoMaterialSwitcher.COMBINED_MATERIAL_SET, true, false);
                            MaterialUtility.MergedAtlasedInner(mr, ml.ToArray(), dih, i, mergedAtlasedMaterial, altasMaterialSlots, materialSwitcherSlot);
                        }
                    }
                    var baseMap = EditorTextureUtility.CreateSerialisedTextureAtlas(altasMaterialSlots, "_BaseMap", new Vector2Int(4096, 4096), new Vector2Int(4, 4), "Assets/Didimo.Experimental/Core/Content/Textures/DiffuseTextureAtlas.png", TextureUtility.PROC_CREATE_EVEN_IF_EXISTS | TextureUtility.PROC_LINEAR_COLOURSPACE, TextureUtility.MODE_LINEAR_TO_SRGB);
                    var normalMap = EditorTextureUtility.CreateSerialisedTextureAtlas(altasMaterialSlots, "_NormalMap", new Vector2Int(4096, 4096), new Vector2Int(4, 4), "Assets/Didimo.Experimental/Core/Content/Textures/NormalTextureAtlas.png", TextureUtility.PROC_CREATE_EVEN_IF_EXISTS | TextureUtility.PROC_LINEAR_COLOURSPACE, TextureUtility.MODE_RG_NORMAL_TO_XYZ);
                    var RPCAOMap = EditorTextureUtility.CreateSerialisedTextureAtlas(altasMaterialSlots, "_Rough_Spec_Cavity_AO", new Vector2Int(4096, 4096), new Vector2Int(4, 4), "Assets/Didimo.Experimental/Core/Content/Textures/Rough_Spec_Cavity_AOTextureAtlas.png", TextureUtility.PROC_CREATE_EVEN_IF_EXISTS | TextureUtility.PROC_LINEAR_COLOURSPACE | TextureUtility.PROC_LOAD_ORIGINAL_IMAGE, TextureUtility.MODE_LINEAR_TO_SRGB);

                    mergedAtlasedMaterial.SetTexture("_BaseMap", baseMap);
                    //mergedAtlasedMaterial.SetTexture("_NormalMap", normalMap);
                    mergedAtlasedMaterial.SetTexture("_Rough_Spec_Cavity_AO", RPCAOMap);
                }
            }
        }

        public struct STextureMergeSpec
        {
            public string MaterialPropName;
            public string MergedFileName;
            public string[] SubChannelMapNames;
            public STextureMergeSpec(string mpn, string mfn, string[] scmn)
            {
                MaterialPropName = mpn;
                MergedFileName = mfn;
                SubChannelMapNames = scmn;
            }
        }
        //checks if there are materials with a name appropriate for merging

        static Dictionary<String, String[]> gMaterialNameToMergedName = new Dictionary<String, String[]>()
        {
            { "skin", new String[]{"CombinedSkinMaterial", "AtlasedCombinedSkinMaterial" } },
            { "bodySkin", new String[]{"CombinedBodySkinMaterial", "AtlasedCombinedBodySkinMaterial"} }
        };
        static STextureMergeSpec[] gTextureMergeSpecs = new[]
        {
            new STextureMergeSpec("_Rough_Spec_Cavity_AO","Rough_Spec_Cavity_AO.png", new string[]{ "_RoughnessMap", "_SpecularMap", "_CavityMap", "_AmbientOcclusionMap" }),
            new STextureMergeSpec("_Trans_Bias_SSSAO","Trans_Bias_SSSAO.png", new string[] { "_TransMap", "_ZBiasMap", "_SssAoMask", "_AlphaMask" })
        };
 
        static String[] gMergedTexturePrefixes = new string[] { "Combined", "AtlasedCombined" };


        static String GetAppropriatePrefixNameFromMaterials(Material[] materials, int level)
        {
            foreach (var m in materials)
            {
                if (gMaterialNameToMergedName.ContainsKey(m.name))
                {
                    return gMergedTexturePrefixes[level];
                }
            }
            return "";
        }


        public static void AssignMergedMaterialPropertiesFromSeparated(Material sourceMaterial, Material targetMaterial, string newMaterialPath, string newTexturePrefix, bool forceCreateTextures)
        {
            var dirName = Path.GetDirectoryName(newMaterialPath);

            var sourceShader = sourceMaterial.shader;
            var newShader = targetMaterial.shader;
            int propCount = newShader.GetPropertyCount();
            for (var i = 0; i < propCount; ++i)
            {
                var propName = newShader.GetPropertyName(i);
                var sourcePropIdx = sourceShader.FindPropertyIndex(propName);
                if (sourcePropIdx != -1)
                {
                    MaterialBuilder.CopyMaterialProperty(sourceMaterial, sourcePropIdx, targetMaterial, i);
                }
            }

            for (var i = 0; i < gTextureMergeSpecs.Length; ++i)
            {
                var mspec = gTextureMergeSpecs[i];
                if (targetMaterial.HasProperty(mspec.MaterialPropName))
                {
                    var fname = dirName + Path.DirectorySeparatorChar + newTexturePrefix + "_" + mspec.MergedFileName;
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                    Texture newTexture = EditorTextureUtility.CreateSerialisedMergedTextureFromMaterial(sourceMaterial, fname, mspec.SubChannelMapNames, forceCreateTextures ? TextureUtility.PROC_CREATE_EVEN_IF_EXISTS : 0);
                    targetMaterial.SetTexture(mspec.MaterialPropName, newTexture);
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                }
            }
        }


        public static string GetCombinedMeshFileNameFromMesh(Mesh mesh, Material[] ml, string materialName)
        {

            string meshFileNameFull = AssetDatabase.GetAssetPath(mesh);
            string materialFileNameFull = ml.Length > 0 ? AssetDatabase.GetAssetPath(ml[0]) : "";

            var meshFilePath = meshFileNameFull != "" ? Path.GetDirectoryName(meshFileNameFull) :
                      (materialFileNameFull != "" ? Path.GetDirectoryName(materialFileNameFull) :
                                                    "Assets\\Didimo.Experimental\\Core\\Content\\Materials");

            return meshFilePath + "\\" + materialName + ".mat";

        }

        public static Material[] BuildUniqueMaterials(Material[] ml, Dictionary<Material, Material> uniqueMaterials, string materialFileName, string newMergedTexturePrefix, Shader oldShader, Shader newShader, bool forceCreateMaterialEvenIfPresent, bool forceCreateTextures)
        {
            List<Material> newMatList = new List<Material>();
            for (var i = 0; i < ml.Length; ++i)
            {
                Material newMat = null;
                var m = ml[i];
                if (m.shader == oldShader)
                {
                    if (!uniqueMaterials.ContainsKey(m))
                    {
                        if (forceCreateMaterialEvenIfPresent || !File.Exists(materialFileName))
                        {
                            newMat = CreateMaterialCopy(m, materialFileName, newMergedTexturePrefix, newShader, forceCreateTextures);
                        }
                        else
                        {
#if UNITY_EDITOR
                            newMat = AssetDatabase.LoadAssetAtPath(materialFileName, typeof(Material)) as Material;
                            AssignMergedMaterialPropertiesFromSeparated(m, newMat, materialFileName, newMergedTexturePrefix, forceCreateTextures);
#endif
                        }
                    }
                    else
                        newMat = uniqueMaterials[m];
                }
                var lmat = newMat ? newMat : m;
                uniqueMaterials[m] = lmat;
                newMatList.Add(lmat);
            }
            return newMatList.ToArray();
        }


        public static Material CreateMaterialCopy(Material sourceMaterial, string newMaterialPath, string newTexturePrefix, Shader newShader, bool forceCreateTextures)
        {
            if (newShader != null)
            {
                //an attempt to get unity to behave rather than fail silently
                var mat = new Material(newShader);
                var dir = Directory.GetParent(newMaterialPath).FullName;
                Directory.CreateDirectory(dir);

                AssetDatabase.CreateAsset(mat, newMaterialPath);

                Debug.Log("Creating material: " + newMaterialPath);

                AssignMergedMaterialPropertiesFromSeparated(sourceMaterial, mat, newMaterialPath, newTexturePrefix, forceCreateTextures);

                EditorUtility.SetDirty(mat);
                Debug.Log(MaterialUtility.PrintMaterialContents(mat));
                AssetDatabase.Refresh();
                Material reloadedMaterial = (Material)AssetDatabase.LoadAssetAtPath(newMaterialPath, typeof(Material));
                return reloadedMaterial;

            }
            else
            {
                return new Material(sourceMaterial);
            }
        }

        public static void GenerateMergedTextureMaterialsForSelected(GameObject[] objects, int materialSwitcherSlot = DidimoMaterialSwitcher.COMBINED_MATERIAL_SET, bool forceCreateMaterial = false, bool forceCreateTextures = false)
        {
            Dictionary<Material, Material> uniqueMaterials = new Dictionary<Material, Material>();
            ShaderResources shaderResources = ResourcesLoader.ShaderResources();
            Shader separateSkinShader = shaderResources.Skin;
            Shader mergedSkinShader = shaderResources.SkinMergedTextures;
            MaterialUtility.EnsureMaterialSwitcher(objects, true);
            foreach (GameObject go in objects)
            {
                try
                {
                    var smrlist = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                    var mrlist = go.GetComponentsInChildren<MeshRenderer>();
                    foreach (var smr in smrlist)
                    {
                        Material[] ml = GetAppropriateMaterialList(objects, smr, 0, true, forceCreateMaterial);
                        string preFix = GetAppropriatePrefixNameFromMaterials(ml, DidimoMaterialSwitcher.COMBINED_MATERIAL_SET - 1);
                        var meshName = smr.sharedMesh.name;
                        string materialName = ml[0].name;
                        materialName = char.ToUpper(materialName[0]) + (materialName.Length > 1 ? materialName.Substring(1) : "");

                        materialName = materialName + "_" + preFix;
                        var materialFileName = GetCombinedMeshFileNameFromMesh(smr.sharedMesh, ml, materialName);
                        var matlist = BuildUniqueMaterials(ml, uniqueMaterials, materialFileName, materialName, shaderResources.Skin, shaderResources.SkinMergedTextures, forceCreateMaterial, forceCreateTextures);
                        DidimoMaterialSwitcher dms = smr.GetComponent<DidimoMaterialSwitcher>();
                        if (dms)
                        {
                            Undo.RecordObject(dms, "setting material switch entries");
                            dms.SetEntryMaterials(materialSwitcherSlot, matlist);
                        }
                        Undo.RecordObject(smr, "setting materials");
                        smr.sharedMaterials = matlist;
                    }
                    foreach (var mr in mrlist)
                    {
                        Material[] ml = GetAppropriateMaterialList(objects, mr, 0, true, forceCreateMaterial);
                        string preFix = GetAppropriatePrefixNameFromMaterials(ml, DidimoMaterialSwitcher.COMBINED_MATERIAL_SET - 1);
                        var mf = mr.GetComponent<MeshFilter>();
                        var materialFileName = GetCombinedMeshFileNameFromMesh(mf.sharedMesh, ml, preFix);
                        var matlist = BuildUniqueMaterials(ml, uniqueMaterials, materialFileName, materialFileName, shaderResources.Skin, shaderResources.SkinMergedTextures, forceCreateMaterial, forceCreateTextures);
                        DidimoMaterialSwitcher dms = mr.GetComponent<DidimoMaterialSwitcher>();
                        if (dms)
                        {

                            Undo.RecordObject(dms, "setting material switch entries");
                            dms.SetEntryMaterials(materialSwitcherSlot, matlist);
                        }
                        Undo.RecordObject(mr, "setting materials");
                        mr.sharedMaterials = matlist;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            //ReplaceMaterials(objects, uniqueMaterials, true, materialSwitcherSlot);
        }


        static string[] pipelineSuffix = { "_URP", "_HDRP", "_SRP" };

        public static void FixUpMaterialsToCurrentPipeline(GameObject[] objects, bool forceCreate = false)
        {
            var ResourceID = ResourcesLoader.GetAppropriateID();
            var OtherID = (EPipelineType)((int)ResourceID ^ 1);
            ShaderResources currentResources = ResourcesLoader.ShaderResources(ResourceID);
            ShaderResources otherResources = ResourcesLoader.ShaderResources(OtherID);

            objects = ComponentUtility.CreateFlatList(objects).ToArray();

            foreach (GameObject go in objects)
            {
                Renderer render = go.GetComponent<Renderer>();
                if (render != null)
                {
                    EBodyPartID bpid = GetBodyPartID(render.name);
                    if (bpid == EBodyPartID.UNKNOWN)
                    {
                        bpid = GetBodyPartID(render.transform.parent.name);                    
                    }
                    if (bpid == EBodyPartID.UNKNOWN)
                    {       
                        Debug.Log("Found Unknown");
                    }
                    if (bpid == EBodyPartID.HAIR)
                    {
                        Debug.Log("Found Hair");
                    }

                    

                    if (bpid == EBodyPartID.UNKNOWN)
                    {
                        if (render.sharedMaterial != null)
                            bpid = GetBodyPartID(render.sharedMaterial.name);
                        if (bpid == EBodyPartID.UNKNOWN)
                            Debug.LogError("unknown body part found");
                    }
                    if (bpid == EBodyPartID.EYELASHES)
                    {
                        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
                        render.shadowCastingMode = ShadowCastingMode.Off;
                    }
                    List<Material> inml = new List<Material>();
                    List<Material> outml = new List<Material>();
                    render.GetSharedMaterials(inml);
                    foreach (var m in inml)
                    {
                        if (bpid == EBodyPartID.CLOTHING)
                        {
                            m.SetFloat("_DoubleSidedEnable", 1.0f);
                        }

                      

                        var shaderIdx = otherResources.GetIndexOfShader(m.shader);
                        Mesh mesh = MeshUtils.GetMeshFromRenderer(render);

                        if (shaderIdx == -1)
                        {
                            var currentShaderIdx = currentResources.GetIndexOfShader(m.shader);
                            if (currentShaderIdx != -1)
                            {

                                if (bpid != EBodyPartID.UNKNOWN)
                                    MaterialUtility.FixupDefaultShaderParams(m, bpid);

                                outml.Add(m);
                            }
                            else if (bpid != EBodyPartID.UNKNOWN)
                            {
                                Material sourceMaterial = render.sharedMaterial;

                                string bpname = BodyPartMaterialNames[(int)bpid];
                                Shader matShader = currentResources.GetShader(bpid, ShaderResources.EShaderType.SEPARATE_CHANNELS);

                                if ((matShader != null) && (bpid != EBodyPartID.UNKNOWN))
                                {
                                    Material NewMaterial = new Material(matShader);
                                    //first, copy material properties from old shader
                                    MaterialBuilder.CopyMaterialProperties(sourceMaterial, NewMaterial, ShaderResources.MaterialPropertyAliasMap);
#if UNITY_EDITOR
                                    var meshFilePath = GetMeshFilePath(mesh);
                                    var materialFileName = meshFilePath + Path.DirectorySeparatorChar + bpname + ".mat" + pipelineSuffix[(int)ResourceID] + ".mat";
                                    materialFileName = IOUtility.NormalizePath(materialFileName);
                                    AssetDatabase.CreateAsset(NewMaterial, materialFileName);
                                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialFileName);
                                    MaterialUtility.FixupDefaultShaderParams(NewMaterial, bpid);
#else
                                    Material mat = m;
#endif
                                    outml.Add(mat);
                                }
                                else
                                {
                                    outml.Add(m);
                                }
                            }
                        }
                        else
                        {
                            var newMatName = m.name.Replace(".mat", "");
                            foreach (var pls in pipelineSuffix) //clear any previous suffix names                            
                                newMatName = newMatName.Replace(pls, "");

                            var meshFilePath = GetMeshFilePath(mesh);
                            var materialFileName = meshFilePath + Path.DirectorySeparatorChar + newMatName + pipelineSuffix[(int)ResourceID] + ".mat";
                            materialFileName = IOUtility.NormalizePath(materialFileName);
                            Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialFileName);

                            if (mat && !forceCreate)
                            {
                                outml.Add(mat);
                            }
                            else
                            {
                                Shader NewPipelineshader = currentResources.GetShaderByIndex(shaderIdx);
                                if (NewPipelineshader != null)
                                {
                                    Material HDRPMaterial = new Material(NewPipelineshader);
                                    MaterialBuilder.CopyMaterialProperties(m, HDRPMaterial);
                                    AssetDatabase.CreateAsset(HDRPMaterial, materialFileName);
                                    mat = AssetDatabase.LoadAssetAtPath<Material>(materialFileName);
                                    MaterialUtility.FixupDefaultShaderParams(mat, bpid);
                                    outml.Add(mat);
                                }
                            }
                        }
                    }
                    render.sharedMaterials = outml.ToArray();
                }
            }
        }

    }
}