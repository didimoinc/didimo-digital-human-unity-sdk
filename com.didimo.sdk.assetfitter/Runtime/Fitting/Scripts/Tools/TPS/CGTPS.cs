using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class CGTPS
    {
        private IntPtr tps;

        float[] ToArray(List<Vector3> source)
        {
            float[] result = new float[source.Count * 3];

            for (int i = 0; i < source.Count; i++)
            {
                result[i * 3] = source[i].x;
                result[(i * 3) + 1] = source[i].y;
                result[(i * 3) + 2] = source[i].z;
            }

            return result;
        }

        List<Vector3> ToVectorList(float[] source)
        {
            List<Vector3> result = new List<Vector3>(source.Length / 3);

            for (int i = 0; i < source.Length / 3; i++)
            {
                Vector3 vertex = new Vector3(source[i * 3], source[(i * 3) + 1], source[(i * 3) + 2]);
                result.Add(vertex);
            }

            return result;
        }

        public CGTPS(byte[] serializedData)
        {
            GCHandle handle = GCHandle.Alloc(serializedData, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();
            tps = Deserialize(pointer);

            handle.Free();
        }

        public byte[] Serialize()
        {
            int serializedDataSize = SerializeTPS(tps, out IntPtr ptr);

            byte[] result = new byte[serializedDataSize];
            Marshal.Copy(ptr, result, 0, serializedDataSize);

            Free(ptr);
            return result;
        }

        public CGTPS(List<Vector3> sourceVertices, List<Vector3> targetVertices, double damping = 0.001)
        {
            if (sourceVertices.Count != targetVertices.Count)
            {
                throw new ArgumentException("Input arrays must have the same size.");
            }

            float[] source = ToArray(sourceVertices);
            float[] target = ToArray(targetVertices);
            tps = CreateTPSAndComputeWMatrix(source, target, targetVertices.Count, damping);
        }

        public List<Vector3> TransformVertices(List<Vector3> source)
        {
            float[] floatArray = ToArray(source);
            float[] result = new float[floatArray.Length];

            GCHandle pinnedArray = GCHandle.Alloc(floatArray, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            GCHandle pinnedArrayResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr pointerResult = pinnedArrayResult.AddrOfPinnedObject();

            Transform(tps, pointer, source.Count, pointerResult);
            pinnedArray.Free();

            Marshal.Copy(pointerResult, result, 0, floatArray.Length);
            pinnedArrayResult.Free();

            return ToVectorList(result);
        }

        ~CGTPS() { Free(tps); }

        [DllImport("TPS")]
        private static extern void Transform(IntPtr tps, IntPtr source, int numVertices, IntPtr result);

        [DllImport("TPS")]
        private static extern void Free(IntPtr ptr);

        [DllImport("TPS")]
        private static extern IntPtr CreateTPSAndComputeWMatrix(float[] origin, float[] target, int numVertices, double damping = 0.0001);

        [DllImport("TPS", EntryPoint = "Serialize")]
        private static extern int SerializeTPS(IntPtr ptr, out IntPtr data);

        [DllImport("TPS")]
        private static extern IntPtr Deserialize(IntPtr ptr);
    }
}

