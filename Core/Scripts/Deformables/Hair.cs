using Didimo.Builder;
using Didimo.Core.Config;
using Didimo.Core.Inspector;
using UnityEngine;

namespace Didimo.Core.Deformables
{
    [System.Serializable]
    public class HairPreset
    {
        public int value;
    }

    public class Hair : Deformable
    {
        private static readonly int PROPERTY_HAIRCOLOR = Shader.PropertyToID("_HairColor");
        private static readonly int PROPERTY_HAIRCAP = Shader.PropertyToID("_HairCap");
        private static readonly int PROPERTY_SPECMULTIPLY = Shader.PropertyToID("_specMultiply");
        private static readonly int PROPERTY_SPECSHIFT = Shader.PropertyToID("_specShift");
        private static readonly int PROPERTY_SPECSHIFT2 = Shader.PropertyToID("_specShift2");
        private static readonly int PROPERTY_SPECEXP2 = Shader.PropertyToID("_specExp2");
        private static readonly int PROPERTY_SPECEXP1 = Shader.PropertyToID("_specExp1");
        private static readonly int PROPERTY_FLOWMULTIPLY = Shader.PropertyToID("_flowMultiplier");

        [Header("Hair")]
        [SerializeField]
        [OnValueChanged("ApplyHairPropertiesToHierarchy")]
        protected Texture2D hairCapTexture;
        public Texture2D HairCapTexture => hairCapTexture;

        [SerializeField]
        public HairLayerSettings outerHairLayer;

        [SerializeField]
        public HairLayerSettings innerHairLayer;

        [SerializeField]
        HairPreset Preset;

        public Color Color
        {
            get
            {
                if( innerHairLayer != null)
                {
                    return innerHairLayer.color;
                }
                else if( outerHairLayer != null)
                {
                    return outerHairLayer.color;
                }
                else
                {
                    return Color.white;
                }
            }

            set
            {
                if (innerHairLayer != null)
                    innerHairLayer.color = value;
                if (outerHairLayer != null)
                    outerHairLayer.color = value;
                ApplyHairPropertiesToHierarchy();
            }
        }

        public override bool SingleInstancePerDidimo => true;

        protected void Start() { ApplyHairPropertiesToHierarchy(); }

        private void OnDestroy()
        {
            void OnPropertyBlock(MaterialPropertyBlock propertyBlock,
                Component callingComponent, Renderer renderer, Material mat)
            {
                // Unfortunately we cannot set a property to null, but a black texture will work just fine.
                propertyBlock.SetTexture(PROPERTY_HAIRCAP, Texture2D.blackTexture);
            }

            MaterialBuilder.ProcessPropertyBlocksInHierarchy(
                DidimoComponents != null ? DidimoComponents.transform : transform, this, OnPropertyBlock);
        }

        public void SetPreset(int preset)
        {
            Preset.value = preset;
            ApplyPreset();
        }

        public void ApplyPreset()
        {
            var hairPresetDatabase = Resources
                .Load<HairPresetDatabase>("HairPresetDatabase");

            var hdb = hairPresetDatabase.Hairs[Preset.value];
            hdb.Apply(this);
            ApplyHairPropertiesToHierarchy();
        }

        public void OnValidate()
        {
            ApplyHairPropertiesToHierarchy();
        }

        public void Update()
        {
            ApplyHairPropertiesToHierarchy();
        }

        readonly string[] testOuterLayerStrings = { "cards", "outer" };
        private void ApplyHairPropertiesToHierarchy()
        {
            void OnPropertyBlock(MaterialPropertyBlock propertyBlock,
            Component callingComponent, Renderer render, Material currentMaterial)
            {
                if (currentMaterial == null) return;
                if (HairCapTexture != null) propertyBlock.SetTexture(PROPERTY_HAIRCAP, HairCapTexture);
                string rendername = render.name;
                string matname = currentMaterial.name;
                bool outer = false;
                foreach (string s in testOuterLayerStrings)
                {
                    if ((matname.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0)
                       || (rendername.IndexOf(s, System.StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        outer = true;
                        break;
                    }
                }
                var hairLayer = outer ? outerHairLayer : innerHairLayer;
                propertyBlock.SetFloat(PROPERTY_SPECMULTIPLY, hairLayer.shineMultiplier);
                propertyBlock.SetFloat(PROPERTY_SPECEXP2,     hairLayer.glossiness2);
                propertyBlock.SetFloat(PROPERTY_SPECEXP1,     hairLayer.glossiness1);
                propertyBlock.SetFloat(PROPERTY_SPECSHIFT,    hairLayer.specShift1);
                propertyBlock.SetFloat(PROPERTY_SPECSHIFT2,   hairLayer.specShift2);
                propertyBlock.SetFloat(PROPERTY_FLOWMULTIPLY, hairLayer.flowMultiply);
                propertyBlock.SetColor(PROPERTY_HAIRCOLOR,    hairLayer.color);
            }
            MaterialBuilder.ProcessPropertyBlocksInHierarchy(DidimoComponents != null
                ? DidimoComponents.transform : transform, this, OnPropertyBlock);
        }
    }
}