using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable 4014
namespace Didimo
{
    public static class DidimoCache
    {
        public const string ALL_ID = "All";
        public const string ANY_ID = "Any";

        private static readonly Dictionary<string, DidimoComponents> didimos = new Dictionary<string, DidimoComponents>();

        public static bool TryDestroy(string id)
        {
            if (id == ALL_ID)
            {
                DestroyAll();
                return true;
            }

            if (!TryFindDidimo(id, out DidimoComponents didimo))
            {
                return false;
            }

            DestroyDidimo(didimo);
            didimos.Remove(id);
            return true;
        }

        public static void DestroyAll()
        {
            foreach (KeyValuePair<string, DidimoComponents> kvp in didimos)
            {
                DestroyDidimo(kvp.Value);
            }

            didimos.Clear();
        }

        public static bool TryFindDidimo(string id, out DidimoComponents didimoComponents)
        {
            if (id != ANY_ID) return didimos.TryGetValue(id, out didimoComponents);

            if (!didimos.Any())
            {
                didimoComponents = null;
                return false;
            }

            didimoComponents = didimos.FirstOrDefault().Value;
            return didimoComponents != null;
        }

        public static void Add(DidimoComponents didimoComponents)
        {
            if (didimoComponents == null) return;
            if (TryFindDidimo(didimoComponents.DidimoKey, out _)) return;

            didimos[didimoComponents.DidimoKey] = didimoComponents;
        }

        private static void DestroyDidimo(DidimoComponents didimoComponents) { Object.Destroy(didimoComponents.gameObject); }
    }
}
#pragma warning restore 4014