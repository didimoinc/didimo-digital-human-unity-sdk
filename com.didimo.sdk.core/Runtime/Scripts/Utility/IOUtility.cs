using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Didimo.Core.Utility
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
        
        public static string FullPathToProjectPath(string path)
        {
            int subidx = path.IndexOf("assets",StringComparison.CurrentCultureIgnoreCase);
            if (subidx == -1)
                subidx = path.IndexOf("packages",StringComparison.CurrentCultureIgnoreCase);
            #if FUTURE_TEST_FOR_PACKAGE_CACHE_LOCATION
            if (subidx == -1)
                subidx = path.IndexOf("packagecache",StringComparison.CurrentCultureIgnoreCase);
            #endif
            if (subidx != -1)
                return path.Substring(subidx);
            return path;
        }

        public static string NormalizePath(string path)
        {            
            return path.Replace("\\", "/");
        }
        public static int StringSimilarityScore(string ip, string qs)
        {
            var al = 2;
            var v = 0;
            for (var ii = 0; ii < ip.Length; ++ii)
            {
                var ac = 0;
                var letter = ip[ii];
                if (ii >= qs.Length)
                    return v;
                if (letter == qs[ii])
                    v += al;
                else
                    ac = 0;
                for (var jj = 0; jj < al; ++jj)
                {
                    if ((ii - jj < 0) || (ii + jj > qs.Length - 1))
                        break;
                    else if (letter == qs[ii - jj] || letter == qs[ii + jj])
                    {
                        ac += jj;
                        break;
                    }
                }
                v += ac;
            }
            return v;
        }

    }
}