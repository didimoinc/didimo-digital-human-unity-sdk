using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DidimoAlphaClipController : MonoBehaviour
{
    public bool FaceVisible = true;
    public bool BodyUnderClothesVisible = false;

    MaterialPropertyBlock bodyPropBlock = null;
    MaterialPropertyBlock headPropBlock = null;
    Renderer bodyRenderer = null;
    Renderer headRenderer = null;

    static string alphaThresholdName = "_AlphaClipThreshold";
    int alphaThresholdID = -1;
    public void OnValidate() { ApplyBlocks(); }

    void ProcessIDs()
    {
        if (alphaThresholdID == -1)
            alphaThresholdID = Shader.PropertyToID(alphaThresholdName);
        // BuildFromComponents();
    }
    // public void BuildFromComponents()
    // {
    //     if (bodyRenderer == null)
    //         bodyRenderer = ComponentUtility.SafeGetComponent<SkinnedMeshRenderer>(ComponentUtility.GetChildWithName(gameObject, MeshUtils.DidimoMeshPartNames.Body, true));
    //     if (headRenderer == null)
    //         headRenderer = ComponentUtility.SafeGetComponent<SkinnedMeshRenderer>(ComponentUtility.GetChildWithName(gameObject, MeshUtils.DidimoMeshPartNames.HeadMesh, true));        
    // }

    [Button]
    public void ResetInternals()
    {
        bodyPropBlock = null;
        headPropBlock = null;
        bodyRenderer = null;
        headRenderer = null;
        alphaThresholdID = -1;
    }

    void ApplyBlocks()
    {
        bodyPropBlock ??= new MaterialPropertyBlock();
        headPropBlock??= new MaterialPropertyBlock();
        ProcessIDs();
        if (bodyRenderer != null)
        {
            bodyRenderer.GetPropertyBlock(bodyPropBlock, 0);
            bodyPropBlock.SetFloat(alphaThresholdID, BodyUnderClothesVisible ? 0.0f : 0.5f);
            bodyRenderer.SetPropertyBlock(bodyPropBlock, 0);
        }
        if (headRenderer != null)
        {
            headRenderer.GetPropertyBlock(headPropBlock, 0);
            headPropBlock.SetFloat(alphaThresholdID, FaceVisible ? 0.0f : 0.5f);
            headRenderer.SetPropertyBlock(headPropBlock, 0);
        }
    }
    private void LateUpdate() { ApplyBlocks(); }
}