using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GeomTools.CompareVertex;

namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "BoneIndexRemap", menuName = "Didimo/Graph/Bone Index Remap", order = 20)]
    public class BoneIndexRemap : ScriptableObject
    {
        [Serializable]
        public class BoneRemap
        {
            public string from, to;
            public static implicit operator bool(BoneRemap empty) => empty != null;
        }

        [SerializeField]
        internal List<BoneRemap> remaps;

        public Dictionary<int, int> GetRemapTable(Transform[] sourceBones, Transform[] targetBones)
        {
            Dictionary<string, int> sourceBoneNameToIndex = BoneNameToIndex(sourceBones), targetBoneNameToIndex = BoneNameToIndex(targetBones);

            // var positions = sourceBones.Select(b => b.position).ToArray();

            // var partition = new Partition3D<int>(positions, 0.01f);
            // for (int i = 0; i < positions.Length; i++)
            //     partition.Add(i, positions[i]);

            // int MatchingBone()
            // {
            //     for (int j = 0; j < positions.Length; j++)
            //         foreach (int i in partition.Get(positions[j]))
            //             if (j != i && Position(positions[j], positions[i], Mathf.Epsilon))
            //                 return i;
            //     return -1;
            // }

            Dictionary<int, int> remap = new Dictionary<int, int>();
            Dictionary<string, BoneRemap> fromToBoneRemap = remaps.ToDictionary(k => k.from, v => v);
            Dictionary<string, string> rogueBones = new Dictionary<string, string>();

            void FindRogueBones(Transform bone)
            {
                if (!fromToBoneRemap.ContainsKey(bone.name))
                {
                    foreach (var r in remaps)
                    {
                        if (bone.name.EndsWith(r.from, StringComparison.OrdinalIgnoreCase))
                        {
                            rogueBones.Add(bone.name, r.to);
                            return;
                        }
                    }

                    Debug.LogWarning("'" + bone.name + "'" + " can't be found or remapped");
                }
            }

            foreach (var bone in sourceBones)
            {
                FindRogueBones(bone);
            }

            foreach (var bone in remaps)
            {
                if (sourceBoneNameToIndex.TryGetValue(bone.from, out int s) && targetBoneNameToIndex.TryGetValue(bone.to, out int t))
                    remap[s] = t;
            }

            foreach (var d in rogueBones)
            {
                if (targetBoneNameToIndex.TryGetValue(d.Value, out int t))
                    remap[sourceBoneNameToIndex[d.Key]] = t;
            }

            return remap;
        }

        public Dictionary<string, int> BoneNameToIndex(Transform[] bones) =>
            bones.Select((b, i) => (b, i)).ToDictionary(k => k.b.name, v => v.i);

        internal Transform FindParentBone(Transform transform, Transform[] sourceBones, Transform[] targetBones)
        {
            Dictionary<string, int> targetBoneNameToIndex = BoneNameToIndex(targetBones);
            Dictionary<string, BoneRemap> fromToBoneRemap = remaps.ToDictionary(k => k.from, v => v);

            for (Transform parent = transform.parent; parent; parent = parent.parent)
            {
                if (fromToBoneRemap.TryGetValue(parent.name, out BoneRemap remap))
                {
                    if (targetBoneNameToIndex.TryGetValue(remap.to, out int t))
                        return targetBones[t];
                }
            }
            return null;
        }
    }
}

namespace Didimo.AssetFitter.Editor.Graph
{
#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(BoneIndexRemap.BoneRemap))]
    public class BoneRemapDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect controlRect = EditorGUI.PrefixLabel(position, label);
            Rect r1 = new Rect(controlRect) { width = controlRect.width / 2 };
            Rect r2 = new Rect(r1) { x = r1.x + r1.width + 2 };

            SerializedProperty pFrom = property.FindPropertyRelative("from");
            SerializedProperty pTo = property.FindPropertyRelative("to");

            GUIStyle style = new GUIStyle(GUI.skin.textField);
            style.normal.textColor = new Color(1, 1, 1, 0.2f);

            pFrom.stringValue = EditorGUI.TextField(r1, pFrom.stringValue);
            if (pFrom.stringValue == "")
            {
                GUI.Label(r1, "From Bone", style);
            }

            pTo.stringValue = EditorGUI.TextField(r2, pTo.stringValue);
            if (pTo.stringValue == "")
            {
                GUI.Label(r2, "To Bone", style);
            }

            property.serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
