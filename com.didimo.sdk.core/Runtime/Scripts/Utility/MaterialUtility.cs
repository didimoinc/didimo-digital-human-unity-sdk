using Didimo.Core.Config;
using Didimo.Core.Deformables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Didimo.Core.Config.ShaderResources;

namespace Didimo.Core.Utility
{



    public static class MaterialUtility
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

   
#if UNITY_EDITOR
        public static void GenerateMaterialForSelected(GameObject[] objects, bool createDiskBasedAsset = true)
        {
            Dictionary<Material, Material> uniqueMaterials = new Dictionary<Material, Material>();
            EPipelineType pipelineID = ResourcesLoader.GetAppropriateID();
            string PipelineSuffix = ResourcesLoader.PipelineName[(int)pipelineID];
            foreach (GameObject go in objects)
            {
                var rlist = go.GetComponentsInChildren<Renderer>();
                List<Material> ml = new List<Material>();
                foreach (var smr in rlist)
                {

                    Mesh mesh = MeshUtils.GetMeshFromRenderer(smr);                    

                    smr.GetSharedMaterials(ml);
                    foreach (var m in ml)
                    {
                        if (!uniqueMaterials.ContainsKey(m))
                        {
                            Material newMat = new Material(m);
                            #if UNITY_EDITOR
                            if (createDiskBasedAsset)
                            {               
                                string meshFileNameFull = AssetDatabase.GetAssetPath(mesh);                    
                                var meshName = Path.GetFileNameWithoutExtension(meshFileNameFull);                    
                                var meshFileName = Path.GetFileName(meshFileNameFull);
                                var meshFilePath = Path.GetDirectoryName(meshFileNameFull);             
                                var materialFileName = meshFilePath + "/" + meshFileName + "_" + PipelineSuffix + ".mat";
                                AssetDatabase.CreateAsset(newMat, materialFileName);
                                newMat = AssetDatabase.LoadAssetAtPath<Material>(materialFileName);
                            }
                            #endif
                            uniqueMaterials[m] = newMat;


                            uniqueMaterials[m] = newMat;
                        }
                    }
                }
            }

            ReplaceMaterials(objects, uniqueMaterials);
        }

#endif

        public static void GenerateAppropriateMaterialForSelected(GameObject[] objects)
        {
            Dictionary<Material, Material> uniqueMaterials = new Dictionary<Material, Material>();

            ShaderResources shaderResources = ResourcesLoader.ShaderResources();

            foreach (GameObject go in objects)
            {
                var smrlist = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                var mrlist = go.GetComponentsInChildren<MeshRenderer>();
                List<Material> ml = new List<Material>();
                foreach (var smr in smrlist)
                {
                    smr.GetSharedMaterials(ml);
                    foreach (var m in ml)
                    {
                        if (!uniqueMaterials.ContainsKey(m))
                        {
                            if ((smr.name.ToLower().Contains("eyelash")) && !m.shader.name.ToLower().Contains("eyelash"))
                            {
                                uniqueMaterials[m] = new Material(shaderResources.Eyelash);
                            }
                            else if ((smr.name.ToLower().Contains("eye") || smr.name.ToLower().Contains("cornea")) && !m.shader.name.ToLower().Contains("eye"))
                            {
                                uniqueMaterials[m] = new Material(shaderResources.Eye);
                            }
                            else if ((smr.name.ToLower().Contains("skin") || smr.name.ToLower().Contains("baseface")) && !m.shader.name.ToLower().Contains("skin"))
                            {
                                uniqueMaterials[m] = new Material(shaderResources.Skin);
                            }
                            else
                                uniqueMaterials[m] = new Material(m);
                        }
                    }
                }
            }

            MaterialUtility.ReplaceMaterials(objects, uniqueMaterials);
        }

        public static Material[] GetUniqueMaterialList(GameObject[] objects)
        {
            Dictionary<Material, int> materialDict = new Dictionary<Material, int>();

            foreach (var go in objects)
            {
                Renderer[] allRenderers = go.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in allRenderers)
                {
                    foreach (var m in r.sharedMaterials)
                        if (materialDict.ContainsKey(m))
                            materialDict[m] += 1;
                        else
                            materialDict[m] = 1;
                }
            }

