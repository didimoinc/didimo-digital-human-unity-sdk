using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Speech
{
    public static class PhraseBuilder
    {
        public static Phrase Build(List<TTSElement> elements, AudioClip audio) => new Phrase(elements, audio);

        public static Phrase Build(byte[] bytes, AudioClip audio, Action<Phrase> onComplete = null)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogWarning("Cannot load Visemes from empty bytes.");
                return Phrase.Empty;
            }

            string json = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("Cannot load Visemes from bytes. Json did not deserialize properly.");
                return Phrase.Empty;
            }
            
            // Amazon Polly sends a "valid JSON per line". The whole file is not actually a JSON valid format
            // https://docs.aws.amazon.com/polly/latest/dg/speechmarkexamples.html
            List<TTSElement> visemes = json.Split(new[] {"\n", "\r", "\r\n"}, StringSplitOptions.RemoveEmptyEntries).Select(JsonConvert.DeserializeObject<TTSElement>).ToList();
            Phrase phrase = Build(visemes, audio);
            onComplete?.Invoke(phrase);
            return phrase;
        }

        public static async Task<Phrase> Build(byte[] visemeData, string audioFilePath, Action<Phrase> onComplete = null)
        {
            if (string.IsNullOrEmpty(audioFilePath) || !File.Exists(audioFilePath))
            {
                Debug.LogWarning($"Cannot load AudioClip at missing path: {audioFilePath}.");
                return Phrase.Empty;
            }

            Task<AudioClip> clipLoadTask = LoadAudioClipFromPath(audioFilePath);
            await clipLoadTask;

            AudioClip clip = clipLoadTask.Result;
            if (clip == null)
            {
                Debug.LogWarning($"Unable to load AudioClip from path ({audioFilePath}). Is the audio file MPEG format?");
                return Phrase.Empty;
            }

            Phrase phrase = Build(visemeData, clip);
            onComplete?.Invoke(phrase);
            return phrase;
        }

        public static async Task<Phrase> Build(string visemesFilePath, string audioFilePath, Action<Phrase> onComplete = null)
        {
            if (string.IsNullOrEmpty(visemesFilePath) || !File.Exists(visemesFilePath))
            {
                Debug.LogWarning($"Cannot load visemes at missing path: {visemesFilePath}.");
                return Phrase.Empty;
            }

            byte[] visemeData = File.ReadAllBytes(visemesFilePath);
            Task<Phrase> buildTask = Build(visemeData, audioFilePath);
            await buildTask;
            onComplete?.Invoke(buildTask.Result);
            return buildTask.Result;
        }

        public static async Task<AudioClip> LoadAudioClipFromPath(string filePath)
        {
            Uri fileUri = new Uri(filePath);
            UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG);
            DownloadHandlerAudioClip audioDownloadHandler = new DownloadHandlerAudioClip(fileUri, AudioType.MPEG); // Probably not needed.
            webRequest.downloadHandler = audioDownloadHandler;

            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                return audioDownloadHandler.audioClip;
            }
            Debug.LogWarning($"PhraseBuilder: Audio WebRequest loading failed: {webRequest.result.ToString()}. Error: {webRequest.error}");
            return null;
        }
    }
}