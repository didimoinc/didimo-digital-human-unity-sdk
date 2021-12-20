using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Didimo
{
    /// <summary>
    /// An 'integer range' class to be used in place of integers where you want a maximum range from 0->maxrange for that integer in the UI inspector widget for this type
    /// </summary>
    [System.Serializable()]
    public struct IntRange
    {
        [SerializeField]
        private int _maxValue;
        [SerializeField]
        private int _value;
        
        public IntRange(int value = 0, int maxValue = 0)
        {
            _value = value;
            _maxValue = maxValue;
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                _value = Mathf.Clamp(_value, 0, _maxValue);
            }
        }

        public int Value
        {
            get { return _value; }
            set { _value = Mathf.Clamp(value, 0, _maxValue); }
        }
    }
}
