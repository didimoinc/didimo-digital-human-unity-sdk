using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Didimo.Mocap
{
    public class DidimoAnimationJson
    {
        [JsonProperty("source", Required = Required.Always)]
        public string Source = "ARKit";

        [JsonProperty("poses", Required = Required.Always)]
        public Dictionary<string, List<float>> MocapValues;
        
        [JsonProperty("skeleton", Required = Required.Default)]
        public Dictionary<string, List<float[]>> SkeletonValues;

        [JsonProperty("timestamps", Required = Required.Default)]
        public List<float> Timestamps;

        [JsonProperty("fps", Required = Required.Default)]
        public int FPS;

        [JsonProperty("animationLength", Required = Required.Always)]
        public float AnimationLength;
        
        [JsonProperty("frameCount", Required = Required.Always)]
        public int FrameCount;


        public DidimoAnimationJson(Dictionary<string, List<float>> mocapValues, int fps = DidimoAnimation.DEFAULT_FPS, float? animationLength = null, int? frameCount = null)
        {
            MocapValues = mocapValues;
            FPS = fps;
            FrameCount = frameCount ?? MocapValues.Values.Max(e => e.Count);
            AnimationLength = animationLength ??  (float) FrameCount / fps;
        }
        
        public DidimoAnimationJson(Dictionary<string, List<float>> mocapValues, [CanBeNull] Dictionary<string, List<float[]>> skeletonValues, int fps = DidimoAnimation.DEFAULT_FPS, float? animationLength = null, int? frameCount = null)
        {
            MocapValues = mocapValues;
            SkeletonValues = skeletonValues;
            FPS = fps;
            FrameCount = frameCount ?? MocapValues.Values.Max(e => e.Count);
            AnimationLength = animationLength ??  (float) FrameCount / fps;
        }

        public DidimoAnimationJson(Dictionary<string, List<float>> mocapValues, List<float> timestamps, int? fps, float? animationLength = null, int? frameCount = null)
        {
            MocapValues = mocapValues;
            Timestamps = timestamps;
            FPS = fps ?? DidimoAnimation.DEFAULT_FPS;
            FrameCount = frameCount ?? timestamps.Count;
            AnimationLength = animationLength ?? timestamps.Last();
        }
        
        public DidimoAnimationJson(Dictionary<string, List<float>> mocapValues, [CanBeNull] Dictionary<string, List<float[]>> skeletonValues, List<float> timestamps, int? fps, float? animationLength = null, int? frameCount = null)
        {
            MocapValues = mocapValues;
            SkeletonValues = skeletonValues;
            Timestamps = timestamps;
            FPS = fps ?? DidimoAnimation.DEFAULT_FPS;
            FrameCount = frameCount ?? timestamps.Count;
            AnimationLength = animationLength ?? timestamps.Last();
        }

        [JsonConstructor]
        public DidimoAnimationJson(string source, Dictionary<string, List<float>> mocapValues, [CanBeNull] Dictionary<string, List<float[]>> skeletonValues, [CanBeNull] List<float> timestamps, int? fps, float animationLength, int frameCount)
        {
            Source = source;
            MocapValues = mocapValues;
            SkeletonValues = skeletonValues;
            Timestamps = timestamps;
            FPS = fps ?? DidimoAnimation.DEFAULT_FPS;
            AnimationLength = animationLength;
            FrameCount = frameCount;
        }

        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
