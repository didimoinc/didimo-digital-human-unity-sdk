using UnityEngine;

namespace Didimo.Builder
{
    public class Configuration
    {
        public enum EAnimationType
        {
            // None,
            Legacy,
            Generic,
            Humanoid
        }
        public Transform Parent { get; private set; }
        public EAnimationType AnimationType = EAnimationType.Generic;
        public Avatar                       Avatar;

        public static Configuration Default() => new Configuration();
        public static Configuration Default(Transform parent) => new Configuration {Parent = parent};

        public static Configuration WithNewAvatarFromThisModel(Transform parent, bool humanoidAvatar) =>
            new Configuration {Parent = parent, AnimationType = humanoidAvatar ? EAnimationType.Humanoid : EAnimationType.Generic};

        public static Configuration WithAvatar(Transform parent, Avatar avatar) =>
            new Configuration {Parent = parent, AnimationType = avatar.isHuman ? EAnimationType.Humanoid : EAnimationType.Generic, Avatar = avatar};
    }
}