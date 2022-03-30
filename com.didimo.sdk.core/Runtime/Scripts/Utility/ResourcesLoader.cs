using System.IO;
using System.Linq;
using Didimo.Core.Config;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Utility
{
    public class ResourcesLoader
    {
        public static ShaderResources ShaderResources()
        {
            var shaderResources = Resources
                                                   .Load<ShaderResources>("ShaderResources");
                                   
                                               // Might be required for the first time the project is loaded
                                   #if UNITY_EDITOR
                                               if (shaderResources == null)
                                               {
                                                   string path = Directory
                                                       .GetFiles("Packages/com.didimo.sdk.core", "ShaderResources.asset", SearchOption.AllDirectories)
                                                       .FirstOrDefault();
                                                   if (path != null)
                                                   {
                                                       shaderResources = AssetDatabase.LoadAssetAtPath<ShaderResources>(path);
                                                   }
                                               }
                                   #endif
                                   
                                               return shaderResources;
        }

        public static Avatar DidimoDefaultAvatar()
        {
            var didimoDefaultAvatar = Resources
                .Load<Avatar>("DidimoDefaultAvatar");
            // Might be required for the first time the project is loaded
#if UNITY_EDITOR
            if (didimoDefaultAvatar == null)
            {
                string path = Directory
                              .GetFiles("Packages/com.didimo.sdk.core", "DidimoDefaultAvatar.asset", SearchOption.AllDirectories)
                              .FirstOrDefault();
                if (path != null)
                {
                    didimoDefaultAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
                }
            }
#endif
                                   
            return didimoDefaultAvatar;
        }
    }
}