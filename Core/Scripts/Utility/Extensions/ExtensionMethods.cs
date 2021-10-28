using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Didimo
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get the string value of the currently selected <see cref="Dropdown.OptionData" /> object.
        /// </summary>
        /// <param name="dropDown">The extended Dropdown.</param>
        /// <returns>The string value of the currently selected <see cref="Dropdown.OptionData" /> object</returns>
        public static string StringValue(this Dropdown dropDown)
        {
            if (dropDown.options.Count != 0)
            {
                return dropDown.options[dropDown.value].text;
            }

            return null;
        }

        /// <summary>
        /// Set the current selected value of the dropbox as the option with the given string value.
        /// </summary>
        /// <param name="dropDown">The extended Dropdown.</param>
        /// <param name="value">The string value we want to select.</param>
        public static bool SetSelectedValue(this Dropdown dropDown, string value)
        {
            for (int i = 0; i < dropDown.options.Count; i++)
            {
                {
                    if (dropDown.options[i].text.Equals(value))
                    {
                        dropDown.value = i;
                        dropDown.RefreshShownValue();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Find for a game object with the provided name, starting from the given transform.
        /// </summary>
        /// <param name="transform">The extended Tranform object.</param>
        /// <param name="name">The name to look for.</param>
        /// <returns>A Transform object with the given name, null if not found.</returns>
        public static bool TryFindRecursive(this Transform transform, string name, out Transform result)
        {
            if (transform.name.Equals(name))
            {
                result = transform;
                return true;
            }

            result = transform.Find(name);
            if (result != null)
                return true;

            foreach (Transform child in transform)
            {
                if (child.TryFindRecursive(name, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        public static bool TryFindRecursive<TComponent>(this Transform transform, string name, out TComponent result) where TComponent : Component
        {
            if (transform.TryFindRecursive(name, out Transform target))
            {
                result = target.GetComponent<TComponent>();
                return result != null;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Swap two elements at the given indices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index1">Swap the element in this index for the one at index2.</param>
        /// <param name="index2">Swap the element in this index for the one at index1.</param>
        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
    }
}