using UnityEngine;

namespace Didimo.Core.Examples.MeetADidimo
{
    public class SpeechManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject man;
        [SerializeField]
        private GameObject woman;

        void Start()
        {
            var o = woman.GetComponent<MeetADidimo>();
            var i = man.GetComponent<MeetADidimo>();

            o.SpeakDidimoFirst();
            i.SpeakDidimoSecond();
        }
    }
}