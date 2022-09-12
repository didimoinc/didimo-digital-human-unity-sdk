using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Extensions;
using UnityEditor;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.GeomTools.CompareVertex;

#if UNITY_EDITOR
namespace Didimo.AssetFitter.Editor.Graph
{
    [ExecuteInEditMode, DefaultExecutionOrder(1000)]
    public class VisualMesh : MonoBehaviour
    {
        public MeshDetails meshDetails;
        public VertexDrawer vertexDrawer;
        public RigDrawer rigDrawer;
        public Drawer[] drawers => new Drawer[] { meshDetails, vertexDrawer, rigDrawer };

        void OnValidate() { }
        void Start() { }
        void Update() { }
        void LateUpdate() { }

        internal Mesh GetMesh(out Component component)
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter && filter.sharedMesh)
            {
                component = filter;
                return filter.sharedMesh;
            }
            SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer>();
            if (skin && skin.sharedMesh)
            {
                component = skin;
                return skin.sharedMesh;
            }
            component = null;
            return null;
        }

        [Serializable]
        public class Drawer
        {
            protected VisualMeshEditor editor;
            protected virtual void OnInit() { }
            public virtual bool Visible => false;
            public void OnStart(VisualMeshEditor editor) { if (!this.editor) { this.editor = editor; OnInit(); } }
            public virtual void OnInspectorGUI(VisualMeshEditor editor) { }
            public virtual void OnSceneGUI(VisualMeshEditor editor) { }
            protected void DrawContainer(SerializedProperty property)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        [Serializable]
        public class MeshDetails : Drawer
        {
            [SerializeField] int vertexCount;
            [SerializeField] int triangleCount;
            [SerializeField] int submeshCount;
            [SerializeField] int bindPoseCount;


            protected Mesh mesh;
            protected Component meshComponent;
            public override bool Visible => mesh && mesh.isReadable;
            protected override void OnInit() => mesh = editor.visualMesh.GetMesh(out meshComponent);
            public override void OnInspectorGUI(VisualMeshEditor editor)
            {
                vertexCount = mesh.vertexCount;
                triangleCount = mesh.triangles.Length / 3;
                submeshCount = mesh.subMeshCount;
                bindPoseCount = mesh.bindposes.Length;
                DrawContainer(editor.serializedObject.FindProperty(nameof(VisualMesh.meshDetails)));
            }
        }

        [Serializable]
        public class VertexDrawer : Drawer
        {
            public bool showVertices = false;
            public bool showNormals = false;
            [Range(0.0001f, 0.01f)] public float vertexSize = 0.0015f;
            public int selectedIndex = -1;
            public MeshCollider collider;
            protected Mesh mesh;
            protected Component meshComponent;
            protected SkinnedMeshRenderer skin => meshComponent as SkinnedMeshRenderer;
            public override bool Visible => mesh;
            protected override void OnInit() => mesh = editor.visualMesh.GetMesh(out meshComponent);

            public override void OnInspectorGUI(VisualMeshEditor editor)
            {
                DrawContainer(editor.serializedObject.FindProperty(nameof(VisualMesh.vertexDrawer)));
            }

            public override void OnSceneGUI(VisualMeshEditor editor)
            {
                Handles.matrix = meshComponent.transform.localToWorldMatrix;

                if (showVertices)
                {
                    DrawVertices();
                }
                if (showNormals)
                {
                    DrawNormals();
                }
            }

            void DrawNormals()
            {
                const float Threshold = 0.001f;
                Handles.color = Color.white;
                Vector3[] vertices = mesh.vertices, normals = mesh.normals;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 origin = vertices[i];
                    Vector3 direction = normals[i];
                    Handles.DrawLine(origin, origin + direction * Threshold);
                }
            }

            void DrawVertices()
            {
                Matrix4x4 matrix = Handles.matrix;

                Plane plane = new Plane(Camera.current.transform.forward, Camera.current.transform.position);
                void draw(Vector3 v, int index)
                {
                    Handles.color = new Color(0.25f, 0.5f, 1, 0.5f);
                    if (Handles.Button(v, Quaternion.identity, vertexSize, vertexSize * 2, Handles.CubeHandleCap))
                    {
                        selectedIndex = index;
                        Debug.Log("  BoneWeights[" + index + "] " + plane.GetDistanceToPoint(v));
                    }
                    if (selectedIndex == index)
                    {
                        Handles.color = new Color(1, 1, 1, 0.5f);
                        Handles.CubeHandleCap(0, v, Quaternion.identity, vertexSize, EventType.Repaint);
                        if (skin)
                        {
                            Transform[] bones = skin.bones;
                            BoneWeights.WeightIndex[] weightIndex = MeshTools.BoneWeights.Create(mesh.boneWeights[index]);

                            Handles.color = new Color(0, 0, 0, 0.5f);
                            weightIndex.ForEach(bw => Handles.DrawLine(v, bones[bw.index].position));

                            Vector3[] deltas = weightIndex.Select(bw => (bones[bw.index].position - v) * bw.weight).ToArray();
                            Vector3 direction = deltas.Aggregate(Vector3.zero, (s, v) => s + v) / (float)weightIndex.Length;

                            Handles.color = new Color(0, 0, 0, 1);
                            weightIndex.ForEach((bw, i) => Handles.DrawLine(v, v + deltas[i]));

                            if (collider)
                            {
                                Handles.color = new Color(1, 0, 0, 1);
                                Ray ray = new Ray(v - direction.normalized * 0.25f, direction);
                                if (collider.Raycast(ray, out RaycastHit hit, 0.5f))
                                {
                                    Handles.DrawLine(v, hit.point);
                                    Handles.SphereHandleCap(1, hit.point, Quaternion.identity, vertexSize, EventType.Repaint);
                                }
                            }
                            Handles.color = new Color(1, 1, 1, 0.5f);
                            Handles.DrawLine(v, v + direction);
                        }
                    }
                }
                mesh.vertices.ForEach((v, i) => draw(v, i));
                Handles.matrix = matrix;
            }
        }

        [Serializable]
        public class RigDrawer : Drawer
        {
            public bool showBones = false;
            [Range(0.0001f, 0.01f)] public float boneSize = 0.0015f;
            public bool showConnections = false;
            [Range(0.0001f, 1)] public float maxConnectionLength = 1;
            public bool selectBonesInHierarchy = false;
            public bool showDetails = false;

            [HideInInspector] [Range(0.0001f, 0.01f)] public float connectionSize = 0.0015f;

            protected Transform selectedBone;

            public BoneRemapCompare boneRemapCompare;

            [Serializable]
            public class BoneRemapCompare
            {
                public bool show = false;
                public SkinnedMeshRenderer skin;
                public BoneIndexRemap boneRemap;
                public bool useSelectedBone;
                public bool showMissingRemap;

                public void DrawConnections(Transform[] fromBones, Transform selectedBone, float boneSize)
                {
                    if (!skin || !boneRemap || !show) return;
                    Matrix4x4 matrix = Handles.matrix;
                    Handles.matrix = Matrix4x4.identity;
                    Transform[] toBones = skin.bones;
                    Dictionary<int, int> map = boneRemap.GetRemapTable(fromBones, toBones);

                    void draw(int ifrom)
                    {
                        Transform t1 = fromBones[ifrom], t2 = toBones[map[ifrom]];
                        Handles.SphereHandleCap(0, t2.position, Quaternion.identity, boneSize, EventType.Repaint);
                        Handles.DrawLine(t1.position, t2.position);
                    }

                    if (useSelectedBone) { if (selectedBone) draw(Array.IndexOf(fromBones, selectedBone)); }
                    else map.Keys.ForEach(ifrom => draw(ifrom));
                    Color c = Handles.color;
                    Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, 0.15f);
                    toBones.ForEach(b => Handles.SphereHandleCap(0, b.position, Quaternion.identity, boneSize, EventType.Repaint));

                    if (showMissingRemap)
                    {
                        Dictionary<string, Transform> nameIndex = fromBones.ToDictionary(k => k.name, v => v);
                        Handles.color = Color.magenta;

                        foreach (var key in GetRemapDifferences(fromBones))
                        {
                            Handles.SphereHandleCap(0, nameIndex[key].position, Quaternion.identity, boneSize, EventType.Repaint);
                        }
                    }
                    Handles.color = c;
                    Handles.matrix = matrix;
                }

                IEnumerable<string> GetRemapDifferences(Transform[] fromBones) => fromBones.Select(b => b.name).Except(boneRemap.remaps.Select(r => r.from));

                public void ShowMissingRemaps(Transform[] fromBones)
                {
                    if (!showMissingRemap) return;

                    //var mapKeys = fromBones.Select(b => b.name).ToHashSet();
                    //var map = boneRemap.GetRemapTable(fromBones, skin.bones);

                    foreach (var key in GetRemapDifferences(fromBones))
                    {
                        GUILayout.Label(key);
                    }
                }
            }

            protected SkinnedMeshRenderer skin;
            public override bool Visible => skin;
            protected override void OnInit() => skin = editor.visualMesh.GetComponent<SkinnedMeshRenderer>();

            public override void OnInspectorGUI(VisualMeshEditor editor)
            {
                DrawContainer(editor.serializedObject.FindProperty(nameof(VisualMesh.rigDrawer)));
                boneRemapCompare.ShowMissingRemaps(skin.bones);
                ShowDetails();
            }

            public void ShowDetails()
            {
                if (!showDetails) return;
                GUILayout.BeginVertical("box");
                EditorGUILayout.IntField("Bineposes Count", skin.sharedMesh.bindposes.Length);
                EditorGUILayout.IntField("Boneweights Count", skin.sharedMesh.boneWeights.Length);
                int[] counts = new int[skin.sharedMesh.bindposes.Length];
                IEnumerable<BoneWeights.WeightIndex> bw = new MeshTools.BoneWeights(skin).weights.SelectMany(x => x).ForEach(x => counts[x.index]++);
                EditorGUILayout.IntField("Bone bind count", counts.Where(c => c > 0).Count());

                EditorGUILayout.IntField("Bone Count", skin.bones.Length);
                skin.bones.ForEach(b => EditorGUILayout.ObjectField(b, typeof(Transform), false));
                // if(skin.bones.Length>0 && bones[0].parent)
                // EditorGUILayout.HelpBox();
                GUILayout.EndVertical();
            }

            public override void OnSceneGUI(VisualMeshEditor editor)
            {
                if (!showBones) return;
                DrawRig(skin);
                boneRemapCompare.DrawConnections(skin.bones, selectedBone, boneSize);
            }

            void DrawRig(SkinnedMeshRenderer skin)
            {
                BoneWeights bones = new BoneWeights(skin);
                List<int> indices = bones.weights.SelectMany(w => w.Select(b => b.index)).Distinct().ToList();

                bool[] boneBound = new bool[skin.bones.Length];
                indices.ForEach(i => boneBound[i] = true);

                void DrawBones(Transform bone, bool bound)
                {
                    Handles.color = bound ? new Color(0, 1, 1, 0.5f) : new Color(0, 1, 0, 0.2f);
                    if (showConnections)
                        foreach (Transform child in bone)
                            if (Vector3.Distance(bone.position, child.position) < maxConnectionLength)
                                Handles.DrawLine(bone.position, child.position);
                    // VisualMeshEditor.Styles.DrawLine(Handles.color, bone.position, child.position);
                    // VisualMeshEditor.Styles.DrawTriangleArrow(bone.position, child.position, Quaternion.identity, connectionSize);
                    if (Handles.Button(bone.position, Quaternion.identity, boneSize, boneSize, Handles.SphereHandleCap))
                    {
                        selectedBone = bone;
                        Debug.Log(editor.target.name + " " + selectedBone.name);
                        if (selectBonesInHierarchy)
                            Selection.objects = new UnityEngine.Object[] { editor.visualMesh.gameObject, selectedBone.gameObject };
                    }

                    if (selectedBone)
                    {
                        Handles.color = new Color(1, 1, 1, 0.5f);
                        Handles.SphereHandleCap(0, selectedBone.position, Quaternion.identity, boneSize, EventType.Repaint);
                    }
                }
                skin.bones.Select((b, i) => (b, i)).Where(g => g.b).ForEach(g => DrawBones(g.b, boneBound[g.i]));
            }
        }
    }

    [CustomEditor(typeof(VisualMesh), true)]
    public class VisualMeshEditor : UnityEditor.Editor
    {
        public VisualMesh visualMesh => target as VisualMesh;

        public override void OnInspectorGUI()
        {
            visualMesh.drawers.ForEach(d => d.OnStart(this));
            visualMesh.drawers.Where(d => d.Visible).ForEach(d => d.OnInspectorGUI(this));
            serializedObject.ApplyModifiedProperties();
        }

        protected void OnSceneGUI()
        {
            if (!visualMesh.enabled) return;
            visualMesh.drawers.ForEach(d => d.OnStart(this));
            visualMesh.drawers.Where(d => d.Visible).ForEach(d => d.OnSceneGUI(this));
        }

        public static class Styles
        {
            static Styles() { }

            public static void GUIDrawLine(Color color, float height = 2)
            {
                Rect rect = EditorGUILayout.GetControlRect();
                rect = new Rect(rect.x, rect.y + (rect.height - height) / 2, rect.width, height);
                EditorGUI.DrawRect(rect, color);
            }

            public static void DrawLine(Color color, Vector3 p1, Vector3 p2, float thickness = 0.01f)
            {
                Handles.DrawAAPolyLine(GetTextureColor(color), thickness, p1, p2);
            }

            public static void DrawTriangleArrow(Vector3 p1, Vector3 p2, Quaternion rotation, float size = 0.01f)
            {
                Vector3 v1 = new Vector3(1, 0, 0), v2 = new Vector3(-0.5f, 0.8660254f, 0), v3 = new Vector3(-0.5f, -0.8660254f, 0);

                const float UnitLength = 0.2f;

                Vector3 delta = p1 - p2;
                float length = delta.magnitude;
                if (length < 0.00001f || length > 0.7f) return;

                Quaternion q = Quaternion.LookRotation(delta);
                float s = Mathf.Min(size, (length / UnitLength) * size);

                Vector3 pt1 = p1 + q * v1 * s, pt2 = p1 + q * v2 * s, pt3 = p1 + q * v3 * s;

                Handles.DrawAAConvexPolygon(pt1, pt2, pt3);
                Handles.DrawAAConvexPolygon(pt1, pt2, p2);
                Handles.DrawAAConvexPolygon(pt2, pt3, p2);
                Handles.DrawAAConvexPolygon(pt3, pt1, p2);
            }

            public static void DrawLabelField(string label, object value, GUIStyle style = null)
            {
                if (style == null) EditorGUILayout.LabelField(label, value?.ToString());
                else EditorGUILayout.LabelField(label, value?.ToString(), style);
            }

            public static void DrawLabel(string text)
            { if (!string.IsNullOrEmpty(text)) GUILayout.Label(text); }

            public static void DrawLabel(string text, GUIStyle style)
            { if (!string.IsNullOrEmpty(text)) GUILayout.Label(text, style); }

            static Dictionary<Color, Texture2D> TextureColors = new Dictionary<Color, Texture2D>();
            static Texture2D GetTextureColor(Color color)
            {
                if (TextureColors == null) TextureColors = new Dictionary<Color, Texture2D>();
                if (!TextureColors.ContainsKey(color))
                {
                    TextureColors[color] = new Texture2D(1, 1);
                    TextureColors[color].SetPixel(0, 0, color);
                    TextureColors[color].Apply();
                }
                return TextureColors[color];
            }
        }
    }
}
#else
namespace Didimo.AssetFitter.Editor.Graph { public class VisualMesh : MonoBehaviour { } }
#endif


