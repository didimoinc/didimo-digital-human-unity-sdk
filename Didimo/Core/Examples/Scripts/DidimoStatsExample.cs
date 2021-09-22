using System.Text;
using Didimo.Inspector;
using UnityEngine;

namespace Didimo.Stats
{
    public class DidimoStatsExample : MonoBehaviour
    {
        
        private DidimoGameObjectStats.MeshData meshData;
        private SceneStats.FPSData             fpsData;
        
        // Send an update log message every X seconds
        private       float fpsUpdateCumulativeTime;
        private const float FPS_UPDATE_TIME_FREQUENCY = 1f;


        
        private void Start()
        {
            meshData = DidimoGameObjectStats.GetMeshData(gameObject);
            fpsData = SceneStats.GetFPSData();
        }

        private void Update()
        {
            fpsData.AddFrame(Time.deltaTime);
            
            // Write Log message 
            fpsUpdateCumulativeTime += Time.deltaTime;
            if (fpsUpdateCumulativeTime >= FPS_UPDATE_TIME_FREQUENCY)
            {
                fpsUpdateCumulativeTime = 0f;
                Debug.Log($"FPS: {fpsData.Average:F1}\nMax: {fpsData.Max:F0}\nMin: {fpsData.Min:F0}");
            }
        }
        
        
        [Button]
        private void GetAllMeshes()
        {
            meshData = DidimoGameObjectStats.GetMeshData(gameObject);
            
            StringBuilder sb = new StringBuilder();
            sb.Append($"GameObject Name: {gameObject.name}\n\n");
            foreach (DidimoGameObjectStats.MeshData.Mesh mesh in meshData.Meshes)
            {
                string meshText = $"Mesh name: {mesh.Name}\nMesh vertices: {mesh.VertexCount}\nMesh triangles: {mesh.TriangleCount}\n";
                sb.Append(meshText);
                sb.Append("=====================================\n");
            }

            sb.Append($"\nNumber of Meshes: {meshData.MeshCount}\nTotal Vertices: {meshData.TotalVertexCount}\nTotal Triangles: {meshData.TotalTriangleCount}\n");
            sb.Append("=====================================\n");
            Debug.Log(sb.ToString());
        }
        
        [Button]
        private void GetAllActiveMeshes()
        {
            meshData = DidimoGameObjectStats.GetMeshData(gameObject);
            
            StringBuilder sb = new StringBuilder();
            sb.Append($"GameObject Name: {gameObject.name}\n\n");
            foreach (DidimoGameObjectStats.MeshData.Mesh mesh in meshData.ActiveMeshes)
            {
                string meshText = $"Mesh name: {mesh.Name}\nMesh vertices: {mesh.VertexCount}\nMesh triangles: {mesh.TriangleCount}\n";
                sb.Append(meshText);
                sb.Append("=====================================\n");
            }

            sb.Append($"\nNumber of Active Meshes: {meshData.ActiveMeshCount}\nTotal Active Vertices: {meshData.TotalActiveVertexCount}\nTotal Active Triangles: {meshData.TotalActiveTriangleCount}\n");
            sb.Append("=====================================\n");
            Debug.Log(sb.ToString());
        }
    }
}
