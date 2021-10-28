using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo
{
    [CreateAssetMenu(fileName = "DeformableDatabase", menuName = "Didimo/Deformable Database")]
    public class DeformableDatabase : ScriptableObject
    {
        [SerializeField]
        private Deformable[] deformables;

        public static Deformable[] Deformables => DidimoResources.DeformableDatabase.deformables;

        public static IEnumerable<string> AllIDs => Deformables.Select(h => h.ID);

        public static bool TryFindDeformable(string id, out Deformable deformable)
        {
            deformable = Deformables.FirstOrDefault(h => h.ID == id);
            return deformable != null;
        }

        public static bool TryFindDeformable<TDeformable>(string id, out TDeformable deformable) where TDeformable : Deformable
        {
            deformable = Deformables.Where(d => d is TDeformable).Cast<TDeformable>().FirstOrDefault(h => h.ID == id);
            return deformable != null;
        }
    }
}