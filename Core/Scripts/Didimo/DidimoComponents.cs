using Didimo.Builder;
using Didimo.Speech;
using UnityEngine;
using UnityEngine.Serialization;
using Didimo.Core.Deformables;
using Didimo.Core.Utility;

namespace Didimo
{
    /// <summary>
    /// Class that caches references to the main didimo components.
    /// If the component does not exist, it is created and then cached.
    /// </summary>
    public class DidimoComponents : MonoBehaviour
    {
        [FormerlySerializedAs("didimoKey")]
        [FormerlySerializedAs("ID")]
        public string DidimoKey;

        private DidimoAnimator didimoAnimator;
        private DidimoIrisController irisController;
        private DidimoDeformables didimoDeformables;
        private DidimoSpeech didimoSpeech;
        private DidimoPoseController didimoPoseController;
        private DidimoMaterials didimoMaterials;
        private DidimoEyeShadowController didimoEyeShadowController;
        private TextureCache textureCache;
        public TextureCache TextureCache => textureCache ??= new TextureCache();
        public DidimoBuildContext BuildContext { get; set; }

        public DidimoAnimator Animator
        {
            get
            {
                if (didimoAnimator == null)
                {
                    ComponentUtility.GetOrAdd(this, ref didimoAnimator);
                }
                return didimoAnimator;
            }
        }

        public DidimoIrisController IrisController
        {
            get
            {
                if (irisController == null)
                {
                    ComponentUtility.GetOrAdd(this, ref irisController);
                }
                return irisController;
            }
        }
        public DidimoPoseController PoseController
        {
            get
            {
                if (didimoPoseController == null)
                {
                    ComponentUtility
                    .GetOrAdd<DidimoPoseController, FallbackPoseController>(this, ref didimoPoseController);
                }
                return didimoPoseController;
            }
        }

        public DidimoDeformables Deformables
        {
            get
            {
                if (didimoDeformables == null)
                {
                    ComponentUtility.GetOrAdd(this, ref didimoDeformables);
                }
                return didimoDeformables;
            }
        }

        public DidimoSpeech Speech
        {
            get
            {
                if (didimoSpeech == null)
                {
                    ComponentUtility.GetOrAdd(this, ref didimoSpeech);
                }
                return didimoSpeech;
            }
        }

        public DidimoMaterials Materials
        {
            get
            {
                if (didimoMaterials == null)
                {
                    ComponentUtility.GetOrAdd(this, ref didimoMaterials);
                }
                return didimoMaterials;
            }
        }

        public DidimoEyeShadowController EyeShadowController
        {
            get
            {
                if (didimoEyeShadowController == null)
                {
                    ComponentUtility.GetOrAdd(this, ref didimoEyeShadowController);
                }
                return didimoEyeShadowController;
            }
        }

        protected void OnDestroy()
        {
            TextureCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