// using System.Linq;
// using UnityEngine;
// using static Didimo.AssetFitter.Editor.Graph.GeomTools;
// using static Didimo.AssetFitter.Editor.Graph.MeshTools;

// namespace Didimo.AssetFitter.Editor.Graph
// {
//     [ExecuteInEditMode, DefaultExecutionOrder(1000)]
//     public class VisualMesh : MonoBehaviour
//     {
//         [Header("Visibility")]
//         public bool showDetails = false;
//         public bool showRig = false;
//         public bool showVertices = false;
//         public bool showBindings = true;
//         public bool showSeams = false;
//         public bool showIntersectingEdges = false;
//         public bool showSimilarPoints = false;
//         [Range(0, 0.001f)] public float similarThreshold = 0.00001f;

//         public bool showVertexOrder = false;
//         public int vertexIndex = -1;

//         public bool showTriangleOrder = false;
//         public int triangleIndex = 0;

//         internal MeshFilter filter => GetComponentInChildren<MeshFilter>();
//         internal SkinnedMeshRenderer skin => GetComponentInChildren<SkinnedMeshRenderer>();


//         void Update() { }

//         void OnDrawGizmos()
//         {
//             var mesh = GetMesh();
//             if (mesh)
//             {
//                 DrawVertexOrder(mesh);
//                 DrawIndexOrder(mesh);
//             }
//         }

