using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonSubTypes
{
    public class JsonSubtypesWithPropertyConverterBuilder
    {
        private readonly Type baseType;
        private readonly Dictionary<string, Type> subTypeMapping = new Dictionary<string, Type>();
        private Type fallbackSubtype;

        private JsonSubtypesWithPropertyConverterBuilder(Type baseType)
        {
            this.baseType = baseType;
        }

        public static JsonSubtypesWithPropertyConverterBuilder Of(Type baseType)
        {
            return new JsonSubtypesWithPropertyConverterBuilder(baseType);
        }

        public static JsonSubtypesWithPropertyConverterBuilder Of<T>()
        {
            return Of(typeof(T));
        }

        public JsonSubtypesWithPropertyConverterBuilder RegisterSubtypeWithProperty(
            Type subtype, string jsonPropertyName)
        {
            subTypeMapping.Add(jsonPropertyName, subtype);
            return this;
        }

        public JsonSubtypesWithPropertyConverterBuilder RegisterSubtypeWithProperty<T>(string jsonPropertyName)
        {
            return RegisterSubtypeWithProperty(typeof(T), jsonPropertyName);
        }

        public JsonSubtypesWithPropertyConverterBuilder SetFallbackSubtype(Type fallbackSubtype)
        {
            this.fallbackSubtype = fallbackSubtype;
            return this;
        }

        public JsonSubtypesWithPropertyConverterBuilder SetFallbackSubtype<T>()
        {
            return SetFallbackSubtype(typeof(T));
        }

        public JsonConverter Build()
        {
            return new JsonSubtypesByPropertyPresenceConverter(baseType, subTypeMapping, fallbackSubtype);
        }
    }
}
