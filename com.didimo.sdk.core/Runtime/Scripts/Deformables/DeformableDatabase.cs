using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Core.Deformables
{
    [CreateAssetMenu(fileName = "DeformableDatabase", menuName = "Didimo/Deformable Database")]
    public class DeformableDatabase : ScriptableObject
    {
        [SerializeField] public Deformable[] Deformables;

        public IEnumerable<string> AllIDs => Deformables.Select(h => h.ID);
    }
}
