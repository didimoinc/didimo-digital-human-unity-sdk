using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using BodyPart = Didimo.Core.Utility.DidimoParts.BodyPart;

namespace Didimo.Builder
{
    [System.Serializable]
    public class DidimoImporterJsonConfig
    {
        public List<MeshProperties> meshList;
        public Dictionary<string, Dictionary<string, int[]>> animationFrames;

        public IReadOnlyDictionary<string, int[]> GetAllPoseClips()
        {
            Dictionary<string, int[]> combinedPoses = new Dictionary<string, int[]>();

            foreach (KeyValuePair<string, Dictionary<string, int[]>> pose in animationFrames)
            {
                foreach (KeyValuePair<string, int[]> poseFrame in pose.Value)
                {
                    combinedPoses.Add($"{pose.Key}_{poseFrame.Key}", poseFrame.Value);
                }
            }

            return combinedPoses;
        }
    }

    [System.Serializable]
    public class MeshProperties
    {
        public string bodyPart;
        public string meshName;
        public string meshId;
        public string color;
        public Dictionary<string, string> textures;
    }
}