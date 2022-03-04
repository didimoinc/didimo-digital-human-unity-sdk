using Didimo.Core.Config;
using Didimo.Core.Utility;
using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo
{
    [ExecuteInEditMode]
    public class DidimoInstancingHelper : MonoBehaviour
    {
        [SerializeField]
        [InspectorName("Instance Index")]
        public int InstanceIndex = 0;

        public const string InstanceIndexName = "_InstanceIndex";

        int InstanceIndexID = -1;
        MaterialPropertyBlock SkinBlock = null;
        SkinnedMeshRenderer HeadMeshRenderer = null;

        void ProcessPropBlock(MaterialPropertyBlock propBlock, Renderer renderer)
        {
            renderer.GetPropertyBlock(propBlock, 0);
            propBlock.SetInt(InstanceIndexID, InstanceIndex);
            renderer.SetPropertyBlock(propBlock, 0);
        }

        public void OnValidate() { ApplyBlocks(); }

        public void Build(SkinnedMeshRenderer faceMeshRenderer) { HeadMeshRenderer = faceMeshRenderer; }

        public void ApplyBlocks()
        {
            if (!HeadMeshRenderer)
            {
                var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var i in meshes)
                {
                    if (i.sharedMesh.name.ToLower().Contains("face"))
                        HeadMeshRenderer = i;
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