using System.Collections;
using System.Collections.Generic;
using Didimo.Inspector;
using UnityEngine;

namespace Didimo.Example
{
    public class DidimoAnimationExampleExpressions : DidimoBehaviour
    {
        private enum DidimoExpression
        {
            Happy,
            Sad,
            Anger,
            Fear,
            Disgust,
            Neutral,
            Surprise,
            None
        }

        [SerializeField]
        private DidimoExpression expression = DidimoExpression.Happy;

        private string previousExpression;

        [Button]
        private void PlaySelectedExpression()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            StopExpression();
            previousExpression = expression.ToString();
            DidimoComponents.Animator.FadeInAnimation(previousExpression);
        }

        [Button]
        private void StopExpression()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            if (previousExpression != null) DidimoComponents.Animator.FadeOutAnimation(previousExpression);
        }
    }
}