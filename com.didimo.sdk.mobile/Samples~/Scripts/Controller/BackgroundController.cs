using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Extensions;
using UnityEngine;

namespace Didimo.Mobile.Controller
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] int activeIndex;
        [SerializeField] CameraBackground[] objects;

        void OnValidate() => Select(activeIndex);
        void Start() => Select(activeIndex);

        public void Select(int index)
        {
            activeIndex = Mathf.Clamp(index, 0, objects.Length - 1);
            objects.Select((c, i) => (c, i)).ForEach(g => g.c.gameObject.SetActive(g.i == activeIndex));
        }

        // [System.Serializable]
        // public class Config
        // {
        //     public Color backgroundColour;
        //     public GameObject gameObject;

        //     public void SetActive(bool active)
        //     {
        //         gameObject.SetActive(active);
        //         Camera.main.backgroundColor = backgroundColour;
        //     }
        // }
    }
}