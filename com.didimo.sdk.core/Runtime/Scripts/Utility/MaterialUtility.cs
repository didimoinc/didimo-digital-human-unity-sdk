using Didimo.Builder;
using Didimo.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static void SetBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
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
            [FieldOffset(0)] private float f;
            [FieldOffset(0)] private int i;
            public static int ConvertToInt(float value)
            {
                return new FloatToInt { f = value }.i;
            }
            public static float ConvertToFloat(int value)
            {
                return new FloatToInt { i = value }.f;
            }
        }

        private static int floatToIntBits(float value)
        {
            return FloatToInt.ConvertToInt(value);
        }

        private static float intToFloatBits(int value)
        {
            return FloatToInt.ConvertToFloat(value);
        }

        private static Vector4 intToFloatBits(Vector4Int value)
        {
            return new Vector4(intToFloatBits(value.x), intToFloatBits(value.y), intToFloatBits(value.z), intToFloatBits(value.w));
        }        
        public static void FixupDefaultShaderParams(Material mat, EBodyPartID bpid)
        {            
            Vector4 EyeProfileAsset = intToFloatBits(new Vector4Int(263726704, -1537711319, -872588405, -926254750));
            float EyeDiffuseHash = intToFloatBits(1080904598);// 2.066104f;
            float SkinDiffuseHash = intToFloatBits(1074019085);// 2.066104f;
            Vector4 SkinProfileAsset = intToFloatBits(new Vector4Int(1306322346, -2065702086, 1728913528, -652625823));// new Vector4(463254800.00f, 0.00f, 666452600000000000000000.00f, -2704275000000000.00f);
            
            for (var i = 0; i < mat.shader.GetPropertyCount(); ++i)
            {
                var propType = mat.shader.GetPropertyType(i);
                var propName = mat.shader.GetPropertyName(i);
    
                if (propName == "_DiffusionProfileHash")
                {
                    float hash = mat.GetFloat("_DiffusionProfileHash");
                    mat.SetFloat(propName, (bpid == EBodyPartID.EYE) ? EyeDiffuseHash : SkinDiffuseHash);
                    Debug.Log("Diffuse profile hash: " + hash.ToString());
                }
                if (propName == "_DiffusionProfileAsset")
                {
                    mat.SetVector(propName, (bpid == EBodyPartID.EYE) ? EyeProfileAsset : SkinProfileAsset);
                }
                //Ensure that body texture has correct combined, shared effects texture
                if (propName == "_Trans_Bias_SSSAO")
                {
                    if (bpid == EBodyPartID.BODY)
                    {
                        Texture tex = mat.GetTexture(propName);
                        
                        if (!tex || !tex.name.ToLower().Contains("BodySkin"))
                        {
#if UNITY_EDITOR
                            var assetPath = "Packages/com.didimo.sdk.core/Runtime/Content/Textures/BodySkin_Combined_Trans_Bias_SSSAO.png"; //first try to load canoncial, shared version
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

                if (bpid == EBodyPartID.EYELASHES)
                {
                    if (propName.ToLower().Contains("surface"))
                    {
                        mat.SetFloat(propName, 1.0f);
                    }
                }

                if (propName == "_AlphaClipThreshold")
                    mat.SetFloat(propName, (bpid == EBodyPartID.HEAD) ? 0.0f : 0.5f);
                
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
            if (bpid == EBodyPartID.BODY)
            {
                int propID = mat.shader.FindPropertyIndex("_UseAlphaClip");
                if (propID != -1)
                    mat.SetFloat(propID, 1.0f);
            }
            mat.shader = mat.shader;
#if UNITY_EDITOR
            EditorUtility.SetDirty(mat);
#endif            
        }

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
                                    LogString.AppendFormat("{0} : {1}\n", indentName, (v != null ? v.ToString() + " : " + UnityEditor.AssetDatabase.GetAssetPath(v) : "Empty"));
#else
                                    LogString.AppendFormat("{0} : {1}\n", indentName, v.ToString());
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
        
        static public Material[] MergedAtlasedInner(Component comp, Material[] ml, DidimoInstancingHelper dih, int cidx, Material mergedAtlasedMaterial, Material[] altasMaterialSlots, int MaterialSwitchSlot)
        {
            GameObject go = comp.gameObject;
            DidimoMaterialSwitcher dms = comp.GetComponent<DidimoMaterialSwitcher>();
            if (dih == null)//still no dih? Try the SMR
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
                        newMatList[i] = m;//use original entry
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
                Undo.RecordObject(dms, "settimg material switch entries");
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


        public static void EnsureMaterialSwitcher(GameObject[] objects, bool createIfNotPresent = false)
        {
            foreach (var go in objects)
            {
                var dms = go.GetComponent<DidimoMaterialSwitcher>();
                if (!dms && createIfNotPresent)
                {
#if UNITY_EDITOR
                    dms = Undo.AddComponent<DidimoMaterialSwitcher>(go);
#else
                    dms = go.AddComponent<DidimoMaterialSwitcher>();
#endif

                }
                if (dms.MaterialSets.Count == 0)
                    dms.SetEntryToOwnMaterials(0);
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

        static List<Material> ReplaceMaterialsInner(List<Material> ml, Dictionary<Material, Material> uniqueMaterials, bool useMaterialSwitcherIfAvailable = true, int switcherIndex = 1)
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
                            Undo.RecordObject(dms, "settimg material switch entries");
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
                            Undo.RecordObject(dms, "settimg material switch entries");
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