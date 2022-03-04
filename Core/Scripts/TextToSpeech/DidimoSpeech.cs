using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Didimo.Core.Utility;

namespace Didimo.Speech
{
    /// <summary>
    /// Class responsible for providing TTS capability to any didimo
    /// through the Amazon's AWS Polly service.
    /// </summary>
    public class DidimoSpeech : DidimoBehaviour
    {
        public const string AMAZON_TTS_SOURCE  = "amazonPolly";

        [SerializeField]
        protected float visemeOffset;

        [SerializeField]
        protected float visemeDuration = 0.25f;
        
        [SerializeField]
        protected float visemeMinAmplitude = 0.8f;

        [SerializeField]
        protected float visemeMaxAmplitude = 1f;
        
        [SerializeField, Tooltip("Percentage of viseme duration that can overlap with other visemes")]
        protected float visemeMaxOverlapRate = 0.5f;

        private AudioSource audioSource;

        private Sequence sequence;

        private List<(VisemeElement viseme, float weight)> currentInterpolatedVisemes;

        public Phrase CurrentPhrase { get; private set; }

        protected AudioSource AudioSource
        {
            get
            {
                if (audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                    if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

                    audioSource.loop = false;
                }

                return audioSource;
            }
        }

        protected void Awake()
        {
            sequence = new Sequence(this);
            currentInterpolatedVisemes = new List<(VisemeElement viseme, float weight)>();
        }

        /// <summary>
        /// Start speaking any generated TTS <c>Phrase</c>. This contains an animation and audio clip.
        /// To generate Phrases, use the <c>PhraseBuilder</c>.
        /// </summary>
        /// <param name="phrase">TTS <c>Phrase</c> to be spoken</param>
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

        /// <summary>
        /// Stop playing the TTS animation and audio clip.
        /// </summary>
        public void StopAnimation()
        {
            AudioSource.Stop();
            sequence.Cancel();
        }

        public bool IsSpeaking => AudioSource.isPlaying;

        /// <summary>
        /// Start the speech routine that is responsible for playing the
        /// audio and the animation in sync.
        /// </summary>
        /// <param name="phrase">TTS <c>Phrase</c> to be spoken</param>
        /// <returns></returns>
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


        /// <summary>
        /// Try to play the correct adequate vises for the corresponding audio time.
        /// </summary>
        /// <param name="time">Audio time to play the visemes.</param>
        /// <returns>True if the time is valid and the visemes were played. False otherwise.</returns>
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

        /// <summary>
        /// Calculate the viseme weights to be played from a <c>Phrase</c> at the given time.
        /// This function clears the <paramref name="result"/> list and replaces it with the new information.
        /// </summary>
        /// <param name="phrase"><c>Phrase</c> object that is being played</param>
        /// <param name="time">Audio time to retrieve the visemes.</param>
        /// <param name="result">List of visemes and weight data to be filled with the new information.</param>
        private void CalculatedInterpolatedVisemes(Phrase phrase, float time,
            ref List<(VisemeElement viseme, float weight)> result)
        {
            result.Clear();
            IReadOnlyList<VisemeElement> visemes = phrase.Visemes;

            for (int i = 0; i < visemes.Count; i++)
            {
                VisemeElement viseme = visemes[i];

                float previousVisemeTime = i > 0 ? visemes[i - 1].TimeSeconds : 0;
                float nextVisemeTime = i < visemes.Count - 1 ? visemes[i + 1].TimeSeconds
                    : visemes[visemes.Count - 1].TimeSeconds + visemeDuration;
                
                
                float possibleFadeInDuration = (viseme.TimeSeconds - previousVisemeTime) * (1 + visemeMaxOverlapRate);
                float possibleFadeOutDuration = (nextVisemeTime - viseme.TimeSeconds) *  (1 + visemeMaxOverlapRate);
                
                float fadeInDuration = i > 0 ?
                    Mathf.Min(visemeDuration / 2f, possibleFadeInDuration)
                    : visemes[0].TimeSeconds;

                float fadeOutDuration = i < visemes.Count - 1 ?
                    Mathf.Min(visemeDuration / 2f, possibleFadeOutDuration)
                    : visemeDuration / 2f;

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

                    // Interpolation function (smooth step)
                    weight = weight * weight * (3f - 2f * weight);
                    
                    
                    // If we don't have the desired time to interpolate the viseme, we have to lower its weight
                    // This will cause the interpolation to be smooth, and the viseme will never reach the full weight
                    float lerpPercentage = Mathf.Min(fadeInDuration, fadeOutDuration) / (visemeDuration / 2f);
                    float lerpMultiplier = visemeMinAmplitude + (visemeMaxAmplitude - visemeMinAmplitude) * lerpPercentage;
                    weight *= lerpMultiplier;
                    

                    if (weight <= 0.0001f)
                    {
                        continue;
                    }

                    result.Add((viseme, weight));
                }
            }
        }

        /// <summary>
        /// Find and play the corresponding viseme animation from the TTS phoneme provided.
        /// </summary>
        /// <param name="phoneme">Phoneme name from the TTS phrase.</param>
        /// <param name="weight">Weight of the viseme</param>
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

        /// <summary>
        /// Tries to map each TTS phoneme name to a viseme name.
        /// </summary>
        /// <param name="phoneme">Name of the phoneme</param>
        /// <param name="visemeName">Mapped name of the corresponding viseme.</param>
        /// <returns>True if the phoneme mapping was found. False otherwise.</returns>
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
