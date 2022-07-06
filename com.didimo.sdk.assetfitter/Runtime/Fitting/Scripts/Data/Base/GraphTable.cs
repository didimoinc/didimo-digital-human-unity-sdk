using System.Collections.Generic;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class GraphTable
    {
        public class Node
        {
            public GraphNode node;
            public Dictionary<string, Node> outputs;
            public Dictionary<string, Node> inputs;
        }
    }
}
