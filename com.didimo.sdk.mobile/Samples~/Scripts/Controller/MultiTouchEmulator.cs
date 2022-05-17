// using UnityEngine;
// using System.Collections.Generic;

// public static class MultiTouchEmulator
// {
//     private static Vector3 lastMousePosition;
//     private static Vector2 dualMousePosition;
//     private static Dictionary<int, Touch> map = new Dictionary<int, Touch>();

//     public static Touch[] touches
//     {
//         get
//         {
//             Vector2 delta = Input.mousePosition - lastMousePosition;
//             lastMousePosition = Input.mousePosition;

//             Dictionary<int, Touch> old = map;
//             map = new Dictionary<int, Touch>();

//             if (true == Input.GetMouseButton(0))
//             {
//                 if (true == Input.GetKey(KeyCode.LeftShift) || true == Input.GetKey(KeyCode.RightShift))
//                 {
//                     map[1] = new Touch { position = Input.mousePosition, deltaPosition = delta };

//                     delta *= true == Input.GetKey(KeyCode.LeftControl) ? 0.66f : -1;
//                     dualMousePosition += delta;

//                     map[0] = new Touch { position = dualMousePosition, deltaPosition = delta };
//                 }
//                 else
//                 {
//                     map[0] = new Touch { position = dualMousePosition = Input.mousePosition, deltaPosition = delta };
//                 }
//             }
//             else
//             {
//                 if (true == Input.GetKey(KeyCode.LeftControl))
//                 {
//                     map[0] = new Touch { position = dualMousePosition, deltaPosition = Vector2.zero };
//                 }
//             }

//             if (true == Input.GetMouseButton(1))
//             {
//                 map[0] = map[1] = new Touch { position = Input.mousePosition, deltaPosition = delta };
//             }

//             List<Touch> result = new List<Touch>();

//             foreach (var kvp in old)
//             {
//                 if (false == map.ContainsKey(kvp.Key))
//                     result.Add(new Touch { fingerId = kvp.Key, position = kvp.Value.position, deltaPosition = Vector2.zero, phase = TouchPhase.Ended });
//             }

//             foreach (var kvp in map)
//             {
//                 TouchPhase _phase = true == old.ContainsKey(kvp.Key) ? 0 != kvp.Value.deltaPosition.sqrMagnitude ? TouchPhase.Moved : TouchPhase.Stationary : TouchPhase.Began;
//                 result.Add(new Touch { fingerId = kvp.Key, position = kvp.Value.position, deltaPosition = kvp.Value.deltaPosition, phase = _phase });
//             }

//             return result.ToArray();
//         }
//     }

//     public static void OnGUI()
//     {
//         foreach (var kvp in map)
//             DrawQuad(new Rect(kvp.Key * 2 + kvp.Value.position.x - 6, kvp.Value.position.y - 6, 12, 12), kvp.Key == 0 ? new Color(1, 0, 0, 0.5f) : new Color(0, 0, 1, 0.5f));
//     }

//     private static void DrawQuad(Rect rect, Color color)
//     {
//         Texture2D texture = new Texture2D(1, 1);
//         texture.SetPixel(0, 0, color);
//         texture.Apply();
//         GUI.skin.box.normal.background = texture;
//         rect.y = Screen.height - rect.y - rect.height;
//         GUI.Box(rect, GUIContent.none);
//     }
// }