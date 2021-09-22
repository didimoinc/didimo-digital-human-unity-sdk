using Didimo.Builder;
using Didimo.Inspector;
using UnityEngine;

namespace Didimo
{
    public enum EHairPresetColor
    {
        BLACK = 0,
        DARK_BROWN = 1,
        BROWN = 2,
        LIGHT_BROWN = 3,
        DARK_BLONDE = 4,
        BLONDE = 5,
        BRIGHT_BLONDE = 6,
        GREY = 7,
        AUBURN = 8,
        GINGER = 9,
        RED = 10
    };

    [System.Serializable]
    public class PresetValues
    {
        static public PresetValues[] presets = {  new PresetValues(new Color(0.01f, 0.01f, 0.01f)),    //BLACK = 0,
                                                  new PresetValues(new Color(0.2f, 0.05f, 0.01f)),           //DARK_BROWN = 1,
                                                  new PresetValues(new Color(0.4f, 0.2f, 0.1f)),           //BROWN = 2,
                                                  new PresetValues(new Color(0.5f, 0.31f, 0.1f)),           //LIGHT_BROWN = 3,
                                                  new PresetValues(new Color(0.75f, 0.4f, 0.3f)),           //DARK_BLONDE = 4,
                                                  new PresetValues(new Color(0.9f, 0.7f, 0.4f)),           //BLONDE = 5,
                                                  new PresetValues(new Color(0.9f, 0.9f, 0.8f)),           //BRIGHT_BLONDE = 6,
                                                  new PresetValues(new Color(0.5f, 0.5f, 0.6f)),           //GREY = 7,
                                                  new PresetValues(new Color(0.6f, 0.09f, 0.1f)),           //AUBURN = 8,
                                                  new PresetValues(new Color(0.91f, 0.4f, 0.1f)),          //GINGER = 9,  
                                                  new PresetValues(new Color(1.0f, 0.05f, 0.1f)),            //RED = 10                                                            
        };        
        public Color color = new Color(1.0f, 1.0f, 1.0f);
        public PresetValues(Color col)
        {
            color = col;
        }
    }

    [System.Serializable]
    public class HairPreset
    {
        public EHairPresetColor value;
    }


    public class Hair : Deformable
    {
        private static readonly int PROPERTY_HAIRCOLOR = Shader.PropertyToID("_HairColor");
        private static readonly int PROPERTY_HAIRCAP = Shader.PropertyToID("_HairCap");
   
        [Header("Hair")]
        [SerializeField]
        [OnValueChanged("ApplyHairPropertiesToHierarchy")]
        protected Texture2D hairCapTexture;

        [SerializeField]
        [OnValueChanged("ApplyHairPropertiesToHierarchy")]
        protected Color color;

        [SerializeField]        
        HairPreset Preset;

        public Texture2D HairCapTexture => hairCapTexture;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                ApplyHairPropertiesToHierarchy();
            }
        }        

        public override bool SingleInstancePerDidimo => true;

        protected void Start() { ApplyHairPropertiesToHierarchy(); }

        private void OnDestroy()
        {
            void OnPropertyBlock(MaterialPropertyBlock propertyBlock)
            {
                // Unfortunately we cannot set a property to null, but a black texture will work just fine.
                propertyBlock.SetTexture(PROPERTY_HAIRCAP, Texture2D.blackTexture);
            }

            MaterialBuilder.ProcessPropertyBlocksInHierarchy(DidimoComponents != null ? DidimoComponents.transform : transform, OnPropertyBlock);
        }

        public void SetPreset(EHairPresetColor preset)
        {
            Preset.value = preset;
            ApplyPreset();
        }

        public void ApplyPreset()
        {
            color = PresetValues.presets[(int)Preset.value].color;
            ApplyHairPropertiesToHierarchy();
        }
        private void ApplyHairPropertiesToHierarchy()
        {
            void OnPropertyBlock(MaterialPropertyBlock propertyBlock)
            {
                if (HairCapTexture != null) propertyBlock.SetTexture(PROPERTY_HAIRCAP, HairCapTexture);
                propertyBlock.SetColor(PROPERTY_HAIRCOLOR, Color);
            }

            MaterialBuilder.ProcessPropertyBlocksInHierarchy(DidimoComponents != null ? DidimoComponents.transform : transform, OnPropertyBlock);
        }
    }
}