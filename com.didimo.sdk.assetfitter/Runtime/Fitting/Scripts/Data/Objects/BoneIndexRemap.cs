using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "BoneIndexRemap", menuName = "Didimo/Graph/Bone Index Remap", order = 20)]
    public class BoneIndexRemap : ScriptableObject
    {
        [Serializable]
        public class BoneRemap
        {
            public string from, to;
        }

        [SerializeField]
        List<BoneRemap> remaps;

        public Dictionary<int, int> GetRemapTable(Transform[] sourceBones, Transform[] targetBones)
        {
            Dictionary<string, int> iSource = BoneNameToIndex(sourceBones), iTarget = BoneNameToIndex(targetBones);

            var remap = new Dictionary<int, int>();
            foreach (var bone in remaps)
                if (iSource.TryGetValue(bone.from, out int s) && iTarget.TryGetValue(bone.to, out int t))
                    remap.Add(s, t);

            return remap;
        }

        public Dictionary<string, int> BoneNameToIndex(Transform[] bones) =>
            bones.Select((b, i) => (b, i)).ToDictionary(k => k.b.name, v => v.i);

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

            var style = new GUIStyle(GUI.skin.textField);
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
        }
    }
#endif
}

