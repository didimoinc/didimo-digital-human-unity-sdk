using UnityEngine;

namespace Didimo
{
    public class FallbackPoseController : DidimoPoseController
    {
        public override ESupportedMovements SupportedMovements => ESupportedMovements.None;

        public override void ResetAll() { }
        public override void ForceUpdateAnimation() { }
    }
}