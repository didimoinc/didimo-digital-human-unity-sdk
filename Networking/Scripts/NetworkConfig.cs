using System.Collections.Generic;
using System.Linq;
using DigitalSalmon.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Didimo.Networking
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "Didimo/Networking/Network Config")]
    public class NetworkConfig : ScriptableObject
    {
#if UNITY_EDITOR
        private static string DEFAULT_PATH = "Assets/Didimo/Networking/NetworkConfig.asset";
#endif
        public static string DEFAULT_DOWNLOADROOT => Application.temporaryCachePath;
        public static string DEFAULT_BASEURL => "https://api.didimo.co/v3";

        [Header("Network Config")]
        [SerializeField]
        protected string baseURL = DEFAULT_BASEURL;

        [SerializeField]
        protected string apiKey;

        [FormerlySerializedAs("downloadRoot")]
        [SerializeField]
        protected string overrideDownloadRoot;

        private static NetworkConfig _instance;

        public string BaseURL { get => baseURL; set => baseURL = value; }
        public string ApiKey { get => apiKey; set => apiKey = value; }

        public string DownloadRoot
        {
            get => !Application.isEditor || string.IsNullOrEmpty(overrideDownloadRoot) ? DEFAULT_DOWNLOADROOT : overrideDownloadRoot;
            set => overrideDownloadRoot = value;
        }

        [HideInInspector, SerializeField]
        public AccountStatusResponse.NewDidimoFeatures Features = new AccountStatusResponse.NewDidimoFeatures();

        public void RemoveFeatures() { Features = new AccountStatusResponse.NewDidimoFeatures(); }

        void SetFeatures<T>(ref List<T> targetFeatures, List<T> featuresFromApi) where T : AccountStatusResponse.NewDidimoFeatures.Feature
        {
            targetFeatures.RemoveWhere(p => featuresFromApi.All(a => a.FeatureName != p.FeatureName));
            foreach (T availableFeature in featuresFromApi)
            {
                T feature = targetFeatures.FirstOrDefault(p => p.FeatureName == availableFeature.FeatureName);
                if (feature == null)
                {
                    targetFeatures.Add(availableFeature);
                }
            }
        }

        public void SetFeatures(AccountStatusResponse.NewDidimoFeatures availableFeaturesResponse)
        {
            SetFeatures(ref Features.BooleanFeatures, availableFeaturesResponse.BooleanFeatures);
            SetFeatures(ref Features.ToggleFeatures, availableFeaturesResponse.ToggleFeatures);
            SetFeatures(ref Features.MultiOptionFeatures, availableFeaturesResponse.MultiOptionFeatures);
        }

        public List<NewDidimoQuery.ApiFeature> GetFeaturesForApi()
        {
            List<NewDidimoQuery.ApiFeature> result = new List<NewDidimoQuery.ApiFeature>();

            foreach (AccountStatusResponse.NewDidimoFeatures.BooleanFeature feature in Features.BooleanFeatures)
            {
                NewDidimoQuery.ApiFeature apiFeature = new NewDidimoQuery.ApiFeature() {FeatureName = feature.FeatureName, FeatureValue = feature.Enabled ? "true" : "false"};
                result.Add(apiFeature);
            }

            foreach (AccountStatusResponse.NewDidimoFeatures.ToggleFeature feature in Features.ToggleFeatures)
            {
                if (!feature.ToggledOn) continue;

                NewDidimoQuery.ApiFeature apiFeature = new NewDidimoQuery.ApiFeature() {FeatureName = feature.FeatureKey, FeatureValue = feature.FeatureName};
                result.Add(apiFeature);
            }

            foreach (AccountStatusResponse.NewDidimoFeatures.MultiOptionFeature feature in Features.MultiOptionFeatures)
            {
                if (feature.SelectedOption >= feature.Options.Count) continue;
                NewDidimoQuery.ApiFeature apiFeature = new NewDidimoQuery.ApiFeature() {FeatureName = feature.FeatureName, FeatureValue = feature.Options[feature.SelectedOption]};
                result.Add(apiFeature);
            }

            return result;
        }

        private void OnValidate()
        {
            foreach (AccountStatusResponse.NewDidimoFeatures.MultiOptionFeature feature in Features.MultiOptionFeatures)
            {
                if (feature.SelectedOption < 0 || feature.SelectedOption > feature.Options.Count)
                {
                    feature.SelectedOption = 0;
                }
            }
        }

#if UNITY_EDITOR
        public static NetworkConfig CreateDefault()
        {
            NetworkConfig networkConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<NetworkConfig>(DEFAULT_PATH);
            if (!networkConfig)
            {
                networkConfig = ScriptableObject.CreateInstance<NetworkConfig>();
                UnityEditor.AssetDatabase.CreateAsset(networkConfig, NetworkConfig.DEFAULT_PATH);
            }

            return networkConfig;
        }
#endif
    }
}