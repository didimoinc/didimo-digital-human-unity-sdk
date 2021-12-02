using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Example
{
    /// <summary>
    /// Example component to play an AWS Polly TTS file on your didimo.
    /// Should be attached/added to the same object where the DidimoComponents component is.
    /// </summary>
    public class DidimoAnimationExampleTts : DidimoBehaviour
    {
        [SerializeField]
        [Tooltip("JSON file that contains the TTS data to be played")]
        protected TextAsset ttsData;

        [SerializeField]
        [Tooltip("Audio file that contains the audio to be played along with the animation")]
        protected AudioClip ttsClip;
        
        /// <summary>
        /// Play with sound the TTS animation provided.
        /// This method only works in PlayMode.
        /// </summary>
        [Button]
        private void PlaySelectedTts()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (ttsData == null || ttsClip == null)
            {
                Debug.LogWarning("tts Data/Clip not set");
                return;
            }

            Speech.Phrase tts = Speech.PhraseBuilder.Build(ttsData.bytes, ttsClip);
            DidimoComponents.Speech.Speak(tts);
        }
    }
}