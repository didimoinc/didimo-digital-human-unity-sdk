using Newtonsoft.Json;

namespace Didimo.Networking
{
    public class TextToSpeechQueryData
    {
        [JsonProperty("text")] public string Text { get; private set; }

        [JsonProperty("voice")] public string Voice { get; private set; }

        public TextToSpeechQueryData(string text, string voice)
        {
            Text = text;
            Voice = voice;
        }
    }
}