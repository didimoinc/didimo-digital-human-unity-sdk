using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.Controller;

namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "New Batch Controller", menuName = "Didimo/Graph/Asset Fitter/Batch Controller")]
    public class BatchController : ScriptableObject
    {
        public GraphData graph;
        public GameObject[] assetPrefabs;
    }
}

#if UNITY_EDITOR
namespace Didimo.AssetFitter.Editor.Graph
{
    using System.Collections;
    using System.Linq;
    using UnityEditor;

    [CustomEditor(typeof(BatchController))]
    public class BatchControllerEditor : Editor
    {
        const float LineSpace = 10;
        new BatchController target => base.target as BatchController;

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            // DrawTitleImage(target.titleImage);
            // GUILayout.Space(LineSpace);

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Run();
        }

        void Run()
        {
            bool enabled = GUI.enabled;
            GUI.enabled = target.graph && target.assetPrefabs.Length > 0;
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Run", Styles.Button))
                EditorCoroutine.Start(BatchRun());
            GUILayout.EndHorizontal();
            GUI.enabled = enabled;
        }

        IEnumerator BatchRun()
        {
            GraphData graph = Instantiate(target.graph);
            CommandAvatar avatarNode = graph.FindNode<CommandAvatar>("input");
            for (int i = 0; i < target.assetPrefabs.Length; i++)
            {
                Debug.Log("Batch: Processing " + (i + 1) + "/" + target.assetPrefabs.Length + " " + target.assetPrefabs[i].name);
                avatarNode.avatarPrefab = target.assetPrefabs[i];
                yield return null;
                graph.Run();
            }
        }

        public static class Styles
        {
            static Styles()
            {
                const int FontSize = 12;

                Button = new GUIStyle(GUI.skin.button)
                { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = FontSize, richText = true };
            }

            public static GUIStyle Button;
        }

        class EditorCoroutine
        {
            readonly IEnumerator routine;
            bool completed;

            public static EditorCoroutine Start(IEnumerator routine)
            {
                EditorCoroutine coroutine = new EditorCoroutine(routine);
                coroutine.Start();
                return coroutine;
            }

            EditorCoroutine(IEnumerator _routine)
            {
                routine = _routine;
                completed = false;
            }

            void Start() => EditorApplication.update += Update;
            public void Stop()
            {
                EditorApplication.update -= Update;
                completed = true;
            }

            public bool GetCompleted() => completed;

            void Update()
            {
                if (!routine.MoveNext()) Stop();
            }
        }
    }
}
#endif