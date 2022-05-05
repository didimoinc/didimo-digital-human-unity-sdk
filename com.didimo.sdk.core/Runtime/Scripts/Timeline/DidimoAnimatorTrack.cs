using UnityEngine.Timeline;

namespace Didimo.Timeline
{
    [TrackColor(.6f, .1f, .2f)]
    [TrackBindingType(typeof(DidimoAnimator))]
    [TrackClipType(typeof(DidimoAnimatorClip))]
    public class DidimoAnimatorTrack : TrackAsset
    {
    }
}