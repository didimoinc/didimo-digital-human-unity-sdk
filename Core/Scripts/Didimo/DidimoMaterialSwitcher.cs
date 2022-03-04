
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

    public class DidimoMaterialSwitcher : MonoBehaviour
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

        public List<Material> GetMaterialList(int index) { return MaterialSets[index].materials; }
        void GetMeshMaterialList(List<Material> ml)
        {
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                smr.GetSharedMaterials(ml);
            else
            {
                MeshRenderer mr = GetComponent<MeshRenderer>();
                mr.GetSharedMaterials(ml);
            }
        }

        public void SetEntryToOwnMaterials(int idx)
        {
            List<Material> ml = new List<Material>();
            GetMeshMaterialList(ml);
            SetEntryMaterials(idx, ml);
        }

        public void SetEntryMaterials(int idx, List<Material> ml)
        {
            EnsureEntryCount(idx + 1, false);
            var mref = new SMatRef();
            mref.materials = ml;
            MaterialSets[idx] = mref;
        }

        public void EnsureEntryCount(int count, bool setToOwnMaterials)
        {
            List<Material> ml = new List<Material>();
            GetMeshMaterialList(ml);
            while (MaterialSets.Count < count)
            {
                SMatRef matRef = new SMatRef();
                if (setToOwnMaterials)
                    matRef.materials = ml;
                else
                {
                    if (matRef.materials == null)
                        matRef.materials = new List<Material>();
                    for (var i = 0; i < ml.Count; ++i)
                    {
                        matRef.materials.Add(null);
                    }
                }
                MaterialSets.Add(matRef);
            }
            if (MaterialSetIndex.MaxValue != MaterialSets.Count)
                MaterialSetIndex.MaxValue = MaterialSets.Count;
        }

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