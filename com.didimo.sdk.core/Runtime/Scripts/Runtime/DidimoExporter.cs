using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Didimo.Core.Inspector;
using GLTFast.Export;
using GLTFast.Schema;
using UnityEditor;
using UnityEngine;

namespace Didimo
{
    public class DidimoExporter : MonoBehaviour
    {
        public string path;
        public DidimoComponents didimo;

#if UNITY_EDITOR
        [Button]
        public void Export()
        {
            string gltfAnimationSource = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(didimo);
            Debug.Log(gltfAnimationSource);
            Export(didimo, path, gltfAnimationSource);
        }
        
#endif

        public static async void Export(DidimoComponents didimoComponents, string path,
            string gltfAnimationSource = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            // GameObjectExport lets you create glTFs from GameObject hierarchies
            var export = new GameObjectExport();

            // Add a scene
            export.AddScene(new[] {didimoComponents.gameObject});

            // In Unity, there is no way to get animation curves in runtime, so for now we have this work-around
            // This implementation will work while didimos have a .bin file specifically for animations
            if (!string.IsNullOrEmpty(gltfAnimationSource))
            {
                async Task BeforeSaveAction()
                {
                    var sourceAnimationGltfRoot = await Task.Run(() =>
                        GLTFast.JsonParser.ParseJson(File.ReadAllText(gltfAnimationSource)));

                    if (sourceAnimationGltfRoot == null)
                    {
                        Debug.LogError("JsonParsingFailed");
                        return;
                    }

                    export.m_Writer.m_Animations = sourceAnimationGltfRoot.animations;

                    var animationAccessor =
                        sourceAnimationGltfRoot.accessors[
                            sourceAnimationGltfRoot.animations.First().samplers.First().input];
                    var animationBufferView = sourceAnimationGltfRoot.bufferViews[animationAccessor.bufferView];
                    var animationBuffer = sourceAnimationGltfRoot.buffers[animationBufferView.buffer];
                    export.m_Writer.m_animationBuffer =
                        animationBuffer; // Assumes the didimo has a single animation buffer
                    string binPath = animationBuffer.uri;

                    File.Copy(Path.Combine(Path.GetDirectoryName(gltfAnimationSource), binPath),
                        Path.Combine(Path.GetDirectoryName(path), binPath), true);

                    foreach (var animation in sourceAnimationGltfRoot.animations)
                    {
                        void AddAccessorAndBufferView(int sourceAccessorId)
                        {
                            var accessor = sourceAnimationGltfRoot.accessors[sourceAccessorId];
                            var bufferView = sourceAnimationGltfRoot.bufferViews[accessor.bufferView];
                            BufferView newBufferView = new BufferView
                            {
                                buffer = 1,
                                target = bufferView.target,
                                byteLength = bufferView.byteLength,
                                byteOffset = bufferView.byteOffset,
                                byteStride = bufferView.byteStride
                            };
                            export.m_Writer.m_BufferViews.Add(newBufferView);

                            Accessor newAccessor = new Accessor
                            {
                                bufferView = export.m_Writer.m_BufferViews.Count - 1,
                                max = accessor.max,
                                min = accessor.min,
                                count = accessor.count,
                                normalized = accessor.normalized,
                                sparse = accessor.sparse,
                                byteOffset = accessor.byteOffset,
                                componentType = accessor.componentType,
                                typeEnum = accessor.typeEnum
                            };
                            export.m_Writer.m_Accessors.Add(newAccessor);
                        }

                        foreach (var channel in animation.channels)
                        {
                            string sourceNodeName = sourceAnimationGltfRoot.nodes[channel.target.node].name;
                            int nodeIndex = export.m_Writer.m_Nodes.FindIndex(node => node.name == sourceNodeName);
                            channel.target.node = nodeIndex;
                        }

                        foreach (var sampler in animation.samplers)
                        {
                            AddAccessorAndBufferView(sampler.input);
                            sampler.input = export.m_Writer.m_Accessors.Count - 1;
                            AddAccessorAndBufferView(sampler.output);
                            sampler.output = export.m_Writer.m_Accessors.Count - 1;
                        }
                    }
                }

                export.m_Writer.beforeSaveAction = BeforeSaveAction;
            }

            // Async glTF export
            bool success = await export.SaveToFileAndDispose(path);

            if (!success)
            {
                Debug.LogError("Something went wrong exporting a glTF");
            }
            else
            {
                Debug.Log($"Exported didimo to {path}");
            }
        }
    }
}