using System;
using System.Collections.Generic;

namespace JsonSubTypes
{
    internal class JsonSubtypesByPropertyPresenceConverter : JsonSubtypesConverter
    {
        private readonly Dictionary<string, Type> jsonPropertyName2Type;

        internal JsonSubtypesByPropertyPresenceConverter(
            Type baseType, Dictionary<string, Type> jsonProperty2Type, Type fallbackType)
            : base(baseType, fallbackType)
        {
            jsonPropertyName2Type = jsonProperty2Type;
        }

        internal override Dictionary<string, Type> GetTypesByPropertyPresence(Type parentType)
        {
            return jsonPropertyName2Type;
        }
    }
}
