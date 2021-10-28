using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Didimo
{
    public abstract class DeformationUtility
    {
        public static class Constants
        {
            public const uint  SIZE_FLOAT        = 4;
            public const float UNITY_TO_PIPELINE = 100;
            public const float PIPELINE_TO_UNITY = 0.01f;
            public const uint  SIZE_VECTOR3      = SIZE_FLOAT * 3;
        }

        public abstract void ApplyToMeshFilters(MeshFilter[] meshFilters);

        public abstract void FromMeshFilters(MeshFilter[] meshFilters, bool shared = true);

        public abstract byte[] Serialize();

        public abstract void Deserialize(byte[] data);

        public abstract List<Vector3> GetVertices();
        public abstract void SetVertices(List<Vector3> vertices);

        public static bool ValidFilter(MeshFilter meshFilter) => meshFilter != null && meshFilter.sharedMesh != null;

        public static Vector3 UnityToPipeline(Vector3 unityVector, bool scale = true)
        {
            Vector3 result = Vector3.Scale(unityVector, new Vector3(-1f, 1f, 1f));
            if (scale)
            {
                result *= Constants.UNITY_TO_PIPELINE;
            }

            return result;
        }

        public static Vector3 PipelineToUnity(Vector3 unityVector, bool scale = true)
        {
            Vector3 result = Vector3.Scale(unityVector, new Vector3(-1f, 1f, 1f));
            if (scale)
            {
                result *= Constants.PIPELINE_TO_UNITY;
            }

            return result;
        }

        protected static List<Vector3> UnityToPipeline(List<Vector3> vertices)
        {
            List<Vector3> result = new List<Vector3>(vertices);
            for (int i = 0; i < vertices.Count; i++)
            {
                result[i] = DeformationUtility.UnityToPipeline(vertices[i]);
            }

            return result;
        }

        protected static List<Vector3> PipelineToUnity(List<Vector3> vertices)
        {
            List<Vector3> result = new List<Vector3>(vertices);
            for (int i = 0; i < vertices.Count; i++)
            {
                result[i] = DeformationUtility.PipelineToUnity(vertices[i]);
            }

            return result;
        }
    }
}