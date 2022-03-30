using UnityEngine;

namespace Didimo.Core.Config
{
    //[CreateAssetMenu(fileName = "IrisDatabase", menuName = "Didimo/Iris Database")]
    public class IrisDatabase : ScriptableObject
    {
        [SerializeField] public Texture2D[] Irises;
    }
}