//         void DrawVertexOrder(Mesh mesh)
//         {
//             if (!enabled || !showVertexOrder) return;
//             Gizmos.matrix = transform.localToWorldMatrix;
//             Gizmos.color = Color.red;

//             int i = (int)Mathf.Repeat(vertexIndex, mesh.vertexCount);
//             var p = mesh.vertices[i];
//             var n = mesh.normals[i];
//             Gizmos.DrawLine(p, p + n * 0.25f);
//         }

//         void DrawIndexOrder(Mesh mesh)
//         {
//             if (!enabled || !showTriangleOrder) return;
//             Gizmos.matrix = transform.localToWorldMatrix;
//             Gizmos.color = Color.red;
//             var triangles = mesh.triangles;
//             int i = (int)Mathf.Repeat(triangleIndex, mesh.triangles.Length / 3);
//             var p1 = mesh.vertices[triangles[i * 3 + 0]];
//             var p2 = mesh.vertices[triangles[i * 3 + 1]];
//             var p3 = mesh.vertices[triangles[i * 3 + 2]];
//             Gizmos.DrawLine(p1, p2);
//             Gizmos.DrawLine(p2, p3);
//             Gizmos.DrawLine(p3, p1);
//         }

//         Mesh GetMesh()
//         {
//             if (filter && filter.sharedMesh) return filter.sharedMesh;
//             else if (skin && skin.sharedMesh) return skin.sharedMesh;
//             return null;
//         }

