using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "Mocap Database", menuName = "Didimo/Animation/Mocap Database")]
    public class MocapDatabase : ScriptableObject
    {
        [SerializeField]
        protected TextAsset[] mocapTracks;

        private static List<DidimoAnimation> _mocapTracks;

        public static List<DidimoAnimation> MocapTracks
        {
            get
            {
                if (_mocapTracks == null)
                {
                    _mocapTracks = GenerateMocapTracks();
                }

                return _mocapTracks;
            }
        }

        public static List<DidimoAnimation> GenerateMocapTracks()
        {
            List<DidimoAnimation> expressions = new List<DidimoAnimation>();

            foreach (TextAsset mocapTrack in DidimoResources.MocapDatabase.mocapTracks)
            {
                if (mocapTrack == null)
                {
                    Debug.LogWarning("Cannot deserialize null expression, skipping");
                    continue;
                }

                DidimoAnimation didimoAnimation = JsonConvert.DeserializeObject<DidimoAnimation>(mocapTrack.text);
                if (didimoAnimation == null)
                {
                    Debug.LogWarning("Failed to deserialize animation json in expression database");
                    continue;
                }

                expressions.Add(didimoAnimation);
            }

            return expressions;
        }
    }
}