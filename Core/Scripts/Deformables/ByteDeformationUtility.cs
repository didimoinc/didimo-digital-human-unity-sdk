using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo
{
    public class ByteDeformationUtility : DeformationUtility
    {
        private byte[] data;

        private static Vector3[] BytesToVector3Array(ref byte[] bytes, int count, ref uint arrayOffset)
        {
            Vector3[] vertexList = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                vertexList[i] = ReadVector3(bytes, ref arrayOffset);
            }

            return vertexList;
        }

        private static void Vector3ArrayToBytes(Vector3[] vertexList, ref byte[] data, ref int position)
        {
            for (int i = 0; i < vertexList.Length; i++)
            {
                WriteVector3(ref data, ref position, vertexList[i]);
            }
        }

        public override void ApplyToMeshFilters(MeshFilter[] meshFilters)
        {
            uint arrayPosition = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (!ValidFilter(meshFilter)) continue;

                Mesh mesh = meshFilter.mesh;

                Vector3[] vertexList = BytesToVector3Array(ref data, mesh.vertexCount, ref arrayPosition);

                mesh.vertices = vertexList;
            }
        }

        public override void FromMeshFilters(MeshFilter[] meshFilters, bool shared = true)
        {
            uint totalLength = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (!ValidFilter(meshFilter)) continue;

                totalLength += Constants.SIZE_VECTOR3 * (uint) (shared ? meshFilter.sharedMesh.vertexCount : meshFilter.mesh.vertexCount);
            }

            data = new byte[totalLength];
            int position = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (!ValidFilter(meshFilter)) continue;

                Vector3ArrayToBytes((shared ? meshFilter.sharedMesh.vertices : meshFilter.mesh.vertices), ref data, ref position);
            }
        }

        public override byte[] Serialize() { return data; }

        public override void Deserialize(byte[] bytes) { data = bytes; }

        public override List<Vector3> GetVertices()
        {
            uint offset = 0;
            int count = (int) (data.Length / Constants.SIZE_VECTOR3);
            return BytesToVector3Array(ref data, count, ref offset).ToList();
        }

        public override void SetVertices(List<Vector3> vertices)
        {
            if (vertices.Count != data.Length / Constants.SIZE_VECTOR3) throw new ArgumentException($"Provided list of vertices doesn't match already existing list size.");
            int position = 0;
            Vector3ArrayToBytes(vertices.ToArray(), ref data, ref position);
        }

        public static Vector3 ReadVector3(byte[] bytes, ref uint pos)
        {
            float x = ReadFloat(bytes, ref pos);
            float y = ReadFloat(bytes, ref pos);
            float z = ReadFloat(bytes, ref pos);

            return PipelineToUnity(new Vector3(x, y, z));
        }

        private static float ReadFloat(byte[] bytes, ref uint pos)
        {
            byte[] buffer = new byte[Constants.SIZE_FLOAT];
            Array.Copy(bytes, pos, buffer, 0, Constants.SIZE_FLOAT);
            pos += Constants.SIZE_FLOAT;
            return ByteToFloat(buffer);
        }

        private static void WriteFloat(ref byte[] array, ref int position, float value)
        {
            byte[] bytes = FloatToByte(value);
            bytes.CopyTo(array, position);
            position += bytes.Length;
        }

        public static void WriteVector3(ref byte[] array, ref int position, Vector3 value)
        {
            value = UnityToPipeline(value);
            WriteFloat(ref array, ref position, value.x);
            WriteFloat(ref array, ref position, value.y);
            WriteFloat(ref array, ref position, value.z);
        }

        private static float ByteToFloat(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }

            return BitConverter.ToSingle(bytes, 0);
        }

        private static byte[] FloatToByte(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // Convert big endian to little endian
            }

            return bytes;
        }
    }
}