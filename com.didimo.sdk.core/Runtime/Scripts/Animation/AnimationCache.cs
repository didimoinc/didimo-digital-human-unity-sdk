using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Didimo
{
    public static class AnimationCache
    {
        private static readonly List<DidimoAnimation> allAnimations;
        private static readonly Dictionary<string, DidimoAnimation> idToAnimation;

        public static IReadOnlyList<DidimoAnimation> AllAnimations => allAnimations;
        public static IReadOnlyDictionary<string, DidimoAnimation> IdToAnimation => idToAnimation;

        static AnimationCache()
        {
            allAnimations = new List<DidimoAnimation>();
            idToAnimation = new Dictionary<string, DidimoAnimation>();

            RegisterDefaults();
        }

        public static void Clear()
        {
            allAnimations.Clear();
            idToAnimation.Clear();

            RegisterDefaults();
        }

        public static bool Add(string id, string filePath)
        {
            if (!File.Exists(filePath))
            {
                string msg = $"Failed to load animation ({id}) from non-existing path: {filePath}";
                Debug.Log(msg);
                return false;
            }

            string json = File.ReadAllText(filePath);
            return AddFromJson(id, json);
        }

        public static bool AddFromJson(string id, string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                string msg = $"Failed to load animation ({id}) null json";
                Debug.Log(msg);
                return false;
            }

            DidimoAnimation animation = DidimoAnimation.FromJSONContent(id, json);
            if (animation == null) return false;
            return Add(id, animation);
        }

        public static bool Add(string id, byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return AddFromJson(id, json);
        }

        public static bool Add(string id, DidimoAnimation animation)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"Cannot add null animation id {id}");
                return false;
            }

            if (TryGet(id, out _))
            {
                string msg = $"Skipping animation load ({id}) as an animation already exists with this id.";
                Debug.Log(msg);
                return false;
            }

            allAnimations.Add(animation);
            idToAnimation.Add(id, animation);
            return true;
        }

        public static bool HasAnimation(string id) => idToAnimation.ContainsKey(id);

        public static bool TryGet(string id, out DidimoAnimation animation)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("id is null, returning");
                animation = null;
                return false;
            }

            return idToAnimation.TryGetValue(id, out animation);
        }

        public static bool TryGetInstance(string id, out DidimoAnimation sourceAnimation)
        {
            if (TryGet(id, out sourceAnimation))
            {
                return true;
            }

            sourceAnimation = null;
            return false;
        }

        public static void RegisterDefaults()
        {
            var expressionDatabase = Resources
                .Load<ExpressionDatabase>("ExpressionDatabase");

            foreach (TextAsset expression in expressionDatabase.Expressions)
            {
                RegisterAnimation(expression);
            }
        }

        private static void RegisterAnimation(TextAsset expression)
        {
            if (expression == null)
            {
                Debug.LogWarning("Cannot deserialize null expression, skipping");
                return;
            }

            DidimoAnimation didimoAnimation = DidimoAnimation.FromJSONContent(expression.name, expression.text);

            if (didimoAnimation == null)
            {
                Debug.LogWarning("Failed to deserialize animation json in expression database");
                return;
            }

            if (TryGet(didimoAnimation.AnimationName, out _))
            {
                Debug.LogWarning($"Animation with id {didimoAnimation.AnimationName} already cached, skipping.");
                return;
            }

            didimoAnimation.WrapMode = WrapMode.ClampForever;
            Add(didimoAnimation.AnimationName, didimoAnimation);
        }
    }
}