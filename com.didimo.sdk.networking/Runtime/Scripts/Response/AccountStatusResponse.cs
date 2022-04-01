using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo.Networking
{
    public class AccountStatusResponse : DidimoResponse
    {
        public class AvailableFeaturesResponse
        {
            [JsonProperty("__type")] public string FeatureType { get; private set; }
            [JsonProperty("name")] public string Name { get; private set; }
            [JsonProperty("values")] public List<string> Values { get; private set; }
        }

        [Serializable]
        public class NewDidimoFeatures
        {
            [Serializable]
            public class Feature
            {
                [HideInInspector]
                public string FeatureName;
            }

            [Serializable]
            public class ToggleFeature : Feature
            {
                [HideInInspector]
                public string FeatureKey;

                public bool ToggledOn = true;
            }

            [Serializable]
            public class BooleanFeature : Feature
            {
                public bool Enabled = true;
            }

            [Serializable]
            public class MultiOptionFeature : Feature
            {
                public int SelectedOption;

                public List<string> Options;
            }

            public List<ToggleFeature>      ToggleFeatures      = new List<ToggleFeature>();
            public List<BooleanFeature>     BooleanFeatures     = new List<BooleanFeature>();
            public List<MultiOptionFeature> MultiOptionFeatures = new List<MultiOptionFeature>();
        }

        public readonly NewDidimoFeatures newDidimoFeatures = new NewDidimoFeatures();

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (AvailableFeaturesResponse availableFeature in AvailableFeatures)
            {
                switch (availableFeature.Name)
                {
                    case "export_fbx":
                    {
                        NewDidimoFeatures.ToggleFeature feature = new NewDidimoFeatures.ToggleFeature() {FeatureName = "fbx", FeatureKey = "transfer_formats"};
                        newDidimoFeatures.ToggleFeatures.Add(feature);
                    }
                        break;
                    case "export_gltf":
                    {
                        NewDidimoFeatures.ToggleFeature feature = new NewDidimoFeatures.ToggleFeature() {FeatureName = "gltf", FeatureKey = "transfer_formats"};
                        newDidimoFeatures.ToggleFeatures.Add(feature);
                    }
                        break;
                    case "ar_kit":
                    {
                        NewDidimoFeatures.BooleanFeature feature = new NewDidimoFeatures.BooleanFeature() {FeatureName = "arkit", Enabled = true};
                        newDidimoFeatures.BooleanFeatures.Add(feature);
                    }
                        break;
                    case "max_texture_dim":
                    {
                        int selectedIndex = availableFeature.Values.IndexOf("2048");
                        selectedIndex = Math.Max(0, selectedIndex);
                        NewDidimoFeatures.MultiOptionFeature feature = new NewDidimoFeatures.MultiOptionFeature()
                        {
                            FeatureName = "max_texture_dimension", Options = availableFeature.Values, SelectedOption = selectedIndex
                        };
                        newDidimoFeatures.MultiOptionFeatures.Add(feature);
                    }
                        break;
                    default:
                        if (availableFeature.Values == null)
                        {
                            NewDidimoFeatures.BooleanFeature feature = new NewDidimoFeatures.BooleanFeature() {FeatureName = availableFeature.Name, Enabled = true};
                            newDidimoFeatures.BooleanFeatures.Add(feature);
                        }
                        else
                        {
                            NewDidimoFeatures.MultiOptionFeature feature =
                                new NewDidimoFeatures.MultiOptionFeature() {FeatureName = availableFeature.Name, Options = availableFeature.Values};
                            newDidimoFeatures.MultiOptionFeatures.Add(feature);
                        }

                        break;
                }
            }
        }

        [JsonProperty("available_features")] public List<AvailableFeaturesResponse> AvailableFeatures { get; private set; }
    }
}