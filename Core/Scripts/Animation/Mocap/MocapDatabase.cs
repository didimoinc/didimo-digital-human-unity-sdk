using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    //TODO: Do we really need this class? There are no references to it in the project
    // https://git.didimo.co/didimo/unity-sdk/-/issues/5

    //[CreateAssetMenu(fileName = "Mocap Database", menuName = "Didimo/Animation/Mocap Database")]
    public class MocapDatabase : ScriptableObject
    {
        [SerializeField] protected TextAsset[] mocapTracks;

        private static List<DidimoAnimation> mocapTracksCache;

        public static List<DidimoAnimation> MocapTracks
        {
            get
            {
                /*
                if (mocapTracksCache == null)
                {
                    mocapTracksCache = GenerateMocapTracks();
                }
                */
                return mocapTracksCache;
            }
        }
        /*
        public static List<DidimoAnimation> GenerateMocapTracks()
        {
            List<DidimoAnimation> expressions = new List<DidimoAnimation>();

            foreach (TextAsset mocapTrack in mocapTracks)
            {
                if (mocapTrack == null)
                {
                    Debug.LogWarning("Cannot deserialize null expression, skipping");
                    continue;
                }

                DidimoAnimation didimoAnimation
                    = JsonConvert.DeserializeObject<DidimoAnimation>(mocapTrack.text);
                if (didimoAnimation == null)
                {
                    Debug.LogWarning("Failed to deserialize animation json in expression database");
                    continue;
                }

                expressions.Add(didimoAnimation);
            }

            return expressions;
        }
        */
    }
}