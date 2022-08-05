using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "New Mesh List", menuName = "Didimo/Graph/Lists/Mesh List")]
    public class MeshList : ScriptableObject
    {
        public Mesh[] meshes;
    }
}
