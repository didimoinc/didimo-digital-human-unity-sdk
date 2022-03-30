using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Didimo.Extensions;
using Didimo.Core.Utility;

namespace Didimo.Core.Deformables
{
    public class DidimoDeformables : DidimoBehaviour
    {
        private readonly Dictionary<string, Deformable> deformables = new Dictionary<string, Deformable>();

        public bool TryFind<TDeformable>(string deformableId, out TDeformable instance) where TDeformable : Deformable
        {
            if (!deformables.TryGetValue(deformableId, out Deformable deformable))
            {
                instance = null;
                return false;
            }

            instance = deformable as TDeformable;
            return instance != null;
        }

        public bool TryFind<TDeformable>(out TDeformable instance) where TDeformable : Deformable
        {
            if (!deformables.Any(d => d.Value is TDeformable))
            {
                instance = null;
                return false;
            }

            instance = deformables.FirstOrDefault(d => d.Value is TDeformable).Value as TDeformable;
            return instance != null;
        }

        public bool TryCreate<TDeformable>(
            TDeformable deformable, out TDeformable instance)
            where TDeformable : Deformable
        {
            if (deformable == null)
            {
                Debug.LogWarning("Cannot instantiate deformable from a null template");
                instance = null;
                return false;
            }

            if (deformable.SingleInstancePerDidimo && Exists<TDeformable>())
            {
                DestroyAll<TDeformable>();
            }

            deformable.DidimoComponents = DidimoComponents;
            instance = Instantiate(deformable);
            deformables.Add(deformable.ID, instance);

            Transform idealBone = null;
            foreach (string idealBoneName in instance.IdealBoneNames)
            {
                if (transform.TryFindRecursive(idealBoneName, out idealBone))
                {
                    break;
                }
            }

            if (idealBone == null)
            {
                Debug.LogWarning($"Cannot find ideal deformable bone with any of "
                                 + $"the names: '{string.Join(",", instance.IdealBoneNames)}'");
            }

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(idealBone ? idealBone : DidimoComponents.transform, true);
            instance.name = deformable.ID;

            return true;
        }

        public bool TryCreate<TDeformable>(
            string deformableId, out TDeformable instance)
            where TDeformable : Deformable
        {
            var deformableDatabase = Resources
                .Load<DeformableDatabase>("DeformableDatabase");

            if (TryFindDeformable(deformableDatabase.Deformables,
                deformableId, out TDeformable deformable) == false)
            {
                Debug.LogWarning($"No database deformable found with ID: {deformableId}");
                instance = null;
                return false;
            }

            return TryCreate(deformable, out instance);
        }

        public void DestroyAll<TDeformable>() where TDeformable : Deformable
        {
            static void OnRemove(KeyValuePair<string, Deformable> kvp)
            {
                if (Application.isPlaying)
                {
                    Destroy(kvp.Value.gameObject);
                }
                else
                {
                    DestroyImmediate(kvp.Value.gameObject);
                }
            }

            deformables.RemoveWhere(d => d.Value is TDeformable, OnRemove);
        }

        private bool Exists<TDeformable>() where TDeformable : Deformable
        {
            return TryFindAllDeformables<TDeformable>().Any();
        }

        private IEnumerable<TDeformable> TryFindAllDeformables<TDeformable>() where TDeformable : Deformable
        {
            return deformables.Where(d => d.Value is TDeformable).Select(d => d.Value).Cast<TDeformable>();
        }

        public static bool TryFindDeformable(Deformable[] deformableArray,
            string id, out Deformable deformable)
        {
            deformable = deformableArray.FirstOrDefault(h => h.ID == id);

            return deformable != null;
        }

        public static bool TryFindDeformable<TDeformable>
            (Deformable[] deformableArray, string id,
            out TDeformable deformable)
            where TDeformable : Deformable
        {
            deformable = deformableArray.Where(d => d is TDeformable)
                .Cast<TDeformable>().FirstOrDefault(h => h.ID == id);

            return deformable != null;
        }
    }
}
