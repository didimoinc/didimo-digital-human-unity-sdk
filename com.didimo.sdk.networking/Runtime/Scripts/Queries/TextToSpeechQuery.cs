namespace Didimo.Networking
{
    public class TextToSpeechQuery : JsonPostQuery<TextToSpeechQueryData, TextToSpeechResponse>
    {
        protected override string URL => $"{base.URL}/speech/tts/wav,json";

        protected override TextToSpeechQueryData Data { get; }

        public TextToSpeechQuery(string text, string voice) { Data = new TextToSpeechQueryData(text, voice); }
    }
}