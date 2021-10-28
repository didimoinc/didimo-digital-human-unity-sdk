using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo
{
    public class SceneContext : ASingletonBehaviour<SceneContext>
    {
        [SerializeField]
        protected Volume volume;

        public static Volume Volume => Instance.volume;
    }
}