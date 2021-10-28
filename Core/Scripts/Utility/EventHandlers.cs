using System;
using UnityEngine;

namespace Didimo
{
    //-----------------------------------------------------------------------------------------
    // Delegates:
    //-----------------------------------------------------------------------------------------

    public delegate void EventHandler();

    public delegate void EventHandler<in T>(T value);

    public delegate void EventHandler<in T1, in T2>(T1 value1, T2 value2);

    public delegate void EventHandler<in T1, in T2, in T3>(T1 value1, T2 value2, T3 value3);

    public delegate void BoolEventHandler(bool value);

    public delegate void StringEventHandler(string value);

    public delegate void FloatEventHandler(float value);

    public delegate void IntEventHandler(int value);

    public delegate void ColorEventHandler(Color value);

    public delegate void Vector2EventHandler(Vector2 value);

    public delegate void Vector3EventHandler(Vector3 value);

    public delegate void Vector4EventHandler(Vector4 value);

    //-----------------------------------------------------------------------------------------
    // Classes:
    //-----------------------------------------------------------------------------------------

    public static class EventHandlerExtensions
    {
        //-----------------------------------------------------------------------------------------
        // Event Invocation:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Safely invoke an EventHandler.
        /// </summary>
        public static void InvokeSafe(this EventHandler self) { self?.Invoke(); }

        /// <summary>
        /// Safely invoke a generic EventHandler.
        /// </summary>
        public static void InvokeSafe<TEventArgs>(this EventHandler<TEventArgs> self, TEventArgs eventArgs) { self?.Invoke(eventArgs); }

        /// <summary>
        /// Safely invoke a generic EventHandler.
        /// </summary>
        public static void InvokeSafe<TEventArgs1, TEventArgs2>(this EventHandler<TEventArgs1, TEventArgs2> self, TEventArgs1 eventArgs1, TEventArgs2 eventArgs2) { self?.Invoke(eventArgs1, eventArgs2); }

        /// <summary>
        /// Safely invoke a generic EventHandler.
        /// </summary>
        public static void InvokeSafe<TEventArgs1, TEventArgs2, TEventArgs3>(this EventHandler<TEventArgs1, TEventArgs2, TEventArgs3> self, TEventArgs1 eventArgs1, TEventArgs2 eventArgs2, TEventArgs3 eventArgs3) { self?.Invoke(eventArgs1, eventArgs2, eventArgs3); }

        //-----------------------------------------------------------------------------------------
        // Event Invocation - Explicit Types:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Safely invoke a generic BoolEventHandler.
        /// </summary>
        public static void InvokeSafe(this BoolEventHandler self, bool value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic StringEventHandler.
        /// </summary>
        public static void InvokeSafe(this StringEventHandler self, string value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic FloatEventHandler.
        /// </summary>
        public static void InvokeSafe(this FloatEventHandler self, float value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic IntEventHandler.
        /// </summary>
        public static void InvokeSafe(this IntEventHandler self, int value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic ColorEventHandler.
        /// </summary>
        public static void InvokeSafe(this ColorEventHandler self, Color value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic Vector2EventHandler.
        /// </summary>
        public static void InvokeSafe(this Vector2EventHandler self, Vector2 value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic Vector3EventHandler.
        /// </summary>
        public static void InvokeSafe(this Vector3EventHandler self, Vector3 value) { self?.Invoke(value); }

        /// <summary>
        /// Safely invoke a generic Vector4EventHandler.
        /// </summary>
        public static void InvokeSafe(this Vector4EventHandler self, Vector4 value) { self?.Invoke(value); }

        //-----------------------------------------------------------------------------------------
        // Action Invocation:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Safely invoke an action if it is valid. Consider using an EventHandler instead.
        /// </summary>
        public static void InvokeSafe(this Action self) { self?.Invoke(); }

        /// <summary>
        /// Safely invoke an action if it is valid. Consider using a generic EventHandler instead.
        /// </summary>
        public static void InvokeSafe<T>(this Action<T> self, T value) { self?.Invoke(value); }
    }
}