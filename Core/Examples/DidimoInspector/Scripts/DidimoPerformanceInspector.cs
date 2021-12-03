using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Didimo.Stats;
using TMPro;

namespace Didimo.Core.Examples.DidimoInspector
{
    public class DidimoPerformanceInspector : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI fpsText;

        [SerializeField]
        private TextMeshProUGUI meshBreakdownText;

        [SerializeField]
        private TextMeshProUGUI vertexCountText;

        [SerializeField]
        private GameObject didimo;

        [SerializeField]
        private GameObject faceMesh;

        private bool didimoRotating = false;
        private Vector3 initialDidimoRotation;

        [SerializeField]
        private float rotationSpeed = 20.0f;

        private float rotationSpeedMin = 10;
        private float rotationSpeedMax = 30;

        [SerializeField]
        private Slider rotationSpeedSlider;

        private DidimoGameObjectStats.MeshData meshData;
        private SceneStats.FPSData fpsData;

        // Send an update log message every X seconds
        private float fpsUpdateCumulativeTime;
        private const float FPS_UPDATE_TIME_FREQUENCY = 1f;


        private void Start()
        {
            meshData = DidimoGameObjectStats.GetMeshData(gameObject);
            fpsData = SceneStats.GetFPSData();

            UpdateActiveMeshBreakdown();
            UpdateVertexCount();

            initialDidimoRotation = didimo.transform.rotation.eulerAngles;
            rotationSpeedSlider.normalizedValue = (rotationSpeed - rotationSpeedMin) / (rotationSpeedMax - rotationSpeedMin);
            
            rotationSpeedSlider.onValueChanged.AddListener(UpdateRotationSpeed);
        }

        private void Update()
        {
            fpsData.AddFrame(Time.deltaTime);

            // Write Log message 
            fpsUpdateCumulativeTime += Time.deltaTime;
            if (fpsUpdateCumulativeTime >= FPS_UPDATE_TIME_FREQUENCY)
            {
                fpsUpdateCumulativeTime = 0f;
                fpsText.text = $"Avg:\t\t{fpsData.Average:F1}\nMax:\t\t{fpsData.Max:F0}\nMin:\t\t{fpsData.Min:F0}";
            }

            if (didimoRotating)
            {
                didimo.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
        }

        private void UpdateActiveMeshBreakdown()
        {
            meshData = DidimoGameObjectStats.GetMeshData(didimo);

            StringBuilder sb = new StringBuilder();

            foreach (DidimoGameObjectStats.MeshData.Mesh mesh in meshData.ActiveMeshes)
            {
                string meshText = $"Mesh name: {mesh.Name}\nMesh vertices: {mesh.VertexCount}\nMesh triangles: {mesh.TriangleCount}\n";
                sb.Append(meshText);

                if (mesh != meshData.ActiveMeshes.Last())
                {
                    sb.Append("\n");
                }
            }

            meshBreakdownText.text = sb.ToString();
        }

        private void UpdateVertexCount()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($@"Number of Active Meshes: {meshData.ActiveMeshCount}
Total Active Vertices: {meshData.TotalActiveVertexCount}
Total Active Triangles: {meshData.TotalActiveTriangleCount}");
            vertexCountText.text = sb.ToString();
        }


        public void ToggleShowDidimoInterior()
        {
            faceMesh.SetActive(!faceMesh.activeSelf);
            UpdateVertexCount();
            UpdateActiveMeshBreakdown();
        }

        public void ToggleRotation()
        {
            if (didimoRotating)
            {
                didimoRotating = false;
                didimo.transform.rotation = Quaternion.Euler(initialDidimoRotation);
            }
            else
            {
                didimoRotating = true;
            }
        }

        public void UpdateRotationSpeed(float sliderValue)
        {
            rotationSpeed = Mathf.Lerp(rotationSpeedMin, rotationSpeedMax, sliderValue);
        }
    }
}
