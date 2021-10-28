using System.IO;
using UnityEngine;

namespace Didimo
{
    public static class IOUtility
    {
        public static string SanitisePath(string path)
        {
            const string DATA_PATH = "$dataPath";
            const string PERSISTENTDATA_PATH = "$persistentDataPath";
            const string STREAMINGASSETS_PATH = "$streamingAssets";

            path = path.Replace(DATA_PATH, Application.dataPath);
            path = path.Replace(PERSISTENTDATA_PATH, Application.persistentDataPath);
            path = path.Replace(STREAMINGASSETS_PATH, Application.streamingAssetsPath);

            return Path.GetFullPath(path); // This should clean up things like /../
        }
    }
}