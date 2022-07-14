using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace Didimo.Core.Utility
{   
    public static class ComponentUtility
    {
        public static void GetOrAdd<TComponent>(MonoBehaviour mono, ref TComponent backingField)
            where TComponent : Component { GetOrAdd<TComponent, TComponent>(mono, ref backingField); }

        public static void GetOrAdd<TComponent, TDefault>(
            MonoBehaviour mono, ref TComponent backingField) where TComponent : Component where TDefault : TComponent
        {
            if (backingField != null) return;
            backingField = mono.GetComponent<TComponent>();
            if (backingField != null) return;
            backingField = mono.gameObject.AddComponent<TDefault>();
        }

        public static void ForEachGameObject(GameObject root, Func<GameObject, GameObject> doit)
        {
            doit(root);
            foreach (Transform t in root.transform)            
                ForEachGameObject(t.gameObject, doit);            
        }

        public static TComponent GetOrAdd<TComponent>(GameObject go) where TComponent : Component
        {
            if (!go)
                return null;
            TComponent comp = go.GetComponent<TComponent>();
            if (comp == null)
                comp = go.AddComponent<TComponent>();
            return comp;
        }

  

        public static TComponent SafeGetComponent<TComponent>(GameObject go) where TComponent : Component
        {
            return go != null ? go.GetComponent<TComponent>() : null;
        }
        public static GameObject GetChildWithName(GameObject parent, string name, bool recurse = false)
        {
            Transform t = parent.GetComponent<Transform>();
            for (var i = 0; i < t.childCount; ++i)
            {
                var child = t.GetChild(i);
                if (child.name == name)                
                    return child.gameObject;                
            }
            if (recurse)
            {
                for (var i = 0; i < t.childCount; ++i)
                {
                    var res = GetChildWithName(t.GetChild(i).gameObject, name, true);
                    if (res)
                        return res;
                }
            }
            return null;
        }

        public static void GetOrAdd<TComponent>(
            GameObject gameObject,
            ref TComponent backingField) where TComponent : Component 
        {
            GetOrAdd<TComponent, TComponent>(gameObject, ref backingField);
        }

        public static void GetOrAdd<TComponent, TDefault>(
            GameObject gameObject, ref TComponent backingField)
            where TComponent : Component where TDefault : TComponent
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
        public static List<GameObject> CreateFlatList(GameObject[] objectList)
        {
            List<GameObject> oblist = new List<GameObject>();
            foreach (var ob in objectList)
            {
                CreateFlatListInner(ob, oblist);
            }
            return oblist;
        }

        public static void CreateFlatListInner(GameObject ob, List<GameObject> objectList)
        {
            objectList.Add(ob);
            var t = ob.transform;
            for (var i = 0; i < t.childCount; ++i)
            {
                CreateFlatListInner(t.GetChild(i).gameObject, objectList);
            }
        }

        public delegate bool GameObjectFunc(GameObject go);
        public static HashSet<GameObject> GetUniqueTopLevelList(GameObject[] selection, GameObjectFunc filter)
        {
            HashSet<GameObject> hs = new HashSet<GameObject>();            
            foreach (GameObject goitr in selection)
            {
                GameObject go = goitr.transform.root.gameObject;
                var smrlist = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                var mrlist = go.GetComponentsInChildren<MeshRenderer>();
                List<Material> ml = new List<Material>();
                foreach (var smr in smrlist)
                {
                    if (filter(smr.gameObject))
                        hs.Add(smr.gameObject);
                }
            }
            return hs;
        }
    }
}