using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Skinned Mesh/Skinned Mesh")]
    [DisplayName("Skinned Mesh")]
    [Width(160)]
    [HeaderColor(TypeColors.SkinnedMeshRenderer)]

    public class CommandSkinnedMesh : GraphNode
    {
        [Input("Skin")] public SkinnedMeshRenderer skinInput;
        [Output("Root Bone")] public Transform rootBoneOutput;
        [Output("Bones")] public Transform[] bonesOutput;
        [Output("Materials")] public Material[] materialsOutput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            var skins = GetInputValues<SkinnedMeshRenderer>(nameof(skinInput));

            switch (info.Name)
            {
                case nameof(meshOutput):
                    values = new List<object>(skins.Select(s => s.sharedMesh).Where(m => m).Select(m => CloneAsset(m)));
                    return true;
                case nameof(rootBoneOutput):
                    values = skins.Select(s => s.rootBone).ToList<object>();
                    return true;
                case nameof(bonesOutput):
                    values = skins.Select(s => s.bones).ToList<object>();
                    return true;
                case nameof(materialsOutput):
                    values = skins.Select(s => s.sharedMaterials).ToList<object>();
                    return true;
            }
            return base.GetOutputValues(info, out values);
        }
    }
}