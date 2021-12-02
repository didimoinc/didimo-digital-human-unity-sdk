using System;
using System.Collections;
using UnityEngine;

namespace Didimo.Core.Examples.MeetADidimo
{
    public class MeetADidimo : MonoBehaviour
    {
        [SerializeField]
        private DidimoComponents didimoComponents;

        private enum AnimationMode
        {
            [InspectorName("TTS")] Tts,
            [InspectorName("ARKit")] Arkit
        }

        [Header("Animations")]
        [SerializeField]
        private AnimationMode animationMode;
        
        [SerializeField]
        private TextAsset textAnimation;
        
        [SerializeField]
        private TextAsset idleARKitAnimation;

        [Header("Text Audio")]
        [SerializeField]
        private AudioClip animationAudioClip;
        

        protected void Start()
        {
            if (didimoComponents == null) didimoComponents = GetComponentInChildren<DidimoComponents>();
        }
        
        public void SpeakDidimo()
        {
            switch (animationMode)
            {
                case AnimationMode.Arkit:
                    SpeakDidimoARKit();
                    break;
                case AnimationMode.Tts:
                    SpeakDidimoTts();
                    break;
                default:
                    throw new Exception($"Invalid Animation Mode {animationMode}");
            }
        }

        public void SpeakDidimoTts()
        {
            Speech.Phrase ttsPhrase = Speech.PhraseBuilder.Build(textAnimation.bytes, animationAudioClip);
            didimoComponents.Speech.Speak(ttsPhrase);
        }
        
        private void SpeakDidimoARKit()
        {
            if (didimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (textAnimation == null)
            {
                Debug.LogWarning("ARKit text not set");
                return;
            }

            string animationID = textAnimation.name;

            if (!AnimationCache.HasAnimation(animationID))
            {
                DidimoAnimation arkitAnimation = DidimoAnimation.FromJSONContent(animationID, textAnimation.text, animationAudioClip);
                AnimationCache.Add(animationID, arkitAnimation);
            }

            didimoComponents.Animator.FadeInAnimation(animationID, 0.5f);
            StartCoroutine(CoroutineFadeOutARKit());
        }

        public void PlayIdle()
        {
            string idleAnimationId = idleARKitAnimation.name;

            if (!AnimationCache.HasAnimation(idleAnimationId))
            {
                DidimoAnimation idleAnimation = DidimoAnimation.FromJSONContent(idleAnimationId, idleARKitAnimation.text);
                idleAnimation.WrapMode = WrapMode.Loop;
                AnimationCache.Add(idleAnimationId, idleAnimation);
            }
            didimoComponents.Animator.PlayAnimation(idleAnimationId);
        }

        public void FadeOutIdle()
        {
            string idleAnimationId = idleARKitAnimation.name;

            if (!AnimationCache.HasAnimation(idleAnimationId))
            {
                DidimoAnimation idleAnimation = DidimoAnimation.FromJSONContent(idleAnimationId, idleARKitAnimation.text);
                AnimationCache.Add(idleAnimationId, idleAnimation);
            }
            didimoComponents.Animator.FadeOutAnimation(idleAnimationId, 0.5f);
        }

        public void FadeInIdle()
        {
            string idleAnimationId = idleARKitAnimation.name;

            if (!AnimationCache.HasAnimation(idleAnimationId))
            {
                DidimoAnimation idleAnimation = DidimoAnimation.FromJSONContent(idleAnimationId, idleARKitAnimation.text);
                idleAnimation.WrapMode = WrapMode.Loop;
                AnimationCache.Add(idleAnimationId, idleAnimation);
            }
            didimoComponents.Animator.FadeInAnimation(idleAnimationId, 0.5f);
        }

        public void FadeOutARKit()
        {
            string animationId = textAnimation.name;

            if (!AnimationCache.HasAnimation(animationId))
            {
                DidimoAnimation arkitAnimation = DidimoAnimation.FromJSONContent(animationId, textAnimation.text);
                AnimationCache.Add(animationId, arkitAnimation);
            }

            didimoComponents.Animator.FadeOutAnimation(animationId, 2f);
            FadeInIdle();
        }

        public void SpeakDidimoFirst()
        {
            PlayIdle();
            StartCoroutine(CoroutineFadeOutIdle());
            StartCoroutine(CoroutineSpeakDidimoARKit());
        }

        public void SpeakDidimoSecond()
        {
            StartCoroutine(CoroutineSpeakDidimo());
        }

        private IEnumerator CoroutineFadeOutIdle()
        {
            yield return new WaitForSeconds(1.25f);
            FadeOutIdle();
        }

        private IEnumerator CoroutineSpeakDidimoARKit()
        {
            yield return new WaitForSeconds(2f);
            SpeakDidimoARKit();
        }

        private IEnumerator CoroutineSpeakDidimo()
        {
            yield return new WaitForSeconds(33f);
            SpeakDidimoTts();
        }

        private IEnumerator CoroutineFadeOutARKit()
        {
            yield return new WaitForSeconds(30f);
            FadeOutARKit();
        }
    }
}
