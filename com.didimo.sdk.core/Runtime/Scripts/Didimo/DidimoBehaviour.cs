using UnityEngine;

namespace Didimo.Core.Utility
{
    /// <summary>
    /// MonoBehaviour class that is used by our didimo components to inherit from
    /// when requiring access to other didimo components.
    /// </summary>
    public class DidimoBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private DidimoComponents didimo;

        private Sequence sequence;

        public DidimoComponents DidimoComponents
        {
            get
            {
                if (didimo == null) didimo = GetComponent<DidimoComponents>();
                return didimo;
            }
            set => didimo = value;
        }

        protected Sequence Sequence
        {
            get
            {
                if (sequence == null) sequence = new Sequence(this);
                return sequence;
            }
        }
    }
}