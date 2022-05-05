using System;
using System.Collections;
using System.Collections.Generic;
using Didimo.Core.Utility;
using Didimo.Extensions;
#if UNITY_IOS || UNITY_ANDROID
using Didimo.Mobile.Communication;
#endif
using UnityEngine;

namespace Didimo.Mobile
{
    public class TouchInputProcessor : ASingletonBehaviour<TouchInputProcessor>
    {
#if UNITY_IOS || UNITY_ANDROID
        private static bool dragging;
        private        int  tapCount;

        public class CallbackEvents
        {
            public readonly RegisterTouchEvents.TouchCallback doubleTapCallback, startDragCallback, endDragCallback;
            public          IntPtr                            objectPointer;

            public CallbackEvents(RegisterTouchEvents.TouchCallback doubleTapCallback, RegisterTouchEvents.TouchCallback startDragCallback,
                RegisterTouchEvents.TouchCallback endDragCallback, IntPtr objectPointer)
            {
                this.doubleTapCallback = doubleTapCallback;
                this.startDragCallback = startDragCallback;
                this.endDragCallback = endDragCallback;
                this.objectPointer = objectPointer;
            }
        }

        private static List<CallbackEvents> callbacksList = new List<CallbackEvents>();

        public void Register(CallbackEvents callbacks) { callbacksList.Add(callbacks); }

        public void UnRegister(IntPtr objPtr) { callbacksList.RemoveWhere(cb => cb.objectPointer == objPtr); }

        private IEnumerator ResetTapCount()
        {
            yield return new WaitForSeconds(0.3f);
            tapCount = 0;
        }

        private void Update()
        {
            if (callbacksList.Count <= 0 || Input.touchCount == 0) return;

            foreach (var touch in Input.touches)
            {
                // Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Moved:
                    {
                        if (!dragging)
                        {
                            dragging = true;
                            foreach (var callback in callbacksList)
                            {
                                callback.startDragCallback?.Invoke((int) touch.position.x, (int) touch.position.y, callback.objectPointer);
                            }
                        }

                        break;
                    }
                    case TouchPhase.Ended:
                    {
                        if (dragging)
                        {
                            foreach (var callback in callbacksList)
                            {
                                callback.endDragCallback?.Invoke((int) touch.position.x, (int) touch.position.y, callback.objectPointer);
                            }
                        }

                        dragging = false;
                        break;
                    }
                }

                if (touch.tapCount == 2)
                {
                    foreach (var callback in callbacksList)
                    {
                        callback.doubleTapCallback?.Invoke((int) touch.position.x, (int) touch.position.y, callback.objectPointer);
                    }
                }
            }
        }
#endif
    }
}