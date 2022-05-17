using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Didimo.Core.Config
{
    [CreateAssetMenu(fileName = "ShaderResources", menuName = "Didimo/Shader Resources")]
    public class ShaderResources : ScriptableObject
    {
        // "Pipeline Shader resources required to load a didimo"
        public enum EBodyPartID
        {
            EYE = 0,
            HEAD,
            BODY,
            MOUTH,
            HAIR,
            EYELASHES,
            CLOTHING,
            HAT,
            UNKNOWN
        }

        public enum EShaderType
        {
            SEPARATE_CHANNELS = 0,
            MERGED_CHANNELS = 1,
            MERGED_CHANNELS_ATLASED = 2
        }

        public enum EHairLayer
        {
            UNKNOWN = -1,
            INNER_OPAQUE = 0,
            INNER_FRINGE = 1,
            OUTER = 2,
            COUNT = 3,
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
            if (name.Contains("opaque"))
                return EHairLayer.INNER_OPAQUE;
            else if (name.Contains("outer"))
                return EHairLayer.OUTER;
            else if (name.Contains("inner"))
                return EHairLayer.INNER_FRINGE;
            else
                return EHairLayer.UNKNOWN;
        }


        [Header("Shader Resources")]
        public Shader Eye;
        public Shader Skin;
        public Shader SkinMergedTextures;
        public Shader SkinMergedAtlasedTextures;
        public Shader Mouth;
        public Shader Eyelash;
        public Shader UnlitTexture;
        public Shader Hair;
        public Shader HairOpaque;
        public Shader Cloth;

        public static string[] hair_name_fragments = { "albedo", "alpha", "ao", "uniqueao", "flow", "id", "ramp" };
        public static string[] hair_material_texture_names = { "_Albedo", "_Opacity", "_AOMap", "_AOMapUnique", "_flowMap", "_ID", "_rootToTip" };
        //this can be used to determine body part IDs for materials and meshes and files - N.B. sensitive to name changes! If asset names are renamed, this needs testing
        public static EBodyPartID GetBodyPartID(string name)
        {
            name = name.ToLower();
            if (name.Contains("eyelash"))
                return EBodyPartID.EYELASHES;
            if (name.Contains("eye"))
                return EBodyPartID.EYE;
            if (name.Contains("head"))
                return EBodyPartID.HEAD;
            if (name.Contains("face"))
                return EBodyPartID.HEAD;
            if (name.Contains("body")) //bodyskin is body on materials, skin is face
                return EBodyPartID.BODY;
            if (name.Contains("skin"))
                return EBodyPartID.HEAD;            
            if (name.Contains("hair"))
                return EBodyPartID.HAIR;
            if (name.Contains("mouth"))
                return EBodyPartID.MOUTH;
            if (name.Contains("clothing") || name.Contains("shirt") || name.Contains("trousers") || name.Contains("dress") || name.Contains("skirt"))
                return EBodyPartID.CLOTHING;
            if (name.Contains("hat") || name.Contains("cap"))
                return EBodyPartID.HAT;

            return EBodyPartID.UNKNOWN;
        }

        public static string[] BodyPartMaterialNames = { "eye", "skin", "bodySkin", "mouth", "hair", "eyelash", "clothing", "clothing" };

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
        public Shader GetShader(EBodyPartID bpid, EShaderType shaderType)
        {
            switch (bpid)
            {
                case EBodyPartID.EYE: return Eye;
                case EBodyPartID.BODY: goto case EBodyPartID.HEAD;
                case EBodyPartID.HEAD:
                    switch (shaderType)
                    {
                        case EShaderType.SEPARATE_CHANNELS: return Skin;
                        case EShaderType.MERGED_CHANNELS: return SkinMergedTextures;
                        case EShaderType.MERGED_CHANNELS_ATLASED: return SkinMergedAtlasedTextures;
                    }
                    return null;
                case EBodyPartID.MOUTH: return Mouth;
                case EBodyPartID.HAIR: return Hair;
                case EBodyPartID.EYELASHES: return Eyelash;
                case EBodyPartID.HAT: return Cloth;
                case EBodyPartID.CLOTHING: return Cloth;
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