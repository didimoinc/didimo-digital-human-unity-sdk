using JsonSubTypes;
using Newtonsoft.Json;

namespace Didimo
{
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(Sentence), "sentence")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(Word), "word")]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(VisemeElement), "viseme")]
    public class TTSElement
    {
        private const float MS_TO_SECONDS = 1000L;

        [JsonProperty("time")]
        public long Time;

        [JsonProperty("value")]
        public string Value;

        [JsonProperty("type")]
        private string Type;

        public float TimeSeconds => Time / MS_TO_SECONDS;

        public override string ToString() => $"[{Type}] ({TimeSeconds}) {Value}";
    }

    public class Sentence : TTSElement
    {
        [JsonProperty("start")]
        public long Start;

        [JsonProperty("end")]
        public long End;
    }

    public class Word : TTSElement
    {
        [JsonProperty("start")]
        public long Start;

        [JsonProperty("end")]
        public long End;
    }

    public class VisemeElement : TTSElement
    {
        public string ValueClean => Value.ToLowerInvariant();
    }
}