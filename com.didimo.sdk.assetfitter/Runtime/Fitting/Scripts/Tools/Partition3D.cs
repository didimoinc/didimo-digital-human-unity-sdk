using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Didimo.AssetFitter.Editor.Graph
{
    // Cubed Partition
    public class Partition3D<TObject>
    {
        public Dictionary<int3, List<TObject>> cells;

        Bounds volume;
        Vector3 origin;
        float size;

        public Partition3D(Vector3[] positions, float size) : this(GetVolume(positions), size) { }
        public Partition3D(Bounds volume, float cellSize)
        {
            this.volume = volume;
            this.size = cellSize;
            this.cells = new Dictionary<int3, List<TObject>>();

            origin = volume.min;

            // var divisions = volume.size / this.size;
            // Debug.Log("Partition divisions: " + divisions + " " +
            //     Mathf.CeilToInt(divisions.x) * Mathf.CeilToInt(divisions.y) * Mathf.CeilToInt(divisions.z));
        }

        public int3 GetIndex(Vector3 position)
        {
            return (int3)math.floor((position - origin) / size);
            // return new int3(
            //    (int)math.floor((position.x - origin.x) / size),
            //    (int)math.floor((position.y - origin.y) / size),
            //   (int)math.floor((position.z - origin.z) / size));
        }
        Vector3 GetPosition(int3 index) => (Vector3)((float3)index * size) + origin;

        public void Add(TObject obj, Vector3 position)
        {
            int3 x = GetIndex(position);
            if (cells.ContainsKey(x)) cells[x].Add(obj);
            else cells[x] = new List<TObject>() { obj };
        }

        public void Add(TObject obj, Bounds volume)
        {
            int3 min = GetIndex(volume.min), max = GetIndex(volume.max);
            for (int3 z = min; z.z <= max.z; z.z++)
                for (int3 y = z; y.y <= max.y; y.y++)
                    for (int3 x = y; x.x <= max.x; x.x++)
                        if (cells.ContainsKey(x)) cells[x].Add(obj);
                        else cells[x] = new List<TObject>() { obj };
        }

        public IEnumerable<TObject> Get(Vector3 position) =>
            Get(new Bounds(position, Vector3.one * size / 2));

        public IEnumerable<TObject> Get(Bounds volume)
        {
            HashSet<TObject> duplicate = new HashSet<TObject>();
            int3 min = GetIndex(volume.min), max = GetIndex(volume.max);
            for (int3 z = min; z.z <= max.z; z.z++)
                for (int3 y = z; y.y <= max.y; y.y++)
                    for (int3 x = y; x.x <= max.x; x.x++)
                        if (cells.ContainsKey(x))
                            foreach (TObject o in cells[x])
                                if (!duplicate.Contains(o))
                                {
                                    duplicate.Add(o);
                                    yield return o;
                                }
        }

        public static Bounds GetVolume(params Vector3[] positions)
        {
            Vector3 min = positions[0], max = min;
            foreach (var p in positions)
            {
                if (p.x < min.x) min.x = p.x; else if (p.x > max.x) max.x = p.x;
                if (p.y < min.y) min.y = p.y; else if (p.y > max.y) max.y = p.y;
                if (p.z < min.z) min.z = p.z; else if (p.z > max.z) max.z = p.y;
            }
            return new Bounds((max + min) / 2, (max - min) / 2);
        }


    }
}


// Dictionary<float, Dictionary<float, Dictionary<float, int>>> data = new Dictionary<float, Dictionary<float, Dictionary<float, int>>>();
// public Vector3Partition(Vector3[] vts)
// {
//     for (var i = 0; i < vts.Length; i++)
//     {
//         Vector3 v = vts[i];

//         if (!data.TryGetValue(v.x, out Dictionary<float, Dictionary<float, int>> y))
//             data.Add(v.x, y = new Dictionary<float, Dictionary<float, int>>());

//         if (!y.TryGetValue(v.y, out Dictionary<float, int> z))
//             y.Add(v.y, z = new Dictionary<float, int>());

//         if (!z.TryGetValue(v.z, out int index))
//             z[v.z] = i;
//     }
// }

// public int[] remap(Vector3[] vts)
// {
//     for (var i = 0; i < vts.Length; i++)
//     {
//         Vector3 v = vts[i];

//         if (data.TryGetValue(v.x, out Dictionary<float, Dictionary<float, int>> y))
//             if (y.TryGetValue(v.y, out Dictionary<float, int> z))
//                 if (z.TryGetValue(v.z, out int index))

//                     if (!y.TryGetValue(v.y, out Dictionary<float, int> z))
//                         y.Add(v.y, z = new Dictionary<float, int>());

//         if (!z.TryGetValue(v.z, out int index))
//             z[v.z] = i;
//     }
// }

// // Vector3 Lookup
// public class Vector3Partition
// {
//     Dictionary<string, int> data = new Dictionary<string, int>();
//     public Vector3Partition(Vector3[] vts)
//     {
//         for (var i = 0; i < vts.Length; i++)
//         {
//             string k = key(vts[i]);
//             if (data.TryGetValue(k, out int index)) throw new System.Exception("Vertex cannot be duplicated!");
//             data[k] = i;
//         }
//     }

//     string key(Vector3 v) => v.x + "_" + v.y + "_" + v.z;

//     public int[] getRemap(Vector3[] vts)
//     {
//         if (vts.Length != data.Count) throw new System.Exception("Vertex count doesn't match!");

//         var result = new int[data.Count];
//         for (var i = 0; i < vts.Length; i++)
//         {
//             if (i == 4)
//                 Debug.Log(i);
//             if (!data.ContainsKey(key(vts[i])))
//                 throw new System.Exception("Vertex is not found " + vts[i] + " (" + i + ")!");
//             result[i] = data[key(vts[i])];
//         }

//         return result;
//     }
// }