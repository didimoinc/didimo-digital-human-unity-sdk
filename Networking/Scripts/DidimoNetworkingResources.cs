using Didimo.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Networking
{
    public class DidimoNetworkingResources : ScriptableObject
    {
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
                if (_instance == null) _instance = Resources.Load<DidimoNetworkingResources>("DidimoNetworkingResources");
#if UNITY_EDITOR
                if (_instance == null)
                {
                    _instance = DidimoNetworkingResources.CreateInstance<DidimoNetworkingResources>();
                    AssetDatabase.CreateAsset(_instance, "Didimo.Networking/DidimoNetworkingResources.asset");
                }
#endif

                return _instance;
            }
        }
    }
}