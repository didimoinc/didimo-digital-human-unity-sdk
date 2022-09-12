using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class AssetFitterHelper : MonoBehaviour
    {
        public AccessoryType accessoryType;

        public enum AccessoryType
        {
            Ignore = -1,
            Auto = 0,
            Glasses = 1,
            Eyelashes = 2,
            Hats = 3,
        }
    }
}