//         Mesh bakedMesh;
//         public void LateUpdate()
//         {
//             if (!showVertices) return;
//             if (!skin) return;
//             skin.BakeMesh(bakedMesh ? bakedMesh : bakedMesh = new Mesh());
//             var m = skin.transform.localToWorldMatrix;
//             bakedMesh.vertices = bakedMesh.vertices.Select(v => m.MultiplyPoint(v)).ToArray();
//         }
//     }
// }

// #if UNITY_EDITOR
// namespace Didimo.AssetFitter.Editor.Graph
// {
//     using System;
//     using System.Collections.Generic;
//     using Didimo.Extensions;
//     using UnityEditor;

//     [CustomEditor(typeof(VisualMesh), true)]
//     public class VisualMeshEditor : UnityEditor.Editor
//     {
//         new Target target;

//         class Target
//         {
//             public VisualMesh visualMesh;
//             public SkinnedMeshRenderer skin;
//             public MeshFilter filter;
//             public MeshRenderer renderer;
//             public Transform transform;
//             public bool enabled => visualMesh.enabled && transform;
//             public Camera camera;
//             public Mesh mesh;
//             public Target(VisualMesh target)
//             {
//                 visualMesh = target;
//                 skin = target.GetComponentInChildren<SkinnedMeshRenderer>();
//                 if (!skin)
//                 {
//                     filter = target.GetComponentInChildren<MeshFilter>();
//                     if (filter)
//                     {
//                         renderer = filter.GetComponent<MeshRenderer>();
//                         transform = filter.transform;
//                         mesh = filter.sharedMesh;
//                     }
//                 }
//                 else
//                 {
//                     transform = skin.transform;
//                     mesh = skin.sharedMesh;
//                 }
//                 camera = Camera.current;
//             }
//         }

