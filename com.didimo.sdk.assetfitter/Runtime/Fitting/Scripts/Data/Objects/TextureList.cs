using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "New Texture List", menuName = "Didimo/Graph/Lists/Texture List")]
    public class TextureList : ScriptableObject
    {
        public Texture2D[] textures;

#if UNITY_EDITOR
        void Reset()
        {
            textures = UnityEditor.Selection.objects.Select(o => o as Texture2D).Where(o => o).ToArray();
        }
#endif
    }
}
