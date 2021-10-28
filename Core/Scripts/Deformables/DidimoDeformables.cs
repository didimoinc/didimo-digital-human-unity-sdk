using System.Collections.Generic;
using System.Linq;
using DigitalSalmon.Extensions;
using UnityEngine;

namespace Didimo
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

        public bool TryCreate<TDeformable>(string deformableId, out TDeformable instance) where TDeformable : Deformable
        {
            if (!DeformableDatabase.TryFindDeformable(deformableId, out TDeformable deformable))
            {
                Debug.LogWarning($"No database deformable found with ID: {deformableId}");
                instance = null;
                return false;
            }

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
            deformables.Add(deformableId, instance);

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
                Debug.LogWarning($"Cannot find ideal deformable bone with any of the names: '{string.Join(",", instance.IdealBoneNames)}'");
            }

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(idealBone ? idealBone : DidimoComponents.transform, true);
            instance.name = deformableId;

            return true;
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

        private bool Exists<TDeformable>() where TDeformable : Deformable => TryFindAllDeformables<TDeformable>().Any();

        private IEnumerable<TDeformable> TryFindAllDeformables<TDeformable>() where TDeformable : Deformable
        {
            return deformables.Where(d => d.Value is TDeformable).Select(d => d.Value).Cast<TDeformable>();
        }
    }
}