//         bool UpdateTarget()
//         {
//             target = new Target(base.target as VisualMesh);
//             return target.enabled;
//         }

//         public override void OnInspectorGUI()
//         {
//             if (!UpdateTarget()) return;
//             base.OnInspectorGUI();

//             if (target.visualMesh.showDetails)
//                 DrawGUI();
//         }

//         protected void OnSceneGUI()
//         {
//             if (!UpdateTarget()) return;
//             DrawGUI();
//             if (!target.camera) return;
//             DrawSkin();
//             DrawFilter();
//         }

//         void DrawSkin()
//         {
//             SkinnedMeshRenderer skin = target.transform.GetComponent<SkinnedMeshRenderer>();
//             if (skin == null || skin.sharedMesh == null) return;

//             if (target.visualMesh.showRig) DrawRig(skin, target.visualMesh.showBindings);

//             DrawMesh(skin.sharedMesh, skin.transform);
//         }

//         void DrawBinding(SkinnedMeshRenderer skin, int index)
//         {
//             if (index != -1)
//             {
//                 // var bindPose = skin.sharedMesh.bindposes[index];
//                 var boneWeights = skin.sharedMesh.boneWeights[index];

//                 Debug.Log(index + " " +
//                     skin.bones[boneWeights.boneIndex0].name + " " + boneWeights.weight0 + " " +
//                     skin.bones[boneWeights.boneIndex1].name + " " + boneWeights.weight1 + " " +
//                     skin.bones[boneWeights.boneIndex2].name + " " + boneWeights.weight2 + " " +
//                     skin.bones[boneWeights.boneIndex3].name + " " + boneWeights.weight3 + " ");

