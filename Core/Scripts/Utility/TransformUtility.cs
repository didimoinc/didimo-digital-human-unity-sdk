using UnityEngine;
using System;
namespace Didimo
{
    public static class TransformUtility
    {
        public static bool TryFindSharedMaterialInHierarchy(Transform transform, string matName, out Material material)
        {
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterial == null) continue;
                if (renderer.sharedMaterial.name.StartsWith(matName))
                {
                    material = renderer.sharedMaterial;
                    return true;
                }
            }

            material = null;
            return false;
        }

        public static Transform FirstChildOrDefault(this Transform parent, Func<Transform, bool> query)
        {
            if (parent.childCount == 0)
                return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (query(child))
                    return child;
                Transform result = FirstChildOrDefault(child, query);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}