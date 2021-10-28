using System;
using Didimo.Builder;
using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(TextureMaterialDataParameter), "2dTexture")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(FloatMaterialDataParameter), "float")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(IntMaterialDataParameter), "long")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(Vector2MaterialDataParameter), "float2")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(Vector3MaterialDataParameter), "float3")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(Vector4MaterialDataParameter), "float4")]
    public class MaterialDataParameter
    {
        [JsonProperty("type")] public string Type { get; protected set; }

        [JsonProperty("name")] public string Name { get; protected set; }

        [JsonProperty("property")] public string Property { get; set; }

        public virtual void ApplyToMaterial(DidimoBuildContext context, Material material) { }
    }

    [Serializable]
    public class MaterialDataParameter<TValue> : MaterialDataParameter
    {
        [JsonProperty("value")] public TValue Value { get; protected set; }
    }

    public class TextureMaterialDataParameter : MaterialDataParameter<string>
    {
        [JsonIgnore] public Texture Texture { get; set; }

        [JsonIgnore] public bool IsLinear => Name != "colorSampler";

        public override void ApplyToMaterial(DidimoBuildContext context, Material material)
        {
            if (Texture == null)
            {
                Debug.LogWarning($"Texture for property {Property} is null.");
                return;
            }

            if (string.IsNullOrEmpty(Property) || !material.HasProperty(Property)) return;
            material.SetTexture(Property, Texture);
        }

        public static TextureMaterialDataParameter CreateNew(string name, Texture value) => CreateNew(name, value != null ? value.name : string.Empty);

        public static TextureMaterialDataParameter CreateNew(string name, string textureName) =>
            new TextureMaterialDataParameter {Name = name, Property = name, Type = "2dTexture", Value = textureName};
    }

    public class FloatMaterialDataParameter : MaterialDataParameter<float>
    {
        public override void ApplyToMaterial(DidimoBuildContext context, Material material)
        {
            if (string.IsNullOrEmpty(Property) || !material.HasProperty(Property)) return;
            material.SetFloat(Property, Value);
        }

        public static FloatMaterialDataParameter CreateNew(string name, float value) =>
            new FloatMaterialDataParameter {Name = name, Property = name, Type = "float", Value = value};
    }

    public class IntMaterialDataParameter : MaterialDataParameter<int>
    {
        public override void ApplyToMaterial(DidimoBuildContext context, Material material)
        {
            if (string.IsNullOrEmpty(Property) || !material.HasProperty(Property)) return;
            material.SetInt(Property, Value);
        }

        public static IntMaterialDataParameter CreateNew(string name, int value) => new IntMaterialDataParameter {Name = name, Property = name, Type = "long", Value = value};
    }

    public class Vector2MaterialDataParameter : MaterialDataParameter<float[]>
    {
        [JsonIgnore] public Vector2 Vector => new Vector2(Value[0], Value[1]);

        public override void ApplyToMaterial(DidimoBuildContext context, Material material)
        {
            if (string.IsNullOrEmpty(Property) || !material.HasProperty(Property)) return;
            material.SetVector(Property, ToVector4(Vector));
        }

        public static Vector2MaterialDataParameter CreateNew(string name, Vector2 vector)
        {
            return new Vector2MaterialDataParameter {Name = name, Property = name, Type = "float2", Value = new float[] {vector.x, vector.y}};
        }

        private static Vector4 ToVector4(Vector2 v) => new Vector4(v.x, v.y, 1, 1);
    }

    public class Vector3MaterialDataParameter : MaterialDataParameter<float[]>
    {
        [JsonIgnore] public Vector3 Vector => new Vector3(Value[0], Value[1], Value[2]);

        public override void ApplyToMaterial(DidimoBuildContext context, Material material)
        {
            if (string.IsNullOrEmpty(Property) || !material.HasProperty(Property)) return;
            material.SetVector(Property, ToVector4(Vector));
        }

        public static Vector3MaterialDataParameter CreateNew(string name, Vector3 vector)
        {
            return new Vector3MaterialDataParameter {Name = name, Property = name, Type = "float3", Value = new float[] {vector.x, vector.y, vector.z}};
        }

        private static Vector4 ToVector4(Vector3 v) => new Vector4(v.x, v.y, v.z, 1);
    }

    public class Vector4MaterialDataParameter : MaterialDataParameter<float[]>
    {
        [JsonIgnore] public Vector4 Vector => new Vector4(Value[0], Value[1], Value[2], Value[3]);

        public override void ApplyToMaterial(DidimoBuildContext context, Material material)
        {
            if (string.IsNullOrEmpty(Property) || !material.HasProperty(Property)) return;
            material.SetVector(Property, Vector);
        }

        public static Vector4MaterialDataParameter CreateNew(string name, Vector4 vector)
        {
            return new Vector4MaterialDataParameter {Name = name, Property = name, Type = "float4", Value = new float[] {vector.x, vector.y, vector.z, vector.w}};
        }
    }
}