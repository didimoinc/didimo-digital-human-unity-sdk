using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Example
{
    /// <summary>
    /// Example component to play a facial expression on your didimo.
    /// Should be attached/added to the same object where the DidimoComponents component is.
    /// </summary>
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
        [Tooltip("Facial expression to play")]
        private DidimoExpression expression = DidimoExpression.Happy;

        private string previousExpression;
        
        
        /// <summary>
        /// Play, with fade in, the facial expression the selected.
        /// This method only works in PlayMode.
        /// </summary>
        [Button]
        private void PlaySelectedExpression()
        {
            if (DidimoComponents == null)
            {
                Debug.LogWarning("DidimoComponents not set/found");
                return;
            }

            previousExpression = expression.ToString();
            DidimoComponents.Animator.PlayExpression(previousExpression);
        }

        /// <summary>
        /// Stop, using a fade out, the expression currently playing.
        /// This method only works in PlayMode and if an expression was played.
        /// </summary>
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