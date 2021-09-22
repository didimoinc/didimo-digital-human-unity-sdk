using UnityEngine;
using UResources = UnityEngine.Resources;

// ReSharper disable once MergeConditionalExpression
#pragma warning disable IDE0029 // Use coalesce expression

namespace DigitalSalmon
{
    public static class Resource
    {
        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public static T Load<T>(string path) where T : Object => UResources.Load<T>(path);

        public static T[] LoadAll<T>(string path) where T : Object => UResources.LoadAll<T>(path);

        public static T[] FindObjectsOfTypeAll<T>() where T : Object => UResources.FindObjectsOfTypeAll<T>();

        public static T LocateResource<T>(ref T backingField, string path) where T : Object => backingField != null ? backingField : backingField = UResources.Load<T>(path);

        public static T[] LocateResources<T>(ref T[] backingField, string path) where T : Object => backingField != null ? backingField : backingField = UResources.LoadAll<T>(path);
    }
}