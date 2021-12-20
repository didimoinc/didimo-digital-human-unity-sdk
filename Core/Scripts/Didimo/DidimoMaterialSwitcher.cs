
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Didimo
{
    [System.Serializable]
    public struct SMatRef
    {
        public List<Material> materials;
    }
        
    class DidimoMaterialSwitcher : MonoBehaviour
    {      
        public IntRange MaterialSetIndex = new IntRange(0, 0);
        int CurrentMaterialSetIndex = 0;

        public void SetMaterialSetIndex(int index)
        {
            MaterialSetIndex.Value = index;
            OnValidate();
        }
        [SerializeField]
        public List<SMatRef> MaterialSets = new List<SMatRef>();


        void OnValidate()
        {
            if (MaterialSets.Count == 0)
                return;
            if (MaterialSetIndex.MaxValue != MaterialSets.Count)
                MaterialSetIndex.MaxValue = MaterialSets.Count;

            if (CurrentMaterialSetIndex != MaterialSetIndex.Value)
            {
                CurrentMaterialSetIndex = MaterialSetIndex.Value;
                var skinnedMeshes = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

                var materialSet = MaterialSets[CurrentMaterialSetIndex].materials;
                if (materialSet.Count > 0)
                {
                    foreach (var m in skinnedMeshes)
                    {
                        m.sharedMaterials = materialSet.ToArray();
                    }
                    var meshes = gameObject.GetComponentsInChildren<MeshRenderer>();
                    foreach (var m in meshes)
                    {
                        m.sharedMaterials = materialSet.ToArray();
                    }
                }
            }
        }
    }
}
