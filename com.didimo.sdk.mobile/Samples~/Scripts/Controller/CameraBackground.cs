using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Extensions;
using UnityEngine;

namespace Didimo.Mobile.Controller
{
    [ExecuteInEditMode]
    public class CameraBackground : MonoBehaviour
    {
        public Color background;
        void Update()
        {
            SetCameraBackground();
        }

        void SetCameraBackground()
        {
            if (!Camera.main) return;
            Camera.main.backgroundColor = background;
        }
    }
}