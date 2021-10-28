using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "IrisDatabase", menuName = "Didimo/Iris Database")]
    public class IrisDatabase : ScriptableObject
    {
        [SerializeField]
        private Texture2D[] irises;

        public static Texture2D[] Irises => DidimoResources.IrisDatabase.irises;
        
    }
}