            return materialDict.Select(o => o.Key).ToArray();
        }
        
        #if UNITY_EDITOR
       /*
                   Ns 96.078431
Ka 0.000000 0.000000 0.000000
Kd 0.640000 0.640000 0.640000
Ks 0.500000 0.500000 0.500000
Ni 1.000000
d 1.000000
illum 2
*/
        static Dictionary<string, KeyValuePair<string, Type> > MTLtoPBRShader = new Dictionary<string,KeyValuePair<string, Type>>()
            {
                ["kd"]= new KeyValuePair<string,Type>("_Albedo", typeof(Color)),
                ["ka"] = new KeyValuePair<string, Type>("_Albedo", typeof(Color)),
                ["ks"] = new KeyValuePair<string, Type>("_Albedo", typeof(Color)),
            };
            
            
        public static List<string> FindUsedMaterialsInObj(string filename)
        {
            string text = System.IO.File.ReadAllText(filename);            
            List<string> result = new List<string>();
            foreach (var l in text.Split("\n"))
            {
                if (l.Trim(' ').StartsWith("usemtl",StringComparison.CurrentCultureIgnoreCase))
                {
                    var items = l.Split(" ");
                    if (items.Length > 1)
                    {
                        var matname = items[1].Trim(' ').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r');
                        if (!result.Contains(matname))
                            result.Add(matname);
                    }
                }                
            }
            return result;
        }
        public static List<Material> ParseMaterialListFromMTLFile(string filename, string[] approvedMaterials = null)
        {               
            List<Material> materialList = new List<Material>();
            string text = System.IO.File.ReadAllText(filename);
            string [] materialdefs = text.Split("newmtl");
            EPipelineType pipelineID = ResourcesLoader.GetAppropriateID();
            ShaderResources shaderResources = ResourcesLoader.ShaderResources(pipelineID);

            for (var i = 0; i < materialdefs.Length; ++i)
            {                
                var matdef = materialdefs[i].Trim(' ');
                if (matdef != "")
                {
                    string [] lines = matdef.Split("\n");
                    var name = lines[0].Trim(' ').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r');

                    if (approvedMaterials ==null || approvedMaterials.Contains(name))
                    {
                        Material newMat = new Material(shaderResources.BasicPBRLitShader);
                        newMat.name = name;

                        for (var j = 1; j < lines.Length; ++j)
                        {
                            var items = lines[j].Trim(' ').TrimEnd('\r').TrimEnd('\n').TrimEnd('\r').Split(" ");
                            var key = items[0].ToLower();
                            KeyValuePair<string, Type> res;
                            if (MTLtoPBRShader.TryGetValue(key, out res))
                            {
                                if (res.Value == typeof(Color) && items.Length >= 4)
                                    newMat.SetColor(res.Key, new Color(float.Parse(items[1]), float.Parse(items[2]), float.Parse(items[3])));                            
                                else if (res.Value == typeof(float) && items.Length >= 2)
                                    newMat.SetFloat(res.Key, float.Parse(items[1]));                        
                            }
                        }
                        materialList.Add(newMat);
                    }
                }                
            }
            return materialList;            
        }
        #endif

        public static void SetBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
            }
        }

        //Necessary low level work-around for setting SSS profiles on new materials. Later releases of Unity have API for this.
        [StructLayout(LayoutKind.Explicit)]
        struct FloatToInt
        {
            [FieldOffset(0)]
            private float f;

            [FieldOffset(0)]
            private int i;

            public static int ConvertToInt(float value) { return new FloatToInt { f = value }.i; }
            public static float ConvertToFloat(int value) { return new FloatToInt { i = value }.f; }
        }

        private static int floatToIntBits(float value) { return FloatToInt.ConvertToInt(value); }

        private static float intToFloatBits(int value) { return FloatToInt.ConvertToFloat(value); }

        private static Vector4 intToFloatBits(Vector4Int value)
        {
            return new Vector4(intToFloatBits(value.x), intToFloatBits(value.y), intToFloatBits(value.z), intToFloatBits(value.w));
        }

        /*
        public static void FixupDefaultShaderParams(Material mat, DidimoParts.BodyPart bpid)
        {
            Vector4 EyeProfileAsset = intToFloatBits(new Vector4Int(263726704, -1537711319, -872588405, -926254750));
            float EyeDiffuseHash = intToFloatBits(1080904598); // 2.066104f;
            float SkinDiffuseHash = intToFloatBits(1074019085); // 2.066104f;
            Vector4 SkinProfileAsset =
                intToFloatBits(new Vector4Int(1306322346,
                    -2065702086,
                    1728913528,
                    -652625823)); // new Vector4(463254800.00f, 0.00f, 666452600000000000000000.00f, -2704275000000000.00f);

            for (var i = 0; i < mat.shader.GetPropertyCount(); ++i)
            {
                var propType = mat.shader.GetPropertyType(i);
                var propName = mat.shader.GetPropertyName(i);

                if (propName == "_DiffusionProfileHash")
                {
                    float hash = mat.GetFloat("_DiffusionProfileHash");
                    mat.SetFloat(propName, (bpid == DidimoParts.BodyPart.EyeLashesMesh || bpid == DidimoParts.BodyPart.RightEyeMesh) ? EyeDiffuseHash : SkinDiffuseHash);
                    Debug.Log("Diffuse profile hash: " + hash.ToString());
                }

                if (propName == "_DiffusionProfileAsset")
                {
                    mat.SetVector(propName, (bpid == DidimoParts.BodyPart.EyeLashesMesh || bpid == DidimoParts.BodyPart.RightEyeMesh) ? EyeProfileAsset : SkinProfileAsset);
                }

                //Ensure that body texture has correct combined, shared effects texture
                if (propName == "_Trans_Bias_SSSAO")
                {
                    if (bpid == DidimoParts.BodyPart.BodyMesh)
                    {
                        Texture tex = mat.GetTexture(propName);

                        if (!tex || !tex.name.ToLower().Contains("BodySkin"))
                        {
#if UNITY_EDITOR
                            var assetPath =
                                "Packages/com.didimo.sdk.core/Runtime/Content/Textures/BodySkin_Combined_Trans_Bias_SSSAO.png"; //first try to load canoncial, shared version
                            Texture2D newAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                            if (newAsset == null)
                            {
                                var texList = AssetDatabase.FindAssets("t:texture BodySkin_Combined_Trans_Bias_SSSAO");
                                if (texList.Length > 0)
                                {
                                    assetPath = AssetDatabase.GUIDToAssetPath(texList[0]);
                                    newAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                                }
                            }

                            mat.SetTexture(propName, newAsset);
#endif
                        }
                    }
                }

                if (bpid == DidimoParts.BodyPart.EyeLashesMesh)
                {
                    if (propName.ToLower().Contains("surface"))
                    {
                        mat.SetFloat(propName, 1.0f);
                    }
                }

                if (propName == "_AlphaClipThreshold")
                    mat.SetFloat(propName, (bpid == DidimoParts.BodyPart.HeadMesh) ? 0.0f : 0.5f);

                if (propType == ShaderPropertyType.Texture)
                {
#if USING_TEXTURE_FIXUP
                    var textureDefaultMap = ShaderResources.BodyPartPropertyMap[(int)bpid];
                    if (textureDefaultMap != null)
                    {
             
                        try
                        {
                            if (textureDefaultMap.ContainsKey(propName)) //do we have an entry for this?
                            {
                                int shadID = Shader.PropertyToID(propName);
                                if (mat.GetTexture(shadID) == null) //Is it empty
                                {
                                    var textureFiles = textureDefaultMap[propName];
                                    var bodyPartSpecificDir = ShaderResources.bodyPartSpecificSearchDirectories[(int)bpid];
                                    string[] prefixes = { "", meshFilePath };
             
                                    prefixes = new List<string>(prefixes).Concat(new List<string>(bodyPartSpecificDir)).ToArray();
             
                                    var failLog = "";
                                    var tex = FindTextureFile(textureFiles, prefixes, failLog);
                                    if (failLog != "")
                                    {
                                        Debug.Log("Tried to load default texture at following locations:" + failLog + "\n but failed");
                                    }
                                    else
                                    {
                                        mat.SetTexture(shadID, tex);
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("Failed to find key " + propName + " in material map for " + mat.name);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                    }
#endif
                }
            }

            if (bpid == DidimoParts.BodyPart.BodyMesh)
            {
                int propID = mat.shader.FindPropertyIndex("_UseAlphaClip");
                if (propID != -1)
                    mat.SetFloat(propID, 1.0f);
            }

            mat.shader = mat.shader;
#if UNITY_EDITOR
            EditorUtility.SetDirty(mat);
#endif
        }*/

        public static int ClassifyHairTextureFromName(string name)
        {
            var sanitisedName = name.ToLower();
            for (int i = 0; i < hair_name_fragments.Length; ++i)
                if (sanitisedName.Contains(hair_name_fragments[i]))
                    return i;
            return -1;
        }

        public static int ClassifyClothTextureFromName(string name)
        {
            var sanitisedName = name.ToLower();
            for (int i = 0; i < cloth_name_fragments.Length; ++i)
                if (sanitisedName.Contains(cloth_name_fragments[i]))
                    return i;
            return -1;
        }

        public struct HairLayerDefaults
        {
            public HairLayerDefaults(float AOFactor_, float AOStrength_)
            {
                _AOFactor = AOFactor_;
                _AOStrength = AOStrength_;
            }

            float _AOFactor;
            float _AOStrength;
        }

        public static bool IsClothName(string name)
        {
            var loname = name.ToLower();
            return (loname.Contains("cloth") || loname.Contains("hat") || loname.Contains("shirt"));
        }

        static HairLayerDefaults[] hairLayerDefaults =
        {
            new HairLayerDefaults(0.1f, 1.0f), new HairLayerDefaults(0.1f, 1.0f), new HairLayerDefaults(1.0f, 0.01f), new HairLayerDefaults(1.0f, 0.01f)
        };

        public static void AddDirectoryIfExists(List<System.IO.DirectoryInfo> directories, string directory)
        {
            if (Directory.Exists(directory))
            {
                var result = directories.Find(x => x.Name == directory);
                if (result == null)
                {
                    directories.Add(new System.IO.DirectoryInfo(directory));
                }
            }
        }

        public static void SetHairMaterialLayerDefaults(Material mat, EHairLayer hairLayer)
        {
            Debug.Assert(hairLayerDefaults.Length >= (int)EHairLayer.COUNT); //make sure we have enough of these 
            if (hairLayer < EHairLayer.COUNT)
            {
                var defaults = hairLayerDefaults[(int)hairLayer];
                foreach (var field in typeof(HairLayerDefaults).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    int idx = mat.shader.FindPropertyIndex(field.Name);
                    if (idx != -1)
                    {
                        if (field.FieldType == typeof(float))
                        {
                            float fval = (float)field.GetValue(defaults);
                            mat.SetFloat(field.Name, fval);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Tried to find hair layer defaults outside of range");
            }
        }

        //TODO: Find a more robust way to determine this, a flag or some indicating feature on the mesh itself
        //Currently this defaults to MSAA alpha-to-coverage shaders for HD hairs and 007, which has many curly layers
        //and can't be made to work with alpha blending
        static bool ShouldUseMSAA(string fileName)
        {
            return (fileName.Contains("hairhd", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.Contains("hair_007", StringComparison.CurrentCultureIgnoreCase) ||
                    fileName.Contains("msaa", StringComparison.CurrentCultureIgnoreCase));
        }

        public static Shader GetHairShaderFromHairLayer(EHairLayer hairLayer, ShaderResources sr, bool usingMSAA)
        {
            if (usingMSAA && (hairLayer != EHairLayer.HAT))
                return sr.HairMSAA;
            switch (hairLayer)
            {
                case EHairLayer.INNER_OPAQUE: return sr.HairOpaque;
                case EHairLayer.INNER_FRINGE: return sr.Hair;
                case EHairLayer.OUTER:        return sr.Hair;
                case EHairLayer.HAT:          return sr.Cloth;
                default:                      return sr.HairMSAA;
            }
        }
#if UNITY_EDITOR
        public static void GenerateHairMaterialsForSelected(GameObject[] objects, bool addHairComponent = true, bool forceRecreate = false,
            EPipelineType pipelineID = EPipelineType.EPT_UNKNOWN)
        {
            if (pipelineID == EPipelineType.EPT_UNKNOWN)
                pipelineID = ResourcesLoader.GetAppropriateID();
            string PipelineSuffix = ResourcesLoader.PipelineName[(int)pipelineID];
            ShaderResources shaderResources = ResourcesLoader.ShaderResources(pipelineID);
            string[] NameSuffixes = { "_inner_opaque", "_inner", "_outer", "_hat", "_unknown" }; //Needs to match up with EHairLayer
            foreach (GameObject go in objects)
            {
                var mrlist = go.GetComponentsInChildren<MeshRenderer>();
                if (go != null)
                {
                    if (addHairComponent && go.GetComponent<Hair>() == null)
                        go.AddComponent<Hair>();
                    foreach (var mr in mrlist)
                    {
                        mr.shadowCastingMode = ShadowCastingMode.Off; //Force shadows to off for new hairs                        
                        Mesh mesh = MeshUtils.GetMeshFromRenderer(mr);
                        string meshFileNameFull = AssetDatabase.GetAssetPath(mesh);

                        if (meshFileNameFull == "")
                        {
                            Debug.Log("Failed to retrieve valid mesh filename, is it a temporary mesh?");
                            continue;
                        }

                        var meshName = Path.GetFileNameWithoutExtension(meshFileNameFull);
                        var meshNameLow = meshName.ToLower();
                        var meshFileName = Path.GetFileName(meshFileNameFull);
                        var meshFilePath = Path.GetDirectoryName(meshFileNameFull);
                        var projectBaseDir = new System.IO.DirectoryInfo(Application.dataPath).Parent.FullName;
                        List<System.IO.DirectoryInfo> dirs = new List<System.IO.DirectoryInfo>();
                        var cDirInfo = new System.IO.DirectoryInfo(meshFilePath);
                        //dirs.Add(cDirInfo);
                        //dirs.Add(cDirInfo.Parent);

                        if (IsClothName(meshName))
                        {
                            AddDirectoryIfExists(dirs, projectBaseDir + "/Packages/com.didimo.sdk.core/Runtime/Content/Deformables/Hats/" + meshName);
                        }
                        else
                        {
                            AddDirectoryIfExists(dirs, projectBaseDir + "/Packages/com.didimo.sdk.core/Runtime/Content/Deformables/Hair/" + meshName);
                            AddDirectoryIfExists(dirs, projectBaseDir + "/Packages/com.didimo.sdk.core/Runtime/Content/Deformables/HairHD/" + meshName);
                            AddDirectoryIfExists(dirs, projectBaseDir + "/Packages/com.didimo.sdk.experimental/Runtime/Content/Deformables/Hair/" + meshName);
                            AddDirectoryIfExists(dirs, projectBaseDir + "/Packages/com.didimo.sdk.experimental/Runtime/Content/Deformables/HairHD/" + meshName);
                            AddDirectoryIfExists(dirs, cDirInfo.Parent.FullName + "/png"); //this is to try and capture the non-conformant HD hair directory structure
                        }

                        List<Material> newMaterials = new List<Material>();
                        int cmat = 0;
                        List<EHairLayer> layerTypes = new List<EHairLayer>();
                        List<bool> usingMSAA = new List<bool>();
                        bool usingMSAAGlobal = ShouldUseMSAA(meshFilePath);

                        //try to determine the relevant slot types
                        foreach (var m in mr.sharedMaterials)
                        {
                            var nameLow = m ? m.name.ToLower() : "";

                            layerTypes.Add(ShaderResources.ClassifyHairLayerFromName(nameLow));
                            usingMSAA.Add(usingMSAAGlobal | ShouldUseMSAA(nameLow));
                            if (layerTypes[cmat] == EHairLayer.UNKNOWN)
                                layerTypes[cmat] = (EHairLayer)Math.Min(cmat, (int)EHairLayer.OUTER);
                            cmat += 1;
                        }

                        //Only create as many layers as the material has slots
                        int hairLayerCount = Math.Min(mr.sharedMaterials.Length, layerTypes.Count);
                        int hairLayerMask = 0x0;
                        
                        if (!forceRecreate)
                        {
                            for (var i = 0; i < hairLayerCount; ++i)
                            {
                                EHairLayer layer = layerTypes[i];
                                Shader hairShader = GetHairShaderFromHairLayer(layer, shaderResources, usingMSAA[i]);

                                var materialFileName = meshFilePath + "/" + meshFileName + NameSuffixes[(int)layer] + "_" + PipelineSuffix + ".mat";

                                var mat = AssetDatabase.LoadAssetAtPath<Material>(materialFileName);
                                if (mat)
                                {
                                    hairLayerMask |= 1 << i;
                                }

                                newMaterials.Add(mat);
                            }

                            if (hairLayerMask == ((1 << hairLayerCount) - 1)) //we got them all, don't bother recreating appropriate material
                            {
                                mr.sharedMaterials = newMaterials.ToArray();
                                break;
                            }
                        }

                        newMaterials.Clear();

                        var FileList = new List<System.IO.FileInfo>();
                        string[] imageExtentions = new string[] { "*.png", "*.tga", "*.jpg", "*.dds", "*.webm" };
                        foreach (var dir in dirs)
                        {
                            foreach (var ext in imageExtentions)
                            foreach (var file in dir.GetFiles(ext, SearchOption.AllDirectories))
                            {
                                FileList.Add(file);
                            }
                        }

                        //In the case of multiple textures with similar names, we need to weight those that match the mesh name before those that do not
                        //(e.g. hair_001_albedo needs to match hair_001.obj more than hair_004_albedo)
                        //This is a fuzzy operation because locations of assets can't be guaranteed and textures can't be determined from obj files as they don't
                        //have all the required slots and the materials don't always refer to the textures at all.
                        Dictionary<string, int> stringScores = new Dictionary<string, int>();
                        for (var i = 0; i < FileList.Count; ++i)
                            stringScores[FileList[i].Name] = IOUtility.StringSimilarityScore(FileList[i].Name.ToLower(), meshNameLow);

                        FileList.Sort(delegate(FileInfo a, FileInfo b)
                        {
                            return stringScores[a.Name] - stringScores[b.Name];
                        });

                        var files = FileList.ToArray();
                        Texture2D[] HairTextures = new Texture2D[(int)EHairTextureType.RAMP + 1];
                        Texture2D[] ClothTextures = new Texture2D[(int)EHairTextureType.RAMP + 1];
                        foreach (var f in files)
                        {
                            if (IsClothName(f.Name))
                            {
                                int texFileClassification = ClassifyClothTextureFromName(f.Name);
                                if (texFileClassification != -1)
                                {
                                    string prechewedFileNameForFussyUnity = IOUtility.FullPathToProjectPath(f.FullName);
                                    ClothTextures[texFileClassification] = (Texture2D)AssetDatabase.LoadAssetAtPath(prechewedFileNameForFussyUnity, typeof(Texture2D));
                                }
                                else
                                {
                                    Debug.Log("Failed to classify a cloth texture with the name '" + f.Name + "'");
                                }
                            }
                            else
                            {
                                int texFileClassification = ClassifyHairTextureFromName(f.Name);
                                if (texFileClassification != -1)
                                {
                                    string prechewedFileNameForFussyUnity = IOUtility.FullPathToProjectPath(f.FullName);
                                    HairTextures[texFileClassification] = (Texture2D)AssetDatabase.LoadAssetAtPath(prechewedFileNameForFussyUnity, typeof(Texture2D));
                                }
                                else
                                {
                                    Debug.Log("Failed to classify a hair texture with the name '" + f.Name + "'");
                                }
                            }
                        }
                        Dictionary<string, Material> alreadyCreatedMaterials = new Dictionary<string, Material>();
                        for (var i = 0; i < hairLayerCount; ++i)
                        {
                            EHairLayer layer = layerTypes[i];
                            Shader shader = GetHairShaderFromHairLayer(layer, shaderResources, usingMSAA[i]);
                            var materialFileName = meshFilePath + "/" + meshFileName + NameSuffixes[(int)layer] + "_" + PipelineSuffix + ".mat";
                            Material mat = null;                            
                            if (!alreadyCreatedMaterials.TryGetValue(materialFileName, out mat))
                            {
                                mat = new Material(shader);                                 
                                AssetDatabase.CreateAsset(mat, materialFileName);
                                var Textures = (layer == EHairLayer.HAT) ? ClothTextures : HairTextures;
                                var shaderTextureNames = (layer == EHairLayer.HAT) ? cloth_material_texture_names : hair_material_texture_names;
                                for (var j = 0; j < Textures.Length; ++j)
                                {
                                    if (Textures[j] != null)
                                    {
                                        int propIdx = mat.shader.FindPropertyIndex(shaderTextureNames[j]);
                                        if (propIdx !=
                                            -1) //the 'propertyIndex' isn't _really_ the property index so we still have to set it via its name (thanks, unity) but does tell us if the property exists at all
                                            mat.SetTexture(shaderTextureNames[j], Textures[j]);
                                        else
                                            Debug.Log("Problem found setting texture parameter '" + shaderTextureNames[j] + "'");
                                    }
                                }

                                if (layer != EHairLayer.HAT)
                                    SetHairMaterialLayerDefaults(mat, layer); //non texture defaults that differ between layers and therefore cannot be stored as shader defaults
                                AssetDatabase.SaveAssets();
                                mat = AssetDatabase.LoadAssetAtPath<Material>(materialFileName);
                                alreadyCreatedMaterials[materialFileName] = mat;
                            }
                            else
                            {
                                Debug.Log("Material at '" + materialFileName + "' already created this session, not re-creating it");
                            }
                            newMaterials.Add(mat);
                        }

                        mr.sharedMaterials = newMaterials.ToArray();
                    }
                }
            }
        }
#endif
        public static string PrintMaterialContents(Material mat)
        {
            var propCount = mat.shader.GetPropertyCount();
            StringBuilder LogString = new StringBuilder("==========================================\n", 46);

#if UNITY_EDITOR

            LogString.AppendFormat("Material properties for: '{0}' ('{1}')\n", mat.name, AssetDatabase.GetAssetPath(mat));
#else
            LogString.AppendFormat("Material properties for: '{0}'\n", mat.name);

#endif
            for (var i = 0; i < propCount; ++i)
            {
                try
                {
                    var propName = mat.shader.GetPropertyName(i);
                    var propType = mat.shader.GetPropertyType(i);
                    var indentName = "   " + propName;
                    if ((propName == "_DiffusionProfileHash") || propName.ToLower().Contains("diffusion"))
                    {
                        int hash = floatToIntBits(mat.GetFloat(propName));
                        LogString.AppendFormat("Diffuse profile hash: {'1'}\n", hash.ToString());
                    }
                    else if (propName == "_DiffusionProfileAsset")
                    {
                        var value = mat.GetVector(propName);
                        Vector4Int intvalue = new Vector4Int(floatToIntBits(value.x), floatToIntBits(value.y), floatToIntBits(value.z), floatToIntBits(value.w));
                        LogString.AppendFormat("Diffuse profile hash: {'1'}\n", intvalue.ToString());
                    }
                    else if (propName.ToLower().Contains("surface"))
                    {
                        float value = mat.GetFloat(propName);
                        LogString.AppendFormat("Surface type found: {0}\n", value.ToString());
                    }
                    else
                    {
                        switch (propType)
                        {
                            case ShaderPropertyType.Color:
                            {
                                Color v = mat.GetColor(propName);
                                LogString.AppendFormat("{0} : {1}\n", indentName, v.ToString());
                                break;
                            }
                            case ShaderPropertyType.Range:
                            case ShaderPropertyType.Float:
                            {
                                float v = mat.GetFloat(propName);
                                LogString.AppendFormat("{0} : {1}\n", indentName, v.ToString());
                                break;
                            }
                            case ShaderPropertyType.Texture:
                            {
                                var v = mat.GetTexture(propName);
#if UNITY_EDITOR
                                LogString.AppendFormat("{0} : {1}\n", indentName, v != null ? $"{v} : {AssetDatabase.GetAssetPath(v)}" : "Empty");
#else
                                LogString.AppendFormat("{0} : {1}\n", indentName, v);
#endif
                                break;
                            }
                            case ShaderPropertyType.Vector:
                            {
                                var v = mat.GetVector(propName);
                                LogString.AppendFormat("{0} : {1}\n", indentName, v.ToString());
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LogString.AppendFormat("!-Exception Found: {0} -! \n", e.Message);
                }
            }

            return LogString.ToString();
        }

        public static void CreateDidimoInstancingHelpers(GameObject [] selection)
        {
            var rootDidimos = selection.SelectMany(x => x.transform.TransformAndAllDecendants().Where(x => x.GetComponent<DidimoComponents>() != null).ToArray()).ToArray();
            int idx = 0;
            foreach (var didimo in rootDidimos)
            {
                DidimoInstancingHelper dih = didimo.GetComponent<DidimoInstancingHelper>();
                if (!dih)
                    dih = didimo.gameObject.AddComponent<DidimoInstancingHelper>();
                if (dih)
                    dih.InstanceIndex = idx;
                ++idx;
            }
        }

        public static Material[] MergedAtlasedInner(Component comp, Material[] ml, DidimoInstancingHelper dih, int cidx, Material mergedAtlasedMaterial,
            Material[] altasMaterialSlots, int MaterialSwitchSlot)
        {
            GameObject go = comp.gameObject;
            DidimoMaterialSwitcher dms = comp.GetComponent<DidimoMaterialSwitcher>();
            if (dih == null) //still no dih? Try the SMR
                dih = comp.GetComponent<DidimoInstancingHelper>();

            ShaderResources shaderResources = ResourcesLoader.ShaderResources();

            Material[] newMatList = new Material[ml.Length];
            for (var i = 0; i < ml.Length; ++i)
            {
                var m = ml[i];

                if (m == null)
                {
                    Debug.Log("NULL material found in material list for " + go.name);
                }
                else
                {
                    if (m.shader == shaderResources.Skin || m.shader == shaderResources.SkinMergedTextures || m.shader == shaderResources.SkinMergedAtlasedTextures)
                    {
                        var idx = -1;
                        if (dih != null)
                            idx = dih.InstanceIndex;
                        else
                            idx = cidx;

                        altasMaterialSlots[idx] = m;
                        newMatList[i] = mergedAtlasedMaterial;
                    }
                    else
                        newMatList[i] = m; //use original entry
                }
            }

            SkinnedMeshRenderer smr = comp.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                smr.sharedMaterials = newMatList;
            MeshRenderer mr = comp.GetComponent<MeshRenderer>();
            if (mr != null)
                mr.sharedMaterials = newMatList;
            if (dms)
            {
#if UNITY_EDITOR
                Undo.RecordObject(dms, "setting material switch entries");
#endif
                dms.SetEntryMaterials(MaterialSwitchSlot, newMatList);
            }

            return newMatList;
        }

        public static T[] AddToArrayIfNotNullAndUnique<T>(T[] a, T b)
        {
            List<T> alist = new List<T>(a);
            if (alist.IndexOf(b) != -1)
                alist.Add(b);

            return alist.ToArray();
        }

        //For now, a simple test for body or face meshes but this may need to become more sophisticated in the future
        public static bool IsRendererAtlasable(Renderer r)
        {
            if (r.gameObject.name.Contains("face", StringComparison.CurrentCultureIgnoreCase) || r.gameObject.name.Contains("body", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public static TComponent FindOrAddComponent<TComponent>(GameObject go) where TComponent : Component
        {
            if (!go)
                return null;
            TComponent comp = go.GetComponent<TComponent>();
            if (comp == null)
            {
#if UNITY_EDITOR
                comp = Undo.AddComponent<TComponent>(go);
#else
                comp = go.AddComponent<TComponent>();
#endif
            }
            return comp;
        }

        public static void EnsureMaterialSwitcher(GameObject[] objects, bool createIfNotPresent = false)
        {
            
            foreach (var go in objects)
            {
                Renderer [] renderList = go.GetComponentsInChildren<Renderer>();
                if (renderList != null)
                {
                    Renderer r = go.GetComponent<Renderer>();
                    if (!r)
                    {
                        var dgmc = go.GetComponent<DidimoGlobalMaterialIndexChooser>();
                        if (dgmc == null)
                        {
                            FindOrAddComponent<DidimoGlobalMaterialIndexChooser>(go);
                        }                    
                    }
                }
                foreach (var r in renderList)
                {
                    if (IsRendererAtlasable(r)) //not all renderables are suitable for atlasing
                    {
                        var dms = r.GetComponent<DidimoMaterialSwitcher>();
                        if (!dms && createIfNotPresent)
                        {
                            dms = FindOrAddComponent<DidimoMaterialSwitcher>(r.gameObject);
                            if (dms != null && dms.MaterialSets.Count == 0)
                                dms.SetEntryToOwnMaterials(0);
                        }
                    }
                }
            }
        }

        public static void EnsureMaterialSwitcherGroup(GameObject[] objects, int index, Dictionary<Material, Material> materialMap)
        {
            foreach (var go in objects)
            {
                var dms = go.GetComponent<DidimoMaterialSwitcher>();
                if (dms)
                {
                    dms.EnsureEntryCount(index, true);
                    var switcherEntry = dms.MaterialSets[index];
                    for (var i = 0; i < switcherEntry.materials.Length; ++i)
                    {
                        if (materialMap.ContainsKey(switcherEntry.materials[i]))
                            switcherEntry.materials[i] = materialMap[switcherEntry.materials[i]];
                    }
                }
            }
        }

        static List<Material> ReplaceMaterialsInner(List<Material> ml, Dictionary<Material, Material> uniqueMaterials, bool useMaterialSwitcherIfAvailable = true,
            int switcherIndex = 1)
        {
            if (ml.Count > 0)
            {
                List<Material> newlist = new List<Material>();
                for (var i = 0; i < ml.Count; ++i)
                {
                    var mat = ml[i];
                    if (mat != null)
                    {
                        if (uniqueMaterials.ContainsKey(mat))
                            newlist.Add(uniqueMaterials[mat]);
                        else
                            newlist.Add(mat);
                    }
                }

                return newlist;
            }

            return null;
        }

        public static void ReplaceMaterials(GameObject[] objects, Dictionary<Material, Material> uniqueMaterials, bool useMaterialSwitcherIfAvailable = true, int switcherIndex = 1)
        {
            foreach (GameObject go in objects)
            {
#if UNITY_EDITOR
                Undo.RecordObject(go, "Replace materials");
#endif
                var smrlist = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                var mrlist = go.GetComponentsInChildren<MeshRenderer>();
                List<Material> ml = new List<Material>();
                foreach (var smr in smrlist)
                {
                    smr.GetSharedMaterials(ml);
                    List<Material> newlist = ReplaceMaterialsInner(ml, uniqueMaterials);
                    if (newlist.Count > 0)
                    {
                        DidimoMaterialSwitcher dms = smr.GetComponent<DidimoMaterialSwitcher>();
                        if (dms && useMaterialSwitcherIfAvailable)
                        {
#if UNITY_EDITOR
                            Undo.RecordObject(dms, "setting material switch entries");
#endif
                            dms.SetEntryToOwnMaterials(switcherIndex);
                            dms.SetEntryMaterials(switcherIndex, newlist.ToArray());
                        }
                        else
                        {
                            var newMat2 = uniqueMaterials[(Material)smr.sharedMaterials.GetValue(0)];
                            smr.sharedMaterial = newMat2;
                            smr.gameObject.GetComponent<Renderer>().sharedMaterial = newMat2;
                        }
                    }
                    else
                        Debug.Log("Empty material list found");
                }

                foreach (var mr in mrlist)
                {
                    mr.GetSharedMaterials(ml);
                    List<Material> newlist = ReplaceMaterialsInner(ml, uniqueMaterials);
                    if (newlist.Count > 0)
                    {
                        DidimoMaterialSwitcher dms = mr.GetComponent<DidimoMaterialSwitcher>();
                        if (dms && useMaterialSwitcherIfAvailable)
                        {
#if UNITY_EDITOR
                            Undo.RecordObject(dms, "setting material switch entries");
#endif
                            dms.SetEntryMaterials(switcherIndex, newlist.ToArray());
                        }
                        else
                        {
                            var newMat2 = uniqueMaterials[(Material)mr.sharedMaterials.GetValue(0)];
                            mr.sharedMaterial = newMat2;
                            mr.gameObject.GetComponent<Renderer>().sharedMaterial = newMat2;
                        }
                    }
                    else
                        Debug.Log("Empty material list found");
                }
            }
        }
    }
}