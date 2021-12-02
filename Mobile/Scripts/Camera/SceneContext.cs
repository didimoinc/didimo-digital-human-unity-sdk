using Didimo.Core.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo
{
    public class SceneContext : ASingletonBehaviour<SceneContext>
    {
#if USING_UNITY_URP
        [SerializeField]
        protected Volume volume;

        public static Volume Volume => Instance.volume;
#endif
    }
}