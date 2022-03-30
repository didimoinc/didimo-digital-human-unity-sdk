using System.Collections.Generic;
using System.Linq;
using System.Text;
using Didimo.Core.Utility;
using UnityEngine;
using UnityEngine.UI;
using Didimo.Stats;
using TMPro;
/// <summary>
/// Class that controls  what is displayed on the information panel in the Didimo Inspector scene as well as 
/// all the functions the control panel has, like triggering the body rotation.
/// </summary>
public class DidimoInspector : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro fpsText;

    [SerializeField]
    private TextMeshPro meshBreakdownText;

    [SerializeField]
    private TextMeshPro vertexCountText;

    [SerializeField]
    private GameObject didimo;

    [SerializeField]
    private List<GameObject> externalMeshes;

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

    private void OnValidate()
    {
        meshData = DidimoGameObjectStats.GetMeshData(gameObject);
        UpdateVertexCount();
        UpdateActiveMeshBreakdown();
    }

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
            string meshText = $"Name: {mesh.Name}\nVerts: {mesh.VertexCount} Tris: {mesh.TriangleCount}\n";
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

        sb.Append($"Number of Active Meshes: {meshData.ActiveMeshCount}\nTotal Active Vertices: {meshData.TotalActiveVertexCount}\nTotal Active Triangles: {meshData.TotalActiveTriangleCount}");

        vertexCountText.text = sb.ToString();
    }


    public void ToggleShowDidimoInterior()
    {
        if (externalMeshes == null || externalMeshes.Count == 0) return;
        if (externalMeshes[0].activeSelf)
        {
            externalMeshes.ForEach(m => m.SetActive(false));
            UpdateVertexCount();
            UpdateActiveMeshBreakdown();
        }
        else
        {
            externalMeshes.ForEach(m => m.SetActive(true));
            UpdateVertexCount();
            UpdateActiveMeshBreakdown();
        }
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
