using Didimo.Builder;
using Didimo.Speech;
using UnityEngine;
using UnityEngine.Serialization;

namespace Didimo
{
    public class DidimoComponents : MonoBehaviour
    {
        [FormerlySerializedAs("didimoKey")]
        [FormerlySerializedAs("ID")]
        public string DidimoKey;

        private DidimoAnimator            _didimoAnimator;
        private DidimoDeformables         _didimoDeformables;
        private DidimoSpeech              _didimoSpeech;
        private DidimoPoseController      _didimoPoseController;
        private DidimoMaterials           _didimoMaterials;
        private DidimoEyeShadowController _didimoEyeShadowController;
        private TextureCache              _textureCache;
        public TextureCache TextureCache => _textureCache ??= new TextureCache();
        public DidimoBuildContext BuildContext { get; set; }

        public DidimoAnimator Animator
        {
            get
            {
                if (_didimoAnimator == null) ComponentUtility.GetOrAdd(this, ref _didimoAnimator);
                return _didimoAnimator;
            }
        }

        public DidimoPoseController PoseController
        {
            get
            {
                if (_didimoPoseController == null) ComponentUtility.GetOrAdd<DidimoPoseController, FallbackPoseController>(this, ref _didimoPoseController);
                return _didimoPoseController;
            }
        }

        public DidimoDeformables Deformables
        {
            get
            {
                if (_didimoDeformables == null) ComponentUtility.GetOrAdd(this, ref _didimoDeformables);
                return _didimoDeformables;
            }
        }

        public DidimoSpeech Speech
        {
            get
            {
                if (_didimoSpeech == null) ComponentUtility.GetOrAdd(this, ref _didimoSpeech);
                return _didimoSpeech;
            }
        }

        public DidimoMaterials Materials
        {
            get
            {
                if (_didimoMaterials == null) ComponentUtility.GetOrAdd(this, ref _didimoMaterials);
                return _didimoMaterials;
            }
        }

        public DidimoEyeShadowController EyeShadowController
        {
            get 
            {
             if (_didimoEyeShadowController == null) ComponentUtility.GetOrAdd(this, ref _didimoEyeShadowController);
                return _didimoEyeShadowController;
            }
        }

        protected void OnDestroy()
        {
            TextureCache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}