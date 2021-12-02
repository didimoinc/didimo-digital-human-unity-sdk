using UnityEngine;

namespace Didimo
{
    /// <summary>
    /// Fallback class of the <c>DidimoPoseController</c> for didimos
    /// that do not support any type of animation.
    /// </summary>
    public class FallbackPoseController : DidimoPoseController
    {
        public override ESupportedMovements SupportedMovements => ESupportedMovements.None;

        public override void ResetAll() { }
        public override void ForceUpdateAnimation() { }
    }
}