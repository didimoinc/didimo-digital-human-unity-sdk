using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    public class DidimoModelDataObject
    {
        public const float DEFAULT_UNITS_PER_METER = 100;

        public class Mesh
        {
            [JsonProperty("name")] public string Name { get; private set; }

            [JsonProperty("vertices")] public float[] Vertices { get; private set; }
            [JsonProperty("normals")] public float[] Normals { get; private set; }
            [JsonProperty("uvs")] public float[][] UVs { get; private set; }
            [JsonProperty("faces")] public int[] Faces { get; private set; }

            [JsonProperty("skin_indices")] public List<List<int>> skin_indices { get; private set; }
            [JsonProperty("skin_weights")] public List<List<float>> skin_weights { get; private set; }

            public bool HasSkinning => skin_indices != null && skin_indices.Count > 0;
            public bool HasValidWeights => HasSkinning && skin_weights != null && skin_indices.Count == skin_weights.Count;
        }

        public class Node
        {
            [JsonProperty("scl")]
            private float[] scale;

            [JsonProperty("rotq")]
            private float[] rotation;

            [JsonProperty("pos")]
            private float[] position;

            [JsonProperty("children")]
            private Node[] children;

            [JsonProperty("name")] public string Name { get; private set; }

            public IReadOnlyCollection<Node> Children => children;
            public Vector3 Position => new Vector3(-position[0], position[1], position[2]);
            public Quaternion Rotation => new Quaternion(rotation[0], -rotation[1], -rotation[2], rotation[3]);
            public Vector3 Scale => new Vector3(scale[0], scale[1], scale[2]);
        }

        public class Constraint
        {
            [JsonProperty("constrainedObj")] public string ConstrainedObj { get; private set; }
            [JsonProperty("constraintSrc")] public string ConstraintSrc { get; private set; }
            [JsonProperty("type")] public string Type { get; private set; }
        }

        public float unitsPerMeter = DEFAULT_UNITS_PER_METER;

        [JsonProperty("meshes")]
        private List<Mesh> meshes;

        // Names of the nodes that are skinned. The Mesh's skinning info (namely skin indices), will point into the indices of this array
        [JsonProperty("bones")]
        private List<string> bones;

        [JsonProperty("constraints")]
        private List<Constraint> constraints;

        [JsonProperty("root_node")] public Node RootNode { get; private set; }

        [JsonProperty("material_state")] public MaterialDataContainer MaterialDataContainer { get; private set; }

        public IReadOnlyList<Constraint> Constraints => constraints;
        public IReadOnlyList<Mesh> Meshes => meshes;
        public IReadOnlyList<string> Bones => bones;
    }
}