//                 var p = target.transform.localToWorldMatrix.MultiplyPoint(skin.sharedMesh.vertices[index]);
//                 if (boneWeights.weight0 > 0) Handles.DrawLine(p, skin.bones[boneWeights.boneIndex0].position);
//                 if (boneWeights.weight1 > 0) Handles.DrawLine(p, skin.bones[boneWeights.boneIndex1].position);
//                 if (boneWeights.weight2 > 0) Handles.DrawLine(p, skin.bones[boneWeights.boneIndex2].position);
//                 if (boneWeights.weight3 > 0) Handles.DrawLine(p, skin.bones[boneWeights.boneIndex3].position);
//             }
//         }

//         public static void DrawRig(SkinnedMeshRenderer skin, bool showBindings)
//         {
//             const float S = 0.004f;
//             Vector3 v1 = new Vector3(1, 0, 0), v2 = new Vector3(-0.5f, 0.8660254f, 0), v3 = new Vector3(-0.5f, -0.8660254f, 0);

//             var weights = skin.sharedMesh.boneWeights;
//             var indices = weights.SelectMany(w => new[] { w.boneIndex0, w.boneIndex1, w.boneIndex2, w.boneIndex3 }).Distinct().Where(w => w > 0).ToList();

//             // Debug.Log(String.Join(",", indices));
//             var selectedBones = new bool[skin.bones.Length];
//             indices.ForEach(i => selectedBones[i] = true);

//             void DrawBones(Transform bone, bool selected)
//             {
//                 Handles.color = selected ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.2f);
//                 if (bone.childCount > 0)
//                 {
//                     foreach (Transform child in bone)
//                         PrimitiveDrawer.DrawTriangleArrow(bone.position, child.position, Quaternion.identity, 0.01f);
//                 }
//                 else Handles.SphereHandleCap(0, bone.position, Quaternion.identity, S, EventType.Repaint);
//             }
//             skin.bones.Select((b, i) => (b, i)).Where(g => g.b).ForEach(g => DrawBones(g.b, selectedBones[g.i] && showBindings));
//         }

//         void DrawFilter()
//         {
//             if (!target.filter) return;
//             DrawMesh(target.filter.sharedMesh, target.transform);
//         }

//         MeshCache meshCache;
//         class MeshCache
//         {
//             public Mesh mesh;
//             public Vector3[] vertices;
//             public Vector3[][] seams;
//             public List<Vector3> similarPositions;
//             public float similarThreshold;
//         }

//         void DrawMesh(Mesh mesh, Transform transform)
//         {
//             if (!mesh) return;

//             if (meshCache == null || meshCache.mesh != mesh)
//                 meshCache = new MeshCache { mesh = mesh, vertices = mesh.vertices };

//             Handles.matrix = transform.localToWorldMatrix;

//             DrawVertices(mesh);
//             DrawSeams(mesh);
//             DrawSimilarPositions(mesh);
//             // drawIntersectingEdges(mesh);
//         }

//         void DrawVertices(Mesh mesh)
//         {
//             if (!target.visualMesh.showVertices) return;

//             var plane = new Plane(Camera.current.transform.forward, Camera.current.transform.position);

//             void draw(Vector3 v, int index)
//             {
//                 if (Handles.Button(v, Quaternion.identity, 0.0015f, 0.002f, Handles.CubeHandleCap))
//                 {
//                     Debug.Log("  BoneWeights[" + index + "] " + plane.GetDistanceToPoint(v));
//                     // var boneWeights = new BoneWeights(clone.headSkin);
//                     // Debug.Log("  BoneWeights[" + index + "]: " + String.Join(",", boneWeights[index].Select(bw => boneWeights.names[bw.index] + " " + bw.weight)));
//                 }
//             }
//             mesh.vertices.ForEach((v, i) => draw(v, i));
//         }

//         void DrawSimilarPositions(Mesh mesh)
//         {
//             if (!target.visualMesh.showSimilarPoints) return;

//             if (meshCache.similarPositions == null || meshCache.similarThreshold != target.visualMesh.similarThreshold)
//             {
//                 meshCache.similarPositions = CompareVertex.Positions(meshCache.vertices, target.visualMesh.similarThreshold).
//                     Select(i => meshCache.vertices[i]).ToList();
//                 meshCache.similarThreshold = target.visualMesh.similarThreshold;
//             }

//             Handles.color = Color.cyan;
//             meshCache.similarPositions.ForEach(p => DrawPoint(p));
//         }

//         void DrawPoint(Vector3 p) => Handles.CubeHandleCap(0, p, Quaternion.identity, 0.001f, EventType.Repaint);

//         void DrawSeams(Mesh mesh)
//         {
//             if (!target.visualMesh.showSeams) return;
//             FindSeams(mesh);
//             Handles.color = Color.red;
//             foreach (var seam in meshCache.seams)
//             {
//                 Handles.DrawLine(seam[0], seam[1]);
//             }
//         }


//         static int[] offsets = { +1, +1, -2 };
//         long GetEdgeID(int _0, int _1) => _0 > _1 ? ((long)_0 << 32) | (long)_1 : ((long)_1 << 32) | ((long)_0);

//         void FindSeams(Mesh mesh)
//         {
//             if (meshCache.seams != null) return;
//             Vector3[] vertices = mesh.vertices;
//             meshCache.seams = Seams.GetEdges(mesh).Select(edge => edge.Select(i => vertices[i]).ToArray()).ToArray();
//         }

//         void DrawGUI()
//         {

//             DrawSkinGUI();
//             DrawMeshGUI();
//         }

//         static string BoneSearch = "";
//         void DrawSkinGUI()
//         {
//             if (target.skin == null) return;

//             Styles.DrawLine(new Color(0, 0, 0, 0.3f), 1);
//             Styles.DrawLabel("Skin details", EditorStyles.boldLabel);
//             Styles.DrawLabelField("Root Bone", target.skin.rootBone);
//             Styles.DrawLabelField("Bones Count", target.skin.bones.Length);

//             BoneSearch = Styles.SearchField(BoneSearch, "Bone search");
//             if (BoneSearch != "")
//                 foreach (var bone in (target.skin.bones.Where(b => BoneSearch == "*" || b.name.Contains(BoneSearch))))
//                     Styles.DrawLabel(bone.name + " (parent:" + bone.parent.name + ")");
//         }

//         void DrawMeshGUI()
//         {
//             if (!target.mesh) return;

//             Styles.DrawLine(new Color(0, 0, 0, 0.3f), 1);

//             Styles.DrawLabel("Mesh details", EditorStyles.boldLabel);

//             var cs = new int[target.mesh.bindposes.Length];
//             foreach (var w in target.mesh.boneWeights) { cs[w.boneIndex0]++; cs[w.boneIndex1]++; cs[w.boneIndex2]++; cs[w.boneIndex3]++; }

//             Styles.DrawLabelField("Vertex Count", target.mesh.vertexCount);
//             Styles.DrawLabelField("Triangle Count", target.mesh.triangles.Length / 3);
//             Styles.DrawLabelField("First 6 Indices", String.Join(",", target.mesh.triangles.Take(6)));
//             Styles.DrawLabelField("Submesh Count", target.mesh.subMeshCount);
//             Styles.DrawLabelField("Blendshape Count", target.mesh.blendShapeCount);
//             Styles.DrawLabelField("BoneWeights Count", target.mesh.boneWeights.Length);
//             Styles.DrawLabelField("Bound Count", cs.Count(c => c != 0));

//             for (int i = 0; target.mesh.subMeshCount > 1 && i < target.mesh.subMeshCount; i++)
//             {
//                 var indices = target.mesh.GetTriangles(i);
//                 Styles.DrawLabelField("Submesh:" + i, " V:" + indices.Distinct().Count() + " T:" + indices.Length / 3);
//             }
//         }

//         static class PrimitiveDrawer
//         {
//             public static void DrawTriangleArrow(Vector3 p1, Vector3 p2, Quaternion rotation, float size = 0.01f)
//             {
//                 Vector3 v1 = new Vector3(1, 0, 0), v2 = new Vector3(-0.5f, 0.8660254f, 0), v3 = new Vector3(-0.5f, -0.8660254f, 0);

//                 const float S = 0.01f, L = 0.2f;

//                 var delta = p1 - p2;
//                 var length = delta.magnitude;
//                 if (length < 0.00001f || length > 0.7f) return;

//                 var q = Quaternion.LookRotation(delta);
//                 float s = Mathf.Min(S, (length / L) * S);

//                 Vector3 pt1 = p1 + q * v1 * s, pt2 = p1 + q * v2 * s, pt3 = p1 + q * v3 * s;

//                 Handles.DrawAAConvexPolygon(pt1, pt2, pt3);
//                 Handles.DrawAAConvexPolygon(pt1, pt2, p2);
//                 Handles.DrawAAConvexPolygon(pt2, pt3, p2);
//                 Handles.DrawAAConvexPolygon(pt3, pt1, p2);
//             }
//         }

//         static class Styles
//         {
//             static Styles()
//             {
//                 Body = new GUIStyle(EditorStyles.label)
//                 { wordWrap = true, fontSize = 14, richText = true, };

//                 DragAndDrop = new GUIStyle(EditorStyles.label)
//                 { wordWrap = true, fontStyle = FontStyle.Bold, fontSize = 18, richText = true, alignment = TextAnchor.MiddleCenter, };

//                 Title = new GUIStyle(EditorStyles.label)
//                 { normal = { textColor = Color.white }, wordWrap = true, fontSize = 26, richText = true, };

//                 Header = new GUIStyle(EditorStyles.label)
//                 { wordWrap = true, fontStyle = FontStyle.Bold, fontSize = 18, richText = true, };

//                 Watermark = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1, 1, 1, 0.1f) } };

//                 Link = new GUIStyle(EditorStyles.label)
//                 { wordWrap = false, normal = { textColor = new Color32(255, 154, 0, 255) }, fontSize = 14, richText = true, }; //stretchWidth = false

//                 PreviewGameObject = new GUIStyle();

//                 Button = new GUIStyle(GUI.skin.button);
//             }

//             public static GUIStyle Title, Header, Body, Link, DragAndDrop, PreviewGameObject, Button, Watermark;

//             public static void DrawLine(Color color, float height = 2)
//             {
//                 var rect = EditorGUILayout.GetControlRect();
//                 rect = new Rect(rect.x, rect.y + (rect.height - height) / 2, rect.width, height);
//                 EditorGUI.DrawRect(rect, color);
//             }

//             public static void DrawLabelField(string label, object value, GUIStyle style = null)
//             {
//                 if (style == null) EditorGUILayout.LabelField(label, value?.ToString());
//                 else EditorGUILayout.LabelField(label, value?.ToString(), style);
//             }

//             public static void DrawLabel(string text)
//             { if (!string.IsNullOrEmpty(text)) GUILayout.Label(text); }

//             public static void DrawLabel(string text, GUIStyle style)
//             { if (!string.IsNullOrEmpty(text)) GUILayout.Label(text, style); }

//             public static void DrawLinkButton(string link, GUIStyle style)
//             { if (!string.IsNullOrEmpty(link) && GUILayout.Button(link, Styles.Link)) Application.OpenURL(link); }

//             public static void DrawTitleImage(Texture2D image) =>
//                 GUILayout.Label(image, GUILayout.Width(Screen.width), GUILayout.Height(Screen.width * image.height / image.width));

//             public static string SearchField(string text, string watermark)
//             {
//                 var rect = EditorGUILayout.GetControlRect();
//                 // GUI.SetNextControlName(watermark);
//                 text = EditorGUI.TextField(rect, text);
//                 if (text == "") EditorGUI.LabelField(rect, watermark, Styles.Watermark);
//                 // Debug.Log(GUI.GetNameOfFocusedControl() == watermark);
//                 return text;
//             }

//             static Texture2D GetTextureColor(Color color)
//             { var t = new Texture2D(1, 1); t.SetPixel(0, 0, color); t.Apply(); return t; }
//         }

//     }
// }
// #endif
