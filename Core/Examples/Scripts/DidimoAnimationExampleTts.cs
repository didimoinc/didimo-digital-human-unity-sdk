using System.Collections;
using System.Collections.Generic;
using Didimo.Inspector;
using UnityEngine;

namespace Didimo.Example
{
    public class DidimoAnimationExampleTts : DidimoBehaviour
    {
        [SerializeField]
        [InspectorName("TTS Data")]
        protected TextAsset ttsData;

        [SerializeField]
        [InspectorName("TTS Audio Clip")]
        protected AudioClip ttsClip;
        
        
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