using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public static class GeomTools
    {
        public static float Volume(this Bounds b) => b.size.x * b.size.y * b.size.z;

        static Dictionary<string, Transform> GetHierarchyPaths(Transform root)
        {
            Dictionary<string, Transform> paths = new Dictionary<string, Transform>();
            void Recursive(Transform transform, string path = "")
            {
                paths.Add(path, transform);
                foreach (Transform t in transform)
                    Recursive(t, path + "/" + t.name);
            }
            Recursive(root);
            return paths;
        }


        public static class CompareVertex
        {
            const float NearDistance = 0.0001f;
            public static bool Position(Vector3 v1, Vector3 v2) => Position(v1, v2, NearDistance);
            // Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);

            public static bool Position(Vector3 v1, Vector3 v2, float threshold) =>
                Mathf.Abs(v1.x - v2.x) <= threshold && Mathf.Abs(v1.y - v2.y) <= threshold && Mathf.Abs(v1.z - v2.z) <= threshold;

            public static IEnumerable<int> Positions(Vector3[] v1, float threshold = 0.00001f)
            {
                var partition = new Partition3D<int>(v1, 0.01f);
                for (int i = 0; i < v1.Length; i++)
                    partition.Add(i, v1[i]);

                for (int j = 0; j < v1.Length; j++)
                    foreach (int i in partition.Get(v1[j]))
                        if (j != i && Position(v1[j], v1[i], threshold))
                        {
                            yield return i;
                            break;
                        }
            }

            public static IEnumerable<int> Positions(Vector3[] v1, Vector3[] v2, float threshold = 0.00001f)
            {
                var partition = new Partition3D<int>(v1, 0.01f);
                for (int i = 0; i < v1.Length; i++)
                    partition.Add(i, v1[i]);

                foreach (var v in v2)
                {
                    // foreach (int i in partition.Get(v))
                    //     if (Position(v, v1[i], threshold))
                    //     {
                    //         yield return i;
                    //         break;
                    //     }
                    var indices = partition.Get(v);
                    if (indices.Count() > 0)
                    {
                        var idx = indices.Select(i => (i, v)).
                            Where(g => Position(g.v, v1[g.i], threshold)).
                            OrderBy(g => (v1[g.i] - g.v).sqrMagnitude);
                        if (idx.Count() > 0) yield return idx.First().i;
                    }
                }
            }
        }

        public static List<Transform> GetDescendants(Transform root)
        {
            var transforms = new List<Transform>();
            void Children(Transform transform)
            {
                transforms.Add(transform);
                foreach (Transform child in transform)
                    Children(child);
            }
            Children(root);
            return transforms;
        }

        public static Matrix4x4[] GetBindPoses(Transform[] bones) =>
            bones.Select(b => b.worldToLocalMatrix).ToArray();

        public static bool RayCast(Ray ray, Mesh mesh, out PointHit hit)
        {
            int[] indices = mesh.triangles;
            Vector3[] vertices = mesh.vertices;

            hit = new PointHit { triangleIndex = -1, distance = 1000 };
            for (int i = 0; i < indices.Length; i += 3)
                if (RayIntersect(ray, vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], out float t) && t < hit.distance)
                    hit = new PointHit { triangleIndex = i, distance = t };

            hit.point = ray.origin + ray.direction * hit.distance;
            return hit.triangleIndex > -1;
        }

        public struct PointHit
        {
            public int triangleIndex;
            public float distance;
            public Vector3 point;
            public Vector3 barycentric;
        }

        public static bool RayIntersect(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out float t)
        {
            t = 0;
            Vector3 N = Vector3.Cross(v1 - v0, v2 - v0);
            float Ndir = Vector3.Dot(N, ray.direction);
            if (Mathf.Abs(Ndir) < Mathf.Epsilon)
                return false;

            t = -(Vector3.Dot(N, ray.origin) - Vector3.Dot(N, v0)) / Ndir;
            if (t < 0) return false;

            Vector3 P = ray.origin + t * ray.direction;

            return !(
                Vector3.Dot(N, Vector3.Cross(v1 - v0, P - v0)) < 0 ||
                Vector3.Dot(N, Vector3.Cross(v2 - v1, P - v1)) < 0 ||
                Vector3.Dot(N, Vector3.Cross(v0 - v2, P - v2)) < 0);
        }

        public static bool PointToTriangle(Vector3 point, Vector3 p0, Vector3 p1, Vector3 p2, out PointHit hit)
        {
            Vector3 diff = p0 - point, edge0 = p1 - p0, edge1 = p2 - p0;
            float a00 = edge0.sqrMagnitude, a01 = Vector3.Dot(edge0, edge1), a11 = edge1.sqrMagnitude;
            float b0 = Vector3.Dot(diff, edge0), b1 = Vector3.Dot(diff, edge1);
            float c = diff.sqrMagnitude;
            float det = Mathf.Abs(a00 * a11 - a01 * a01);
            float s = a01 * b1 - a11 * b0, t = a01 * b0 - a00 * b1;
            float sqrd;

            if (s + t <= det)
            {
                if (s < 0)
                {
                    if (t < 0)
                    {
                        if (b0 < 0)
                        {
                            t = 0;
                            if (-b0 >= a00) { s = 1; sqrd = a00 + 2f * b0 + c; }
                            else { s = -b0 / a00; sqrd = b0 * s + c; }
                        }
                        else
                        {
                            s = 0;
                            if (b1 >= 0) { t = 0; sqrd = c; }
                            else if (-b1 >= a11) { t = 1; sqrd = a11 + 2f * b1 + c; }
                            else { t = -b1 / a11; sqrd = b1 * t + c; }
                        }
                    }
                    else
                    {
                        s = 0;
                        if (b1 >= 0) { t = 0; sqrd = c; }
                        else if (-b1 >= a11) { t = 1; sqrd = a11 + 2f * b1 + c; }
                        else { t = -b1 / a11; sqrd = b1 * t + c; }
                    }
                }
                else if (t < 0)
                {
                    t = 0;
                    if (b0 >= 0) { s = 0; sqrd = c; }
                    else if (-b0 >= a00) { s = 1; sqrd = a00 + 2f * b0 + c; }
                    else { s = -b0 / a00; sqrd = b0 * s + c; }
                }
                else
                {
                    float invDet = (1) / det;
                    s *= invDet;
                    t *= invDet;
                    sqrd = s * (a00 * s + a01 * t + 2f * b0) + t * (a01 * s + a11 * t + 2f * b1) + c;
                }
            }
            else
            {
                if (s < 0)
                {
                    float tmp0 = a01 + b0;
                    float tmp1 = a11 + b1;
                    if (tmp1 > tmp0)
                    {
                        float numer = tmp1 - tmp0;
                        float denom = a00 - 2f * a01 + a11;
                        if (numer >= denom) { s = 1; t = 0; sqrd = a00 + 2f * b0 + c; }
                        else { s = numer / denom; t = 1 - s; sqrd = s * (a00 * s + a01 * t + 2f * b0) + t * (a01 * s + a11 * t + 2f * b1) + c; }
                    }
                    else
                    {
                        s = 0;
                        if (tmp1 <= 0) { t = 1; sqrd = a11 + 2f * b1 + c; }
                        else if (b1 >= 0) { t = 0; sqrd = c; }
                        else { t = -b1 / a11; sqrd = b1 * t + c; }
                    }
                }
                else if (t < 0)
                {
                    float tmp0 = a01 + b1;
                    float tmp1 = a00 + b0;
                    if (tmp1 > tmp0)
                    {
                        float numer = tmp1 - tmp0;
                        float denom = a00 - 2f * a01 + a11;
                        if (numer >= denom) { t = 1; s = 0; sqrd = a11 + 2f * b1 + c; }
                        else { t = numer / denom; s = 1 - t; sqrd = s * (a00 * s + a01 * t + 2f * b0) + t * (a01 * s + a11 * t + 2f * b1) + c; }
                    }
                    else
                    {
                        t = 0;
                        if (tmp1 <= 0) { s = 1; sqrd = a00 + 2f * b0 + c; }
                        else if (b0 >= 0) { s = 0; sqrd = c; }
                        else { s = -b0 / a00; sqrd = b0 * s + c; }
                    }
                }
                else
                {
                    float numer = a11 + b1 - a01 - b0;
                    if (numer <= 0) { s = 0; t = 1; sqrd = a11 + 2f * b1 + c; }
                    else
                    {
                        float denom = a00 - 2f * a01 + a11;
                        if (numer >= denom) { s = 1; t = 0; sqrd = a00 + 2f * b0 + c; }
                        else { s = numer / denom; t = 1 - s; sqrd = s * (a00 * s + a01 * t + 2f * b0) + t * (a01 * s + a11 * t + 2f * b1) + c; }
                    }
                }
            }

            hit = new PointHit
            {
                point = p0 + s * edge0 + t * edge1,
                distance = Mathf.Sqrt(sqrd),
                barycentric = new Vector3(1 - s - t, s, t),
                triangleIndex = -1,
            };
            return true;
        }
    }
}
