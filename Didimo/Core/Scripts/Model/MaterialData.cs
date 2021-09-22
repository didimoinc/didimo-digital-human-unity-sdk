using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo
{
    public class MaterialData
    {
        [JsonProperty("name")] public string Name { get; private set; }

        [JsonProperty("shader")] public string ShaderName { get; private set; }

        [JsonProperty("parameters")] public List<MaterialDataParameter> parameters { get; private set; }

        [JsonIgnore] public IReadOnlyList<MaterialDataParameter> Parameters => parameters;

        public void MergeParameters(IReadOnlyList<MaterialDataParameter> parametersToMerge) { }

        public static MaterialData FromMaterial(Material material) => new MaterialData {Name = material.name, ShaderName = material.shader.name, parameters = GetParameters(material)};

        private static List<MaterialDataParameter> GetParameters(Material material)
        {
            List<MaterialDataParameter> parameters = new List<MaterialDataParameter>();
            for (int propertyIndex = 0; propertyIndex < material.shader.GetPropertyCount(); propertyIndex++)
            {
                ShaderPropertyType propertyType = material.shader.GetPropertyType(propertyIndex);
                string propertyName = material.shader.GetPropertyName(propertyIndex);

                if (propertyName.StartsWith("unity_")) continue;

                switch (propertyType)
                {
                    case ShaderPropertyType.Color:
                        Color cValue = material.GetColor(propertyName);
                        Vector4MaterialDataParameter cParameter = Vector4MaterialDataParameter.CreateNew(propertyName, cValue);
                        parameters.Add(cParameter);
                        break;
                    case ShaderPropertyType.Vector:
                        Vector4 vValue = material.GetVector(propertyName);
                        Vector4MaterialDataParameter vParameter = Vector4MaterialDataParameter.CreateNew(propertyName, vValue);
                        parameters.Add(vParameter);
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        float fValue = material.GetFloat(propertyName);
                        FloatMaterialDataParameter fParameter = FloatMaterialDataParameter.CreateNew(propertyName, fValue);
                        parameters.Add(fParameter);
                        break;
                    case ShaderPropertyType.Texture:
                        Texture tValue = material.GetTexture(propertyName);
                        TextureMaterialDataParameter tParameter = TextureMaterialDataParameter.CreateNew(propertyName, tValue);
                        parameters.Add(tParameter);
                        break;
                }
            }

            return parameters;
        }
    }
}