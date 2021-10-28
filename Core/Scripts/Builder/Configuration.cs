using UnityEngine;

namespace Didimo.Builder
{
    public class Configuration
    {
        public Transform Parent { get; private set; }

        public static Configuration Default() => new Configuration();
        public static Configuration Default(Transform parent) => new Configuration {Parent = parent};
    }
}