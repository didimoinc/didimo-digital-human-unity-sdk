using UnityEngine;

namespace Didimo
{
    public class ExpressionsController : MonoBehaviour
    {
        public Animator animator;

        // public AnimatorEventManager animatorEventManager;
        public float        minVideoLength = 1.5f;

        protected string lastExpression = "Neutral";

        public string LastExpression => lastExpression;

        public void PlayNeutral()
        {
            if (animator != null)
                animator.SetTrigger("Neutral");
        }

        public void SetExpression(string expression)
        {
            lastExpression = expression;
            if (animator != null)
                animator.SetTrigger(expression);
        }

        public void Happy(bool enable)
        {
            if (enable)
            {
                SetExpression("Happy");
            }
        }

        public void Angry(bool enable)
        {
            if (enable)
            {
                SetExpression("Angry");
            }
        }

        public void Neutral(bool enable)
        {
            if (enable)
            {
                SetExpression("Neutral");
            }
        }

        public void Sad(bool enable)
        {
            if (enable)
            {
                SetExpression("Sad");
            }
        }

        public void Fear(bool enable)
        {
            if (enable)
            {
                SetExpression("Fear");
            }
        }

        public void Disgust(bool enable)
        {
            if (enable)
            {
                SetExpression("Disgust");
            }
        }

        public void Surprise(bool enable)
        {
            if (enable)
            {
                SetExpression("Surprise");
            }
        }

        public string GetExpressionLoop()
        {
            string lastExpression_perfectLoop = "";
            switch (lastExpression)
            {
                case "Neutral":
                    lastExpression_perfectLoop = "NeutralRecLoop";
                    break;
                case "Happy":
                    lastExpression_perfectLoop = "HappyRecLoop";
                    break;
                case "Sad":
                    lastExpression_perfectLoop = "SadRecLoop";
                    break;
                case "Angry":
                    lastExpression_perfectLoop = "AngryRecLoop";
                    break;
                case "Fear":
                    lastExpression_perfectLoop = "FearRecLoop";
                    break;
                case "Surprise":
                    lastExpression_perfectLoop = "SurpriseRecLoop";
                    break;
                case "Disgust":
                    lastExpression_perfectLoop = "DisgustRecLoop";
                    break;
                default:
                    lastExpression_perfectLoop = lastExpression;
                    break;
            }

            return lastExpression_perfectLoop;
        }

        public string ReplayExpression()
        {
            if (animator != null)
                animator.SetTrigger(lastExpression);
            return lastExpression;
        }

        public string ReplayExpressionLoop()
        {
            string lastExpression_perfectLoop = GetExpressionLoop();
            if (animator != null)
                animator.SetTrigger(lastExpression_perfectLoop);
            return lastExpression_perfectLoop;
        }

        public string ReplayExpressionMiddleLoop()
        {
            string lastExpression_perfectLoop = "";
            switch (lastExpression)
            {
                case "Neutral":
                    lastExpression_perfectLoop = "NeutralRecVideoLoop_MidLoop";
                    break;
                case "Happy":
                    lastExpression_perfectLoop = "HappyRecLoop_MidLoop";
                    break;
                case "Sad":
                    lastExpression_perfectLoop = "SadRecLoop_MidLoop";
                    break;
                case "Angry":
                    lastExpression_perfectLoop = "AngryRecLoop_MidLoop";
                    break;
                case "Fear":
                    lastExpression_perfectLoop = "FearRecLoop_MidLoop";
                    break;
                case "Surprise":
                    lastExpression_perfectLoop = "SurpriseRecLoop_MidLoop";
                    break;
                case "Disgust":
                    lastExpression_perfectLoop = "DisgustRecLoop_MidLoop";
                    break;
                default:
                    lastExpression_perfectLoop = lastExpression;
                    break;
            }

            if (animator != null)
                animator.SetTrigger(lastExpression_perfectLoop);
            return lastExpression_perfectLoop;
        }

        public string ReplayExpressionForVideoStart()
        {
            string lastExpression_perfectLoop = "";
            switch (lastExpression)
            {
                case "Neutral":
                    lastExpression_perfectLoop = "NeutralRecVideoLoop_Start";
                    break;
                case "Happy":
                    lastExpression_perfectLoop = "HappyRecVideoLoop_Start";
                    break;
                case "Sad":
                    lastExpression_perfectLoop = "SadRecVideoLoop_Start";
                    break;
                case "Angry":
                    lastExpression_perfectLoop = "AngryRecVideoLoop_Start";
                    break;
                case "Fear":
                    lastExpression_perfectLoop = "FearRecVideoLoop_Start";
                    break;
                case "Surprise":
                    lastExpression_perfectLoop = "SurpriseRecVideoLoop_Start";
                    break;
                case "Disgust":
                    lastExpression_perfectLoop = "DisgustRecVideoLoop_Start";
                    break;
                default:
                    lastExpression_perfectLoop = lastExpression;
                    break;
            }

            if (animator != null)
                animator.SetTrigger(lastExpression_perfectLoop);
            return lastExpression_perfectLoop;
        }

        public string ReplayExpressionForVideoEnd()
        {
            string lastExpression_perfectLoop = "";
            switch (lastExpression)
            {
                case "Neutral":
                    lastExpression_perfectLoop = "NeutralRecVideoLoop_End";
                    break;
                case "Happy":
                    lastExpression_perfectLoop = "HappyRecVideoLoop_End";
                    break;
                case "Sad":
                    lastExpression_perfectLoop = "SadRecVideoLoop_End";
                    break;
                case "Angry":
                    lastExpression_perfectLoop = "AngryRecVideoLoop_End";
                    break;
                case "Fear":
                    lastExpression_perfectLoop = "FearRecVideoLoop_End";
                    break;
                case "Surprise":
                    lastExpression_perfectLoop = "SurpriseRecVideoLoop_End";
                    break;
                case "Disgust":
                    lastExpression_perfectLoop = "DisgustRecVideoLoop_End";
                    break;
                default:
                    lastExpression_perfectLoop = lastExpression;
                    break;
            }

            if (animator != null)
                animator.SetTrigger(lastExpression_perfectLoop);
            return lastExpression_perfectLoop;
        }

        public string ResetPoseForSpeechVideoStart()
        {
            string lastExpression_perfectLoop = "";
            switch (lastExpression)
            {
                case "Neutral":
                    lastExpression_perfectLoop = "ResetPose";
                    break;
                case "Happy":
                    lastExpression_perfectLoop = "ResetPose_Happy";
                    break;
                case "Sad":
                    lastExpression_perfectLoop = "ResetPose_Sad";
                    break;
                case "Angry":
                    lastExpression_perfectLoop = "ResetPose_Angry";
                    break;
                case "Fear":
                    lastExpression_perfectLoop = "ResetPose_Fear";
                    break;
                case "Surprise":
                    lastExpression_perfectLoop = "ResetPose_Surprise";
                    break;
                case "Disgust":
                    lastExpression_perfectLoop = "ResetPose_Disgust";
                    break;
                default:
                    lastExpression_perfectLoop = lastExpression;
                    break;
            }

            if (animator != null)
                animator.SetTrigger(lastExpression_perfectLoop);
            return lastExpression_perfectLoop;
        }
    }
}