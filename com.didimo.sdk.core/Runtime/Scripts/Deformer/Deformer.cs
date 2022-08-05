using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Core.Deformer
{
    public class Deformer
    {
        public const float SCALE = 100f;

        public virtual Vector3[] DeformVertices(IEnumerable<Vector3> deformableVertices) { return deformableVertices.ToArray(); }


        // Expected coordinate system is in cm, and X axis in the opposite direction
        protected static Vector3 TransformVertexCoordinates(Vector3 vector) => new Vector3(-vector.x, vector.y, vector.z) * SCALE;
        protected static IEnumerable<Vector3> TransformVertexCoordinates(IEnumerable<Vector3> vectors) => vectors.Select(TransformVertexCoordinates);

        // Undo coordinate system transformation
        protected static Vector3 RevertVertexCoordinates(Vector3 vector) => new Vector3(-vector.x, vector.y, vector.z) / SCALE;
        protected static IEnumerable<Vector3> RevertVertexCoordinates(IEnumerable<Vector3> vectors) => vectors.Select(RevertVertexCoordinates);

        public virtual bool IsAbleToDeform() { return false; }
    }
}