using System.Collections.Generic;

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
        
        public override IReadOnlyList<string> GetAllIncludedPoses() => new List<string>();
        public override bool IsPoseIncluded(string poseName) => false;
    }
}