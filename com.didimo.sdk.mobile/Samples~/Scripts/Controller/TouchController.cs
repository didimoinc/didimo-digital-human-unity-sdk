using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Utility;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Emulation: 
// 1 Touch - Left
// 2 Touch - Right 
// 2 Pinch - Left-Shift

namespace Didimo.Mobile.Controller
{
    [DefaultExecutionOrder(-10)]
    public class TouchController : ASingletonBehaviour<TouchController>
    {
        public static Touch[] Touches;
        public static int TouchCount => Touches.Length;
        static Vector3? LastScreenPosition;

        /// <summary>Get Touch by Index</summary>
        public static Touch GetTouch(int index) => index < Touches.Length ? Touches[index] : new Touch();

        public static Vector3 ScreenSize => new Vector3(Screen.width, Screen.height, 0);
        public static Vector3 ScreenPosition
        {
            get
            {
                if (TouchCount == 1) LastScreenPosition = GetTouch(0).position;
                else if (TouchCount == 2) LastScreenPosition = (GetTouch(1).position + GetTouch(0).position) / 2;
                return (Vector3)LastScreenPosition;
            }
        }

        public static Vector3 GetLookPosition(float hscale, float vscale)
        {
            var p = ScreenPosition;
            float w = Screen.width / 2, h = Screen.height / 2;
            return Camera.main.ScreenPointToRay(new Vector3((p.x - w) * hscale + w, (p.y - h) * vscale + h)).origin;
        }

        static float Size => Mathf.Min(Screen.width, Screen.height);
        public static Vector3 Unitize(Vector3 v) => new Vector3(v.x / Size, v.y / Size);
        public static float Unitize(float v) => v / Size;

        void Update()
        {
            Touches = GetTouches().ToArray();
            // if (TouchCount == 1) Debug.Log(Time.frameCount + " " + TouchCount + " " + GetTouch(0).phase);
            // if (TouchCount == 2) Debug.Log(Time.frameCount + " " + TouchCount + " " + GetTouch(0).phase + " " + GetTouch(1).phase);
        }

#if UNITY_EDITOR
        static Vector3? DownScreenPosition;

        void OnGUI()
        {
            const float Size = 10;
            for (int i = 0; i < TouchCount; i++)
            {
                var touch = GetTouch(i);
                var p = new Rect(touch.position.x - Size / 2, (Screen.height - touch.position.y) - Size / 2, Size, Size);
                GUI.Box(p, GUIContent.none, touchStyle);
            }
        }

        GUIStyle _touchStyle;
        GUIStyle touchStyle
        {
            get
            {
                if (_touchStyle == null)
                {
                    _touchStyle = new GUIStyle(GUI.skin.box);
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.red);
                    texture.Apply();
                    _touchStyle.normal.background = texture;
                }
                return _touchStyle;
            }
        }
#endif
        static IEnumerable<Touch> GetTouches()
        {
#if UNITY_EDITOR
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                DownScreenPosition = null;
                yield break;
            }

            Touch touch = new Touch();
            touch.position = Input.mousePosition;
            if (DownScreenPosition == null)
            {
                DownScreenPosition = Input.mousePosition;
                touch.phase = TouchPhase.Began;
            }
            else touch.phase = TouchPhase.Moved;

            if (Input.GetMouseButton(0))
            {
                yield return touch;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    var vector = Input.mousePosition - (Vector3)DownScreenPosition;
                    touch.position = Input.mousePosition - vector * 2;
                    touch.phase = Input.GetKeyDown(KeyCode.LeftShift) ? TouchPhase.Began : touch.phase;
                    yield return touch;
                }
            }
            else if (Input.GetMouseButton(1))
            {
                yield return touch;
                yield return touch;
            }
#else
            for (int i = 0; i < Input.touchCount; i++)
                yield return Input.GetTouch(i);
#endif
        }
    }
}


// #if UNITY_EDITOR
// if (TouchCount > 0)
//     touches[0] = Input.mouse;

// #else
//             Touch touch;
//             if (TouchCount > 0 && (touch = Input.GetTouch(0)).phase == TouchPhase.Began)
//                 touches[0] = touch.position;
//             if (TouchCount > 1 && (touch = Input.GetTouch(1)).phase == TouchPhase.Began)
//                 touches[1] = touch.position;
// #endif
//         }

//         public static Vector3 ScreenSize => new Vector3(Screen.width, Screen.height, 0);
// public static Vector3 ScreenPosition
// {
//     get
//     {
//         if (TouchCount == 1) LastScreenPosition = Input.GetTouch(0).position;
//         else if (TouchCount == 2) LastScreenPosition = (Input.GetTouch(1).position + Input.GetTouch(0).position) / 2;
//         return (Vector3)LastScreenPosition;
//     }
// }


// public static Vector3 GetLookPosition(float scale) =>
//     Camera.main.ScreenPointToRay((ScreenPosition - ScreenSize / 2) * scale + ScreenSize / 2).origin;

// public static float PinchDelta =>
//     TouchCount != 2 ? 0 : Vector2.Distance(Input.GetTouch(1).position, Input.GetTouch(0).position) -
//         Vector2.Distance(touches[1], touches[0]);