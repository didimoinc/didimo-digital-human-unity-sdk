using System;
using System.Collections.Generic;
using System.Reflection;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Core.Config
{
    [CreateAssetMenu(fileName = "ShaderResources", menuName = "Didimo/Shader Resources")]
    public class ShaderResources : ScriptableObject
    {

        public enum EShaderType
        {
            SEPARATE_CHANNELS = 0,  //Shader uses entirely separate channels with no merging - specular, roughness, metalicity etc. all separate
            MERGED_CHANNELS = 1, //channels are merged for efficient storage and run-time sampling
            ATLASED = 2, //atlasing refers to the combination on one texture page of several different object's textures, which are then indexed in the shader. This can be more efficient
            CHANNEL_SETTINGS_MASK = 3, //mask for merged+atlased
            MERGED_CHANNELS_ATLASED = 3, //convenience combination of merged and atlased
            MSAA_COVERAGE_TO_ALPHA_BLENDED = 4, //this shader implements 'coverage to alpha', a method for combining alpha blending and depth sorting by 'abusing' MSAA functionality
            ALPHA_BLENDED = 8, //Shader uses alpha blending, which provides best quality blending but is problematic when used with z-buffering
            ALPHA_SETTINGS_MASK = 12,
        }

        public enum EHairLayer
        {
            UNKNOWN = -1,
            INNER_OPAQUE = 0, //use opaque hair shader
            INNER_FRINGE = 1, //use alpha blended hair shader but with inner settings
            OUTER = 2, //use alpha blended hair shader but with outer settings
            HAT = 3, //use cloth shader - not hair but some models have hair and hat
            COUNT = 4,
        }

        //n.b. these indices are linked - change the indices, change the arrays below
        public enum EHairTextureType
        {
            ALBEDO = 0,
            ALPHA = 1,
            AO = 2,
            UNQIUE_AO = 3,
            FLOW = 4,
            ID = 5,
            RAMP = 6,
        }
     
        public static EHairLayer ClassifyHairLayerFromName(string name)
        {
            if (name.Contains("hat", StringComparison.CurrentCultureIgnoreCase))
                return EHairLayer.HAT;
            if (name.Contains("opaque", StringComparison.CurrentCultureIgnoreCase))
                return EHairLayer.INNER_OPAQUE;
            if (name.Contains("solid", StringComparison.CurrentCultureIgnoreCase)) //sometimes they call it solid, so solid shall be checked for
                return EHairLayer.INNER_OPAQUE;
            if (name.Contains("outer", StringComparison.CurrentCultureIgnoreCase))
                return EHairLayer.OUTER;
            if (name.Contains("fringe", StringComparison.CurrentCultureIgnoreCase))
                return EHairLayer.INNER_FRINGE;
            if (name.Contains("inner", StringComparison.CurrentCultureIgnoreCase))
                return EHairLayer.INNER_OPAQUE;
            
            return EHairLayer.UNKNOWN;
        }


        [Header("Shader Resources")]
        public Shader Eye;
        public Shader Skin;
        public Shader SkinMergedTextures;
        public Shader SkinMergedAtlasedTextures;
        public Shader Mouth;
        public Shader Eyelash;
        public Shader PBRTransparent;
        public Shader UnlitTexture;
        public Shader BasicPBRLitShader;
        public Shader Hair;
        public Shader HairOpaque;
        public Shader HairMSAA;
        public Shader Cloth;    
        //these should support regex expressions to aid in the matching of names that can vary a great deal
        public static string[] hair_name_fragments = { "albedo", "alpha", "ao", "uniqueao", "flow", "id", "ramp" };
        public static string[] hair_material_texture_names = { "_Albedo", "_Opacity", "_AOMap", "_AOMapUnique", "_flowMap", "_ID", "_rootToTip" };
        
        public static string[] cloth_name_fragments = { "albedo", "normal", "metal"};
        public static string[] cloth_material_texture_names = { "_BaseMap", "_NormalMap", "_Metal_Rough_AO_SS_Map"};
        //this can be used to determine body part IDs for materials and meshes and files - N.B. sensitive to name changes! If asset names are renamed, this needs testing


        public static Dictionary<string, string[]> MaterialPropertyAliasMap =
          new Dictionary<string, string[]>()
          {
               {"_MainAlbedo" ,new []{"*albedo"}},
               {"BaseColorMap",new []{"*albedo"}},
               {"MaskMap",new []{""}},
               { "NormalMap" ,new []{"", "*normal"}},
               { "_MainNormal" ,new []{"", "*normal"}},
               { "_MetalRough",new []{"", ""}},
               {"_Metal_SS_AO",new []{"", "*SSAO" } },
               { "NormalMapOS",new []{""}},
               {"_NormalScale",new []{""}},
               {"_BentNormalMap",new []{""}},
               {"_BentNormalMapOS",new []{""}},
               { "_AO",new []{"AOMap" } }
          };
        public Shader GetShader(DidimoParts.BodyPart bpid, EShaderType shaderType)
        {
            switch (bpid)
            {
                case DidimoParts.BodyPart.LeftEyeMesh:
                case DidimoParts.BodyPart.RightEyeMesh:
                    return Eye;
                case DidimoParts.BodyPart.BodyMesh: 
                case DidimoParts.BodyPart.HeadMesh:
                    switch ((EShaderType)((int)(shaderType) & (int)EShaderType.CHANNEL_SETTINGS_MASK))
                    {
                        case EShaderType.SEPARATE_CHANNELS: return Skin;
                        case EShaderType.MERGED_CHANNELS: return SkinMergedTextures;
                        case EShaderType.MERGED_CHANNELS_ATLASED: return SkinMergedAtlasedTextures;
                    }
                    return null;
                case DidimoParts.BodyPart.MouthMesh:
                    return Mouth;
                case DidimoParts.BodyPart.HairMesh:
                    switch ((EShaderType)((int)(shaderType) & (int)EShaderType.ALPHA_SETTINGS_MASK))
                    {
                        case EShaderType.ALPHA_BLENDED: return Hair;
                        case EShaderType.MSAA_COVERAGE_TO_ALPHA_BLENDED: return HairMSAA;
                        default:return HairOpaque;
                    }
                case DidimoParts.BodyPart.EyeLashesMesh: return Eyelash;
                // case EBodyPartID.HAT: return Cloth;
                case DidimoParts.BodyPart.ClothingMesh: return Cloth;
            }
            return null;
        }
        public int GetIndexOfShader(Shader shader)
        {
            Type t = this.GetType();
            int idx = 0;
            foreach (var f in t.GetFields())
            {
                if (f.FieldType == typeof(Shader))
                {
                    if ((Shader)f.GetValue(this) == shader)
                        return idx;
                }
                idx++;
            }
            return -1;
        }

        public Shader GetShaderByIndex(int index)
        {
            Type t = this.GetType();
            var fields = t.GetFields();
            var field = fields[index];
            try
            {
                return (Shader)field.GetValue(this);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return null;
            }            
        }
    }
}