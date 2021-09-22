using DigitalSalmon;
using UnityEngine;

namespace Didimo
{
    public class DidimoBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private DidimoComponents _didimo;

        private Sequence _sequence;

        public DidimoComponents DidimoComponents
        {
            get
            {
                if (_didimo == null) _didimo = GetComponent<DidimoComponents>();
                return _didimo;
            }
            set => _didimo = value;
        }

        protected Sequence Sequence
        {
            get
            {
                if (_sequence == null) _sequence = new Sequence(this);
                return _sequence;
            }
        }
    }
}