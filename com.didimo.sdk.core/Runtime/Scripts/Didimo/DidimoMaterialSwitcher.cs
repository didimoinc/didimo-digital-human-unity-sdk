
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
        public Material[] materials;
    }

    public class DidimoMaterialSwitcher : MonoBehaviour
    {
        public IntRange MaterialSetIndex = new IntRange(0, 0);
        int CurrentMaterialSetIndex = 0;

        public const int BASE_MATERIAL_SET = 0;
        public const int COMBINED_MATERIAL_SET = 1;
        public const int COMBINED_ATLASED_MATERIAL_SET = 2;
        public void SetMaterialSetIndex(int index)
        {
            MaterialSetIndex.Value = index;
            OnValidate();
        }

        [SerializeField]
        public List<SMatRef> MaterialSets = new List<SMatRef>();

        public Material[] GetMaterialList(int index) { return MaterialSets[index].materials; }
        Material[] GetMeshMaterialList()
        {
            List<Material> ml = new List<Material>();
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                smr.GetSharedMaterials(ml);
            else
            {
                MeshRenderer mr = GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.GetSharedMaterials(ml);
            }
            return ml.ToArray();
        }

        public void SetEntryToOwnMaterials(int idx)
        {
            var ml = GetMeshMaterialList();
            SetEntryMaterials(idx, ml);
        }

        public void SetEntryMaterials(int idx, Material[] ml)
        {
            EnsureEntryCount(idx + 1, false);
            var mref = new SMatRef();
            mref.materials = ml;
            MaterialSets[idx] = mref;
        }

        public bool HasValidEntry(int MaterialSwitcherIdx)
        {
            if (MaterialSwitcherIdx < MaterialSets.Count)
            {
                var matset = MaterialSets[MaterialSwitcherIdx];
                for (var i = 0; i < matset.materials.Length; ++i)
                {
                    if (matset.materials[i] != null)
                        return true;
                }
            }
            return false;
        }
        public void EnsureEntryCount(int count, bool setToOwnMaterials)
        {
            var ml = GetMeshMaterialList();
            while (MaterialSets.Count < count)
            {
                SMatRef matRef = new SMatRef();
                if (setToOwnMaterials)
                    matRef.materials = ml;
                else
                {
                    if (matRef.materials == null)
                        matRef.materials = new Material[ml.Length];
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
                var meshes = gameObject.GetComponentsInChildren<MeshRenderer>();
                if (CurrentMaterialSetIndex < MaterialSets.Count)
                {
                    var materialSet = MaterialSets[CurrentMaterialSetIndex].materials;
                    if (materialSet.Length > 0)
                    {
                        foreach (var m in skinnedMeshes)
                        {
                            m.sharedMaterials = materialSet.ToArray();
                        }
                        foreach (var m in meshes)
                        {
                            m.sharedMaterials = materialSet.ToArray();
                        }
                    }
                }
                else
                    Debug.Log("Current material set index out of range");
            }
        }
    }
}