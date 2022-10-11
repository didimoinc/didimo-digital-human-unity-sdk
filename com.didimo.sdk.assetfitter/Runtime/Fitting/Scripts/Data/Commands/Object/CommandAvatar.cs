using System.Collections.Generic;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [Width(240)]
    public abstract class CommandAvatar : GraphNode, IExposable
    {
        public abstract Gender gender { get; }
        public abstract GameObject GetPrefab();
        public abstract Mesh CreateManifold();
    }
}
