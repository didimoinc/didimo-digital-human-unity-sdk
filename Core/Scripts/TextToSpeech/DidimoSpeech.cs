using System.Collections;
using System.Collections.Generic;
using DigitalSalmon;
using UnityEngine;

namespace Didimo.Speech
{
    public class DidimoSpeech : DidimoBehaviour
    {
        public const string AMAZON_TTS_SOURCE  = "amazonPolly";

        [SerializeField]
        protected float visemeOffset;

        [SerializeField]
        protected float visemeDuration = 0.3f;

        [SerializeField]
        protected float visemeMaxAmplitude = 1f;

        private AudioSource _audioSource;

        private Sequence sequence;

        private List<(VisemeElement viseme, float weight)> currentInterpolatedVisemes;

        public Phrase CurrentPhrase { get; private set; }

        protected AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                {
                    _audioSource = GetComponent<AudioSource>();
                    if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

                    _audioSource.loop = false;
                }

                return _audioSource;
            }
        }

        protected void Awake()
        {
            sequence = new Sequence(this);
            currentInterpolatedVisemes = new List<(VisemeElement viseme, float weight)>();
        }

        public void Speak(Phrase phrase)
        {
            if (!DidimoComponents.PoseController.SupportsAnimation)
            {
                Debug.LogWarning("Attempting to speak phrase but PoseController does not support animation.");
            }

            if (!phrase.IsValid)
            {
                Debug.LogWarning("Skipping invalid phrase.");
                return;
            }

            CurrentPhrase = phrase;

            sequence.Cancel();
            sequence.Coroutine(SpeechRoutine(phrase));
        }

        public void StopAnimation()
        {
            AudioSource.Stop();
            sequence.Cancel();
        }
        
        public bool IsSpeaking => AudioSource.isPlaying; 

        private IEnumerator SpeechRoutine(Phrase phrase)
        {
            AudioSource.clip = phrase.Audio;
            AudioSource.Play();

            while (AudioSource.isPlaying)
            {
                if (!TryEvaluateTime(AudioSource.time))
                {
                    Debug.LogWarning("Failed to evaluate time.");
                    break;
                }

                yield return null;
            }

            TryEvaluateTime(0);
            CurrentPhrase = null;
            AudioSource.clip = null;
            AudioSource.Stop();
        }

        private bool TryEvaluateTime(float time)
        {
            if (CurrentPhrase == null) return false;
            if (AudioSource.clip == null || time > AudioSource.clip.length)
            {
                return false;
            }

            CalculatedInterpolatedVisemes(CurrentPhrase, time + visemeOffset, ref currentInterpolatedVisemes);

            //DidimoComponents.PoseController.ResetAll();
            foreach ((VisemeElement viseme, float weight) interpolatedViseme in currentInterpolatedVisemes)
            {
                PlayMatchingVisemeFromPhoneme(interpolatedViseme.viseme.ValueClean, interpolatedViseme.weight);
            }

            return true;
        }

        private void CalculatedInterpolatedVisemes(Phrase phrase, float time, ref List<(VisemeElement viseme, float weight)> result)
        {
            result.Clear();
            IReadOnlyList<VisemeElement> visemes = phrase.Visemes;

            for (int i = 0; i < visemes.Count; i++)
            {
                VisemeElement viseme = visemes[i];

                float previousVisemeTime = i > 0 ? visemes[i - 1].TimeSeconds : 0;
                float nextVisemeTime = i < visemes.Count - 1 ? visemes[i + 1].TimeSeconds : visemes[visemes.Count - 1].TimeSeconds + visemeDuration;

                float fadeInDuration = visemeDuration / 2f;
                if (viseme.TimeSeconds - previousVisemeTime < visemeDuration / 2f)
                {
                    fadeInDuration = viseme.TimeSeconds - previousVisemeTime;
                }

                float fadeOutDuration = visemeDuration / 2f;
                if (nextVisemeTime - viseme.TimeSeconds < visemeDuration / 2f)
                {
                    fadeOutDuration = nextVisemeTime - viseme.TimeSeconds;
                }

                float minStartTime = viseme.TimeSeconds - fadeInDuration;
                float maxEndTime = viseme.TimeSeconds + fadeOutDuration;

                if (time >= minStartTime && time <= maxEndTime)
                {
                    float weight;

                    // Fade in
                    if (time < viseme.TimeSeconds)
                    {
                        weight = (time - minStartTime) / fadeInDuration;
                    } // Fade out
                    else
                    {
                        weight = (maxEndTime - time) / fadeOutDuration;
                    }

                    // Interpolation function ( smooth step)
                    weight = weight * weight * (3f - 2f * weight);

                    weight *= visemeMaxAmplitude;

                    // If we don't have the desired time to interpolate the viseme, we have to lower its weight
                    // This will cause the interpolation to be smooth, and the viseme will never reach the full weight
                    weight *= Mathf.Min(fadeInDuration, fadeOutDuration) / (visemeDuration / 2f);

                    if (weight <= 0.0001f)
                    {
                        continue;
                    }

                    result.Add((viseme, weight));
                }
            }

            if (result.Count > 2)
            {
                Debug.LogWarning($"Found more than two visemes for time {time} ({result.Count}). This shouldn't happen.");
            }
        }

        private void PlayMatchingVisemeFromPhoneme(string phoneme, float weight)
        {
            if (TryFindVisemeName(phoneme, out string visemeName))
            {
                if (!DidimoComponents.PoseController.SetWeightForPose(AMAZON_TTS_SOURCE, visemeName, weight))
                {
                    Debug.LogWarning($"Failed to set blend shape weight for: {visemeName}");
                }
            }
            else if (phoneme == Constants.PHONEME_SILENCE)
            {
                // Clear Pose values?
            }
            else
            {
                Debug.LogWarning($"Failed to find viseme for phoneme: {phoneme}");
            }
        }

        private static bool TryFindVisemeName(string phoneme, out string visemeName)
        {
            switch (phoneme)
            {
                case "p":
                    visemeName = Constants.p_b_m;
                    return true;
                case "t":
                case "TT":
                    visemeName = Constants.d_t_n;
                    return true;
                case "s":
                case "SS":
                    visemeName = Constants.s_z;
                    return true;
                case "f":
                    visemeName = Constants.f_v;
                    return true;
                case "k":
                    visemeName = Constants.k_g_ng;
                    return true;
                case "i":
                    visemeName = Constants.ay;
                    return true;
                case "r":
                    visemeName = Constants.r;
                    return true;
                case "e":
                case "u":
                    visemeName = Constants.ey_eh_uh;
                    return true;
                case "&":
                case "@":
                    visemeName = Constants.aa;
                    return true;
                case "a":
                    visemeName = Constants.ae_ax_ah;
                    return true;
                case "o":
                case "EE":
                    visemeName = Constants.ao;
                    return true;
                case "OO":
                    visemeName = Constants.aw;
                    return true;
            }

            visemeName = "";
            return false;
        }
    }
}