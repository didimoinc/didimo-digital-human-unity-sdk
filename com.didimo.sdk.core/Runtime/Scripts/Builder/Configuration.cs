using Didimo.GLTFUtility;
using UnityEngine;

namespace Didimo.Builder
{
    public class Configuration
    {
        public Transform Parent { get; private set; }
        public ImportSettings.AnimationType AnimationType = ImportSettings.AnimationType.Generic;
        public Avatar                       Avatar;

        public static Configuration Default() => new Configuration();
        public static Configuration Default(Transform parent) => new Configuration {Parent = parent};

        public static Configuration WithNewAvatarFromThisModel(Transform parent, bool humanoidAvatar) =>
            new Configuration {Parent = parent, AnimationType = humanoidAvatar ? ImportSettings.AnimationType.Humanoid : ImportSettings.AnimationType.Generic};

        public static Configuration WithAvatar(Transform parent, Avatar avatar) =>
            new Configuration {Parent = parent, AnimationType = avatar.isHuman ? ImportSettings.AnimationType.Humanoid : ImportSettings.AnimationType.Generic, Avatar = avatar};
    }
}