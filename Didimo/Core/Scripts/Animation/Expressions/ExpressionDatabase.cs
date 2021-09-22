using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "Expression Database", menuName = "Didimo/Animation/Expression Database")]
    public class ExpressionDatabase : ScriptableObject
    {
        [SerializeField]
        private TextAsset[] expressions;

        public static void RegisterDefaults()
        {
            foreach (TextAsset expression in DidimoResources.ExpressionDatabase.expressions)
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

            if (AnimationCache.TryGet(didimoAnimation.AnimationName, out _))
            {
                Debug.LogWarning($"Animation with id {didimoAnimation.AnimationName} already cached, skipping.");
                return;
            }

            didimoAnimation.WrapMode = WrapMode.ClampForever;
            AnimationCache.Add(didimoAnimation.AnimationName, didimoAnimation);
        }
    }
}