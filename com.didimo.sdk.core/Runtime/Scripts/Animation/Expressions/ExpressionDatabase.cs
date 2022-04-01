using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "Expression Database",
        menuName = "Didimo/Animation/Expression Database")]
    public class ExpressionDatabase : ScriptableObject
    {
        [SerializeField] public TextAsset[] Expressions;
    }
}