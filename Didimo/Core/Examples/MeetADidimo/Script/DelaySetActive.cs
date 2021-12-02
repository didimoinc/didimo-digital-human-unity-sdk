using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Didimo.Core.Examples.MeetADidimo
{
    public class DelaySetActive : MonoBehaviour
    {
        public UnityEvent DelayComplete;
        public float DelaySeconds = 5;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(DelaySeconds);
            DelayComplete?.Invoke();
        }
    }
}