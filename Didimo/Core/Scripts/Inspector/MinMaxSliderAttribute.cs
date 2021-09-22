using UnityEngine;

namespace Didimo.Inspector
{
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float MIN;
        public readonly float MAX;

        public MinMaxSliderAttribute() { }

        public MinMaxSliderAttribute(float min, float max)
        {
            MIN = min;
            MAX = max;
        }
    }
}