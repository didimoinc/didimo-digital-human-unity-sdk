using UnityEngine;
using Didimo;
using Didimo.Speech;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading;

namespace Didimo.Core.Examples.AzureTTSIntegration
{
    public class AzureTTSPlayback : MonoBehaviour
    {
        [SerializeField]
        public DidimoComponents didimoComponents;

        [SerializeField]
        public AudioSource audioSource;

        [Header("API Connection Details")]
        [SerializeField]
        public string yourSubscriptionKey;
        [SerializeField]
        public string yourServiceRegion;

        [Header("Playback Mode Parameters")]
        [SerializeField]
        public PlaybackMode playbackMode;

        [Header("File Mode Parameters")]
        [SerializeField]
        public TextAsset textAnimation;

        [SerializeField]
        public AudioClip animationAudioClip;

        [Header("Stream Mode: SSML Parameters")]
        [SerializeField]
        public string ssmlFile = "Assets/AzureTTSIntegration/Content/ssml.xml";

        [Header("Stream Mode: String Parameters")]
        [SerializeField]
        public string textSample = "That quick beige fox jumped in the air over each thin dog. Look out, I shout, for he's foiled you again, creating chaos.";
        [SerializeField]
        public string voiceName = "en-US-JennyNeural";

        private const int SampleRate = 24000;
        private static SpeechConfig speechConfig;
        private static AudioConfig audioConfig;
        private static SpeechSynthesizer synthesizer;
        private StringBuilder stringBuilder;
        private string message;
        private string newMessage;
        private Dictionary<int, string> visemeMapping;

        public enum PlaybackMode
        {
            [InspectorName("File Mode")] FileMode,
            [InspectorName("Stream Mode: String")] StreamStringMode,
            [InspectorName("Stream Mode: SSML")] StreamSSMLMode,

        }

        void Start()
        {
            if (didimoComponents == null) didimoComponents = GetComponentInChildren<DidimoComponents>();
        }

        public void SetupAzureAPISpeechConfig()
        {
            if (!string.IsNullOrEmpty(yourServiceRegion) && !string.IsNullOrEmpty(yourSubscriptionKey))
            {
                speechConfig = SpeechConfig.FromSubscription(yourSubscriptionKey, yourServiceRegion);
                speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);
                speechConfig.SpeechSynthesisVoiceName = voiceName;
                synthesizer = new SpeechSynthesizer(speechConfig, null);


                synthesizer.SynthesisCanceled += (s, e) =>
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
                    message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
                };
            }
            else
            {
                Debug.LogWarning("Please fill in your API Connection details");
            }
        }

        public async Task Playback()
        {
            if (Application.isPlaying)
            {
                switch (playbackMode)
                {
                    case PlaybackMode.FileMode:
                        PlaybackFile();
                        break;

                    case PlaybackMode.StreamSSMLMode:
                        SetupAzureAPISpeechConfig();
                        await PlaybackStreamSSML();
                        break;

                    case PlaybackMode.StreamStringMode:
                        SetupAzureAPISpeechConfig();
                        await PlaybackStreamString();
                        break;

                    default:
                        throw new Exception($"Invalid TTS Mode {playbackMode}");
                }
            }
            else
            {
                Debug.LogWarning("You need to be in Play Mode to playback the animation.");
            }
        }

        public void PlaybackFile()
        {
            Phrase ttsPhrase = PhraseBuilder.Build(textAnimation.bytes, animationAudioClip);
            didimoComponents.Speech.Speak(ttsPhrase);
        }

        public async Task PlaybackStreamSSML()
        {
            DateTime starTime = DateTime.Now;
            await GenerateSSMLStream(starTime);
        }

        public async Task PlaybackStreamString()
        {
            DateTime starTime = DateTime.Now;
            await GenerateStringStream(starTime);
        }

        public async Task GenerateSSMLStream(DateTime starTime)
        {
            if (!string.IsNullOrEmpty(ssmlFile))
            {
                stringBuilder = new StringBuilder();
                System.EventHandler<SpeechSynthesisVisemeEventArgs> visemeCallback = (s, e) =>
                {
                    string map = AzureTTSVisemeMapping.VisemeMapping[(int)e.VisemeId];
                    stringBuilder.AppendLine($"{{\"time\":{e.AudioOffset / 10000},\"type\":\"viseme\",\"value\":\"{map}\"}}");
                };

                synthesizer.VisemeReceived += visemeCallback;

                using (SpeechSynthesisResult result = await synthesizer.StartSpeakingSsmlAsync(File.ReadAllText(ssmlFile)))
                {
                    AudioDataStream audioDataStream = AudioDataStream.FromResult(result);
                    bool isFirstAudioChunk = true;

                    AudioClip audioClip = AudioClip.Create("Speech", SampleRate * 600, 1, SampleRate, true, (float[] audioChunk) =>
                    {
                        int chunkSize = audioChunk.Length;
                        byte[] audioChunkBytes = new byte[chunkSize * 2];
                        uint readBytes = audioDataStream.ReadData(audioChunkBytes);

                        if (isFirstAudioChunk && readBytes > 0)
                        {
                            DateTime endTime = DateTime.Now;
                            double latency = endTime.Subtract(starTime).TotalMilliseconds;
                            newMessage = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                            isFirstAudioChunk = false;
                        }

                        for (int i = 0; i < chunkSize; ++i)
                        {
                            if (i < readBytes / 2)
                            {
                                audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                            }
                            else
                            {
                                audioChunk[i] = 0.0f;
                            }
                        }

                        if (readBytes == 0)
                        {
                            Thread.Sleep(200); // Leave some time for the audioSource to finish playback
                        }
                    });

                    byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString());

                    Phrase ttsPhrase = PhraseBuilder.Build(textBytes, audioClip);
                    didimoComponents.Speech.Speak(ttsPhrase);
                }
                synthesizer.VisemeReceived += visemeCallback;
            }
            else
            {
                Debug.Log("Please add SSML file!");
            }
        }

        public async Task GenerateStringStream(DateTime starTime)
        {
            if (!string.IsNullOrEmpty(textSample))
            {
                stringBuilder = new StringBuilder();
                System.EventHandler<SpeechSynthesisVisemeEventArgs> visemeCallback = (s, e) =>
                {
                    string map = AzureTTSVisemeMapping.VisemeMapping[(int)e.VisemeId];
                    stringBuilder.AppendLine($"{{\"time\":{e.AudioOffset / 10000},\"type\":\"viseme\",\"value\":\"{map}\"}}");
                };

                synthesizer.VisemeReceived += visemeCallback;

                using (SpeechSynthesisResult result = await synthesizer.StartSpeakingTextAsync(textSample))
                {
                    AudioDataStream audioDataStream = AudioDataStream.FromResult(result);
                    bool isFirstAudioChunk = true;

                    AudioClip audioClip = AudioClip.Create("Speech", SampleRate * 600, 1, SampleRate, true, (float[] audioChunk) =>
                    {
                        int chunkSize = audioChunk.Length;
                        byte[] audioChunkBytes = new byte[chunkSize * 2];
                        uint readBytes = audioDataStream.ReadData(audioChunkBytes);

                        if (isFirstAudioChunk && readBytes > 0)
                        {
                            DateTime endTime = DateTime.Now;
                            double latency = endTime.Subtract(starTime).TotalMilliseconds;
                            newMessage = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                            isFirstAudioChunk = false;
                        }

                        for (int i = 0; i < chunkSize; ++i)
                        {
                            if (i < readBytes / 2)
                            {
                                audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                            }
                            else
                            {
                                audioChunk[i] = 0.0f;
                            }
                        }

                        if (readBytes == 0)
                        {
                            Thread.Sleep(200); // Leave some time for the audioSource to finish playback
                        }
                    });

                    byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString());
                    Debug.Log(stringBuilder.ToString());

                    Phrase ttsPhrase = PhraseBuilder.Build(textBytes, audioClip);
                    didimoComponents.Speech.Speak(ttsPhrase);
                }
                synthesizer.VisemeReceived -= visemeCallback;
            }
            else
            {
                Debug.Log("Please write some text on the Inspector!");
            }
        }
    }
}