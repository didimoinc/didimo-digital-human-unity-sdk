using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Didimo.Timeline
{
    [Serializable]
    public class DidimoAnimatorClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField]
        private DidimoAnimatorBehaviour template = new DidimoAnimatorBehaviour();

        [SerializeField, HideInInspector]
        private TextAsset previousAnimationJson;

        [SerializeField, HideInInspector]
        private double previousDuration = 5;

        public override double duration
        {
            get
            {
                if (template != null && previousAnimationJson != template.animationJson)
                {
                    if (template.animationJson == null || string.IsNullOrEmpty(template.animationJson.text))
                    {
                        previousDuration = 5;
                        previousAnimationJson = template.animationJson;
                    }
                    else
                    {
                        previousAnimationJson = template.animationJson;
                        DidimoAnimation animation = DidimoAnimation.FromJSONContent("default", template.animationJson.text);
                        previousDuration = animation.TotalAnimationTime;
                        //Debug.Log(previousDuration);
                    }
                }

                return previousDuration;
            }
        }

        public ClipCaps clipCaps => ClipCaps.None;

        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go) => ScriptPlayable<DidimoAnimatorBehaviour>.Create(graph, template);
    }
}