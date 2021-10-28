using System;
using UnityEditor;
using UnityEngine;

namespace Didimo
{
    public class DidimoMaterials : DidimoBehaviour
    {
        public enum DidimoColorPropertyOverride
        {
            None,
            EyeColor,
            HairColor
        }

        public void OverrideColor(DidimoColorPropertyOverride propertyOverride, Color color)
        {
            (string propertyName, string overridePropertyName) propertyNames = GetPropertyNames(propertyOverride);
            SetColor(propertyNames.propertyName, color);
            SetInt(propertyNames.overridePropertyName, 1);
        }

        public void ClearColorOverride(DidimoColorPropertyOverride propertyOverride)
        {
            (string propertyName, string overridePropertyName) propertyNames = GetPropertyNames(propertyOverride);
            SetInt(propertyNames.overridePropertyName, 0);
        }

        public void SetColor(string property, Color color) { SetProperties((b) => b.SetColor(property, color)); }

        public void SetFloat(string property, float val) { SetProperties((b) => b.SetFloat(property, val)); }

        public void SetInt(string property, int val) { SetProperties((b) => b.SetInt(property, val)); }

        public void SetProperties(Action<MaterialPropertyBlock> onBlock)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (Renderer r in renderers)
            {
                r.GetPropertyBlock(propertyBlock);
                onBlock(propertyBlock);
                r.SetPropertyBlock(propertyBlock);
            }
        }

        private (string propertyName, string overridePropertyName) GetPropertyNames(DidimoColorPropertyOverride propertyOverride)
        {
            string property = $"_{propertyOverride}";
            string overrideProperty = $"_Override{propertyOverride}";
            return (property, overrideProperty);
        }
    }
}