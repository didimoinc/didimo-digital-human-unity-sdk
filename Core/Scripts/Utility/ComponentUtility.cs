using UnityEngine;

namespace Didimo
{
    public static class ComponentUtility
    {
        public static void GetOrAdd<TComponent>(MonoBehaviour mono, ref TComponent backingField) where TComponent : Component { GetOrAdd<TComponent, TComponent>(mono, ref backingField); }

        public static void GetOrAdd<TComponent, TDefault>(MonoBehaviour mono, ref TComponent backingField) where TComponent : Component where TDefault : TComponent
        {
            if (backingField != null) return;
            backingField = mono.GetComponent<TComponent>();
            if (backingField != null) return;
            backingField = mono.gameObject.AddComponent<TDefault>();
        }

        public static void GetOrAdd<TComponent>(GameObject gameObject, ref TComponent backingField) where TComponent : Component { GetOrAdd<TComponent, TComponent>(gameObject, ref backingField); }

        public static void GetOrAdd<TComponent, TDefault>(GameObject gameObject, ref TComponent backingField) where TComponent : Component where TDefault : TComponent
        {
            if (backingField != null) return;
            backingField = gameObject.GetComponent<TComponent>();
            if (backingField != null) return;
            backingField = gameObject.AddComponent<TDefault>();
        }
        public static T FindParentThatImplements<T>(GameObject go)
        {
            T comp = go.GetComponent<T>();
            if (comp != null)
                return comp;
            if (go.transform.parent != null)
                return FindParentThatImplements<T>(go.transform.parent.gameObject);
            return default(T);
        }

    }
}