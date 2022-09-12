using System;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo
{
    [ExecuteInEditMode]
    public class DidimoInstancingHelper : MonoBehaviour
    {
        [SerializeField]
        [InspectorName("Instance Index")]
        public int InstanceIndex = 0;

        public const string      InstanceIndexName = "_InstanceIndex";
        int                      InstanceIndexID   = -1;
        MaterialPropertyBlock    SkinBlock         = null;
        SkinnedMeshRenderer      HeadMeshRenderer  = null;
        private DidimoComponents didimoComponents;

        void ProcessPropBlock(MaterialPropertyBlock propBlock, Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock, 0);
            propBlock.SetInt(InstanceIndexID, InstanceIndex);
            renderer.SetPropertyBlock(propBlock, 0);
        }

        public void OnValidate() { ApplyBlocks(); }

        public void Build(DidimoComponents components, SkinnedMeshRenderer faceMeshRenderer)
        {
            didimoComponents = components;
            HeadMeshRenderer = faceMeshRenderer;
        }

        public void ApplyBlocks()
        {
            if (!HeadMeshRenderer)
            {
                didimoComponents = GetComponent<DidimoComponents>();
                if (didimoComponents == null)
                {
                    Debug.LogWarning("No didimo components found. Disabling.");
                    enabled = false;
                }
                var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var i in meshes)
                {
                    try
                    {
                        if (didimoComponents.Parts.GetBodyPartType(i.transform) == DidimoParts.BodyPart.HeadMesh)
                        {
                            HeadMeshRenderer = i;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            ProcessIDs();
            SkinBlock ??= new MaterialPropertyBlock();
            ProcessPropBlock(SkinBlock, HeadMeshRenderer);
        }

        private void ProcessIDs()
        {
            if (InstanceIndexID == -1)
                InstanceIndexID = Shader.PropertyToID(InstanceIndexName);
        }

        private void LateUpdate() { ApplyBlocks(); }
    }
}