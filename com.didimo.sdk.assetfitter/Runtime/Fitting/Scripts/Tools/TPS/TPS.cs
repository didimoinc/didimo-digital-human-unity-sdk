using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class TPS : MonoBehaviour
    {
        public Mesh sourceMesh;  // template mesh
        public Mesh targetMesh;  // didimo mesh
        public MeshFilter meshToDeform;  // hair mesh

        [Button]
        private void Deform()
        {
            float startTime = Time.realtimeSinceStartup;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "You must enter play mode to use this feature.", "OK");
                return;
            }
#endif
            List<Vector3> source = sourceMesh.vertices.ToList();
            List<Vector3> target = targetMesh.vertices.ToList();

            List<Vector3> verticesToDeform = new List<Vector3>();
            meshToDeform.mesh.GetVertices(verticesToDeform);

            float getDeformMatrixStartTime = Time.realtimeSinceStartup;
            CGTPS cgtps = new CGTPS(source, target);

            Debug.Log($"Got deformation matrix in {Time.realtimeSinceStartup - getDeformMatrixStartTime} seconds.");

            float deformVerticesStartTime = Time.realtimeSinceStartup;
            List<Vector3> deformedVertices = cgtps.TransformVertices(verticesToDeform);
            Debug.Log($"Applied matrix to vertices in {Time.realtimeSinceStartup - deformVerticesStartTime} seconds.");
            meshToDeform.mesh.SetVertices(deformedVertices);

            float duration = Time.realtimeSinceStartup - startTime;
            Debug.Log($"Total time: {duration} seconds.");
        }
    }
}