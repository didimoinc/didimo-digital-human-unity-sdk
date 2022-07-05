using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    public class TPSWeights : ScriptableObject
    {
        [HideInInspector, SerializeField] byte[] data;
        [SerializeField] int dataSize;

        public int GetSize() => data == null ? 0 : data.Length;

        public static TPSWeights CreateInstance(Vector3[] vertices1, Vector3[] vertices2)
        {
            var weights = ScriptableObject.CreateInstance<TPSWeights>();

            var tps = new CGTPS(vertices1.ToList(), vertices2.ToList());
            weights.data = tps.Serialize();
            weights.dataSize = weights.data.Length;

            return weights;
        }

        public Vector3[] Transform(Vector3[] vertices)
        {
            vertices = new CGTPS(data).TransformVertices(vertices.ToList()).ToArray();

            return vertices;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(TPSWeights))]
    public class TPSWeights_Drawer : UnityEditor.Editor
    {
        new TPSWeights target => base.target as TPSWeights;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.IntField("Data Size", target.GetSize());
        }
    }
#endif
}
