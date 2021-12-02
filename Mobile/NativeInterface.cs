using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Didimo.Builder;
using Didimo.Builder.JSON;
using Didimo;
using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;
using Didimo.Core.Deformables;
using Didimo.Core.Utility;

namespace Didimo
{
    public class NativeInterface : ASingletonBehaviour<NativeInterface>
    {
        [JsonConverter(typeof(JsonSubtypes), "methodName")]
        [JsonSubtypes.FallBackSubTypeAttribute(typeof(UnsupportedMethodCall))]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(CacheAnimationCall), "cacheAnimation")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(BuildDidimoFromDirectoryCall), "buildDidimoFromDirectory")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(UpdateDeformable), "updateDeformable")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(SetCameraCall), "setCamera")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(SetHairStyleCall), "setHairStyle")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(SetHairColorCall), "setHairColor")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(SetEyeColorCall), "setEyeColor")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(DestroyDidimoCall), "destroyDidimo")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(SetOrbitControlsCall), "setOrbitControls")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(ClearCacheCall), "clearCache")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(PlayExpression), "playExpression")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(TextToSpeechCall), "textToSpeech")]
        [JsonSubtypes.KnownSubTypeAttribute(typeof(ResetCameraCall), "resetCamera")]
        public class NativeMethodCall
        {
            [JsonProperty("methodName")] public string MethodName { get; private set; }

            public virtual void CallSync() { }
        }

        public class AsyncNativeMethodCall : NativeMethodCall
        {
#pragma warning disable 1998
            public virtual async Task CallAsync() { }
#pragma warning restore 1998
        }

        public class UnsupportedMethodCall : NativeMethodCall
        {
            public override void CallSync()
            {
                base.CallSync();
                Debug.LogWarning($"You are attempting to call a method which is not supported. ({MethodName})");
            }
        }

        public class CacheAnimationCall : NativeMethodCall
        {
            [JsonProperty("id")] public string ID { get; private set; }
            [JsonProperty("filePath")] public string FilePath { get; private set; }

            public override void CallSync() { AnimationCache.Add(ID, FilePath); }
        }

        public class BuildDidimoFromDirectoryCall : AsyncNativeMethodCall
        {
            [JsonProperty("directory")] public string Directory { get; private set; }
            [JsonProperty("type")] public string Type { get; private set; }

            public override async Task CallAsync()
            {

                await DidimoLoader.LoadDidimoInFolder(Path.GetFileNameWithoutExtension(Directory), Directory!);
            }

            public static BuildDidimoFromDirectoryCall FromDirectory(string directory) => new BuildDidimoFromDirectoryCall {Directory = directory};
        }

        public class UpdateDeformable : NativeMethodCall
        {
            [JsonProperty("didimoID")] public string DidimoID { get; private set; }
            [JsonProperty("deformableId")] public string DeformableID { get; private set; }
            [JsonProperty("deformedData")] public byte[] DeformedData { get; private set; }

            public override void CallSync()
            {
                base.CallSync();
                if (!DidimoCache.TryFindDidimo(DidimoID, out DidimoComponents didimo))
                {
                    Debug.LogWarning($"Could not find didimo with ID: {DidimoID}");
                    return;
                }

                if (!didimo.Deformables.TryFind(DeformableID, out Deformable deformable))
                {
                    Debug.LogWarning($"Could not find deformable with ID: {DeformableID}");
                    return;
                }

                deformable.SetDeformedMeshData(DeformedData);
            }
        }

        public class SetCameraCall : NativeMethodCall
        {
            [JsonProperty("position")]
            protected float[] position;

            [JsonProperty("rotation")]
            protected float[] rotation;

            [JsonProperty("fieldOfView")]
            protected float fieldOfView;

            public Vector3 Position => new Vector3(position[0], position[1], position[2]);
            public Quaternion Rotation => new Quaternion(rotation[0], rotation[1], rotation[2], rotation[3]);

            public override void CallSync()
            {
                Camera cam = Camera.main;
                if (cam == null)
                {
                    Debug.LogWarning("Failed to SetCamera as no valid camera was found.");
                    return;
                }

                cam.transform.position = Position;
                cam.transform.rotation = Rotation;
                cam.fieldOfView = fieldOfView;
            }
        }

        public class SetHairStyleCall : NativeMethodCall
        {
            [JsonProperty("didimoID")]
            protected string didimoID;

            [JsonProperty("styleID")]
            protected string styleID;

            public string DidimoID => didimoID;
            public string StyleID => styleID;

            public override void CallSync()
            {
                if (DidimoCache.TryFindDidimo(DidimoID, out DidimoComponents didimo))
                {
                    didimo.Deformables.TryCreate<Hair>(StyleID, out _);
                }
            }

            public static SetHairStyleCall FromIDs(string didimoID, string styleID) => new SetHairStyleCall {didimoID = didimoID, styleID = styleID};
        }

        public class SetHairColorCall : NativeMethodCall
        {
            [JsonProperty("didimoID")]
            protected string didimoID;

            [JsonProperty("color")]
            protected float[] color;

            public string DidimoID => didimoID;
            public Color Color => new Color(color[0], color[1], color[2], color[3]);

            public override void CallSync()
            {
                if (DidimoCache.TryFindDidimo(DidimoID, out DidimoComponents didimo))
                {
                    if (didimo.Deformables.TryFind(out Hair hair))
                    {
                        hair.Color = Color;
                    }
                }
            }

            public static SetHairColorCall FromColor(string didimoID, Color color) => new SetHairColorCall {didimoID = didimoID, color = new float[4] {color.r, color.g, color.b, color.a}};
        }

        public class SetEyeColorCall : NativeMethodCall
        {
            [JsonProperty("didimoID")]
            protected string didimoID;

            [JsonProperty("color")]
            protected float[] color;

            public string DidimoID => didimoID;
            public Color Color => new Color(color[0], color[1], color[2], color[3]);

            public override void CallSync()
            {
                if (DidimoCache.TryFindDidimo(DidimoID, out DidimoComponents didimo))
                {
                    didimo.Materials.OverrideColor(DidimoMaterials.DidimoColorPropertyOverride.EyeColor, Color);
                }
            }

            public static SetEyeColorCall FromColor(string didimoID, Color color) => new SetEyeColorCall {didimoID = didimoID, color = new[] {color.r, color.g, color.b, color.a}};
        }

        public class DestroyDidimoCall : NativeMethodCall
        {
            [JsonProperty("didimoID")]
            protected string didimoID;

            public string DidimoID => didimoID;

            public static DestroyDidimoCall DestroyAll => new DestroyDidimoCall {didimoID = DidimoCache.ALL_ID};

            public override void CallSync() { DidimoCache.TryDestroy(DidimoID); }
        }

        public class SetOrbitControlsCall : NativeMethodCall
        {
            [JsonProperty("enabled")]
            protected bool enabled;

            public bool Enabled => enabled;

            public static SetOrbitControlsCall DefaultEnabled => new SetOrbitControlsCall {enabled = true};

            public override void CallSync()
            {
                Camera cam = Camera.main;
                if (!cam) return;
                DragOrbit dragOrbit = cam.GetComponent<DragOrbit>();
                if (enabled && dragOrbit == null) dragOrbit = cam.gameObject.AddComponent<DragOrbit>();
                if (!enabled && dragOrbit != null) Destroy(dragOrbit);
            }
        }

        public class ClearCacheCall : NativeMethodCall
        {
            public override void CallSync() { AnimationCache.Clear(); }
        }

        public class PlayExpression : NativeMethodCall
        {
            [JsonProperty("didimoID")]
            protected string didimoID;

            [JsonProperty("animationID")]
            protected string animationID;

            public override void CallSync()
            {
                if (DidimoCache.TryFindDidimo(didimoID, out DidimoComponents didimo))
                {
                    didimo.Animator.PlayExpression(animationID);
                }
                else
                {
                    Debug.LogWarning($"Failed to locate didimo with ID {didimoID}");
                }
            }

            public static PlayExpression FromIDs(string didimoID, string animationID) => new PlayExpression {didimoID = didimoID, animationID = animationID};
        }

        public class TextToSpeechCall : AsyncNativeMethodCall
        {
            [JsonProperty("didimoID")]
            protected string didimoID;

            [JsonProperty("dataPath")]
            protected string dataPath;

            [JsonProperty("clipPath")]
            protected string clipPath;

            public string DidimoID => didimoID;
            public string DataPath => dataPath;
            public string ClipPath => clipPath;

            public override async Task CallAsync()
            {
                if (DidimoCache.TryFindDidimo(DidimoID, out DidimoComponents didimo))
                {
                    await Speech.PhraseBuilder.Build(dataPath, clipPath, (phrase) => ThreadingUtility.WhenMainThread(() => didimo.Speech.Speak(phrase)));
                }
            }

            public static TextToSpeechCall FromPaths(string didimoID, string dataPath, string clipPath) => new TextToSpeechCall {didimoID = didimoID, dataPath = dataPath, clipPath = clipPath};
        }

        public class ResetCameraCall : NativeMethodCall
        {
            [JsonProperty("instant")]
            protected bool instant;

            public static ResetCameraCall Instant => new ResetCameraCall {instant = true};
            public static ResetCameraCall NonInstant => new ResetCameraCall {instant = false};

            public override void CallSync()
            {
                Camera cam = Camera.main;
                if (!cam) return;
                DragOrbit dragOrbit = cam.GetComponent<DragOrbit>();
                if (dragOrbit == null) return;
                dragOrbit.ResetView(instant);
            }
        }

        private static readonly Queue<NativeMethodCall>          methodQueue  = new Queue<NativeMethodCall>();
        private static readonly HashSet<ConfiguredTaskAwaitable> asyncMethods = new HashSet<ConfiguredTaskAwaitable>();

        protected void Awake()
        {
            Sequence seq = new Sequence(this);
            seq.Coroutine(StackRoutine());
        }

        public void CallMethod(string json)
        {
            NativeMethodCall call = JsonConvert.DeserializeObject<NativeMethodCall>(json);
            EnqueueMethodCall(call);
        }

        public static void EnqueueMethodCall(NativeMethodCall methodCall) { methodQueue.Enqueue(methodCall); }

        private IEnumerator StackRoutine()
        {
            while (true)
            {
                while (methodQueue.Any())
                {
                    NativeMethodCall callObject = methodQueue.Dequeue();

                    // Push the async method to the async collection, so we can make sure all async
                    // is finished before running any sync methods.
                    if (callObject is AsyncNativeMethodCall asyncCallObject)
                    {
                        try
                        {
                            ConfiguredTaskAwaitable task = asyncCallObject.CallAsync().ConfigureAwait(false);
                            asyncMethods.Add(task);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex);
                        }
                    }
                    else
                    {
                        if (asyncMethods.Any())
                        {
                            // Wait until all async are finished.
                            while (!asyncMethods.All(t => t.GetAwaiter().IsCompleted))
                            {
                                yield return null;
                            }

                            asyncMethods.Clear();
                            yield return null;
                        }

                        // Run sync.
                        callObject.CallSync();
                    }
                }

                yield return null;
            }
        }
    }
}
