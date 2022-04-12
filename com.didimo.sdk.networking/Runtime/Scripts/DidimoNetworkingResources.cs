using System.IO;
using Didimo.Core.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Networking
{
    public class DidimoNetworkingResources : ScriptableObject
    {
        private const string ASSET_PATH = "Assets/Didimo/Networking/Resources";
        private const string ASSET_NAME = "DidimoNetworkingResources";
        [SerializeField]
        protected NetworkConfig networkConfig;

        private static DidimoNetworkingResources _instance;

        public static NetworkConfig NetworkConfig
        {
            get { return Instance.networkConfig; }
            set
            {
                bool changed = Instance.networkConfig != value;
                Instance.networkConfig = value;
#if UNITY_EDITOR
                if (!changed) return;
                EditorUtility.SetDirty(Instance);
                AssetDatabase.SaveAssets();
#endif
            }
        }

        public static DidimoNetworkingResources Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<DidimoNetworkingResources>(ASSET_NAME);
#if UNITY_EDITOR
                if (_instance == null)
                {
                    _instance = DidimoNetworkingResources.CreateInstance<DidimoNetworkingResources>();
                    if (!Directory.Exists(ASSET_PATH))
                    {
                        Directory.CreateDirectory(ASSET_PATH);
                    }
                    AssetDatabase.CreateAsset(_instance, $"{ASSET_PATH}/{ASSET_NAME}.asset");
                }
#endif

                return _instance;
            }
        }
    }
}