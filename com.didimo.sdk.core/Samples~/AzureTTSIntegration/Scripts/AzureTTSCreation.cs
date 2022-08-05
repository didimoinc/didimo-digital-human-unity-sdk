using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.Core.Examples.AzureTTSIntegration
{
    public class AzureTTSCreation : MonoBehaviour
    {
        [Header("API Connection Details")]
        [SerializeField]
        public string yourSubscriptionKey;
        [SerializeField]
        public string yourServiceRegion;

        [Header("Creation Mode")]
        [SerializeField]
        public TTSMode ttsCreationMode;

        [Header("SSML Mode Parameters")]
        [SerializeField]
        public string ssmlFile = "Assets/AzureTTSIntegration/Content/ssml.xml";

        [Header("String Mode Parameters")]
        [SerializeField]
        public string textSample = "That quick beige fox jumped in the air over each thin dog. Look out, I shout, for he's foiled you again, creating chaos.";
        [SerializeField]
        public string voiceName = "en-US-JennyNeural";

        [Header("File Info")]
        [SerializeField]
        public string fileName = "azureTTS";
        [SerializeField]
        public string audioFilePath = "Assets/AzureTTSIntegration/Audio";
        [SerializeField]
        public string animationFilePath = "Assets/AzureTTSIntegration/Animations";

        private Dictionary<int, string> visemeMapping;
        private const int SampleRate = 24000;
        private static SpeechConfig speechConfig;
        private static AudioConfig audioConfig;
        private static SpeechSynthesizer synthesizer;
        private StringBuilder stringBuilder;
        private string message;
        private string newMessage;
        public enum TTSMode
        {
            [InspectorName("Create From String")] CreateFromString,
            [InspectorName("Create From SSML")] CreateFromSSML,
        }

        public async Task CreateTTSFiles()
        {
            speechConfig = SpeechConfig.FromSubscription(yourSubscriptionKey, yourServiceRegion);
            speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio48Khz192KBitRateMonoMp3);
            speechConfig.SpeechSynthesisVoiceName = voiceName;

            switch (ttsCreationMode)
            {
                case TTSMode.CreateFromString:
                    await CreateStringSpeech();
                    break;

                case TTSMode.CreateFromSSML:
                    await CreateSSMLSpeech();
                    break;

                default:
                    throw new Exception($"Invalid TTS Mode {ttsCreationMode}");
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
            string uniqueFilePathAnim = AssetDatabase.GenerateUniqueAssetPath($"{animationFilePath}/{fileName}.json");
            File.WriteAllText(uniqueFilePathAnim, stringBuilder.ToString());
            stringBuilder.Clear();
            AssetDatabase.ImportAsset(uniqueFilePathAnim);
#endif
        }

        public async Task CreateSSMLSpeech()
        {
            if (!string.IsNullOrEmpty(ssmlFile))
            {
#if UNITY_EDITOR
                string uniqueFilePathAudio = AssetDatabase.GenerateUniqueAssetPath($"{audioFilePath}/{fileName}.mp3");
#else
                string uniqueFilePathAudio = $"{audioFilePath}/{fileName}.mp3";
#endif
                using var audioConfig = AudioConfig.FromWavFileOutput(uniqueFilePathAudio);
                using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

                synthesizer.SynthesisCanceled += (s, e) =>
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
                    message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
                };

                stringBuilder = new StringBuilder();

                synthesizer.VisemeReceived += (s, e) =>
                {
                    string map = AzureTTSVisemeMapping.VisemeMapping[(int)e.VisemeId];
                    stringBuilder.AppendLine($"{{\"time\":{e.AudioOffset / 10000},\"type\":\"viseme\",\"value\":\"{map}\"}}");
                };

                await synthesizer.SpeakSsmlAsync(File.ReadAllText(ssmlFile));
            }
            else
            {
                Debug.Log("Please add SSML file path!");
            }
        }

        public async Task CreateStringSpeech()
        {
            if (!string.IsNullOrEmpty(textSample))
            {
#if UNITY_EDITOR
                string uniqueFilePathAudio = AssetDatabase.GenerateUniqueAssetPath($"{audioFilePath}/{fileName}.mp3");
#else
                string uniqueFilePathAudio = $"{audioFilePath}/{fileName}.mp3";
#endif
                using var audioConfig = AudioConfig.FromWavFileOutput(uniqueFilePathAudio);
                using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

                synthesizer.SynthesisCanceled += (s, e) =>
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
                    message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
                };

                stringBuilder = new StringBuilder();
                synthesizer.VisemeReceived += (s, e) =>
                {
                    string map = AzureTTSVisemeMapping.VisemeMapping[(int)e.VisemeId];
                    stringBuilder.AppendLine($"{{\"time\":{e.AudioOffset / 10000},\"type\":\"viseme\",\"value\":\"{map}\"}}");
                };

                await synthesizer.SpeakTextAsync(textSample);
            }
            else
            {
                Debug.Log("Please write some text in the Inspector!");
            }
        }
    }


}
