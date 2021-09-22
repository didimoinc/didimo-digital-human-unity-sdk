using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Didimo
{
    public class Deformable : DidimoBehaviour
    {
        [Header("Deformable")]
        [SerializeField]
        protected string[] idealBoneNames;

        public string ID => name;

        public virtual bool SingleInstancePerDidimo => false;
        public string[] IdealBoneNames => idealBoneNames;

        public Type DeformationUtilityType = typeof(ObjDeformationUtility);
        public DeformationUtility GetDeformationUtility() { return (DeformationUtility) Activator.CreateInstance(DeformationUtilityType); }

        /// <summary>
        /// Get original, undeformed mesh data, from the shared mesh(s). Coordinates will be converted to the units used by the Didimo generation pipeline.
        /// </summary>
        /// <returns>Byte array of the data ready to be sent to the server.</returns>
        public byte[] GetUndeformedMeshData()
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            DeformationUtility deformationUtility = GetDeformationUtility();
            deformationUtility.FromMeshFilters(meshFilters, true);
            return deformationUtility.Serialize();
        }

        /// <summary>
        /// Get deformed mesh data, from the mesh instance(s). Coordinates will be converted to the units used by the Didimo generation pipeline.
        /// </summary>
        /// <returns>Byte array of the data ready to be sent to the server.</returns>
        public byte[] GetMeshData()
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            DeformationUtility deformationUtility = GetDeformationUtility();
            deformationUtility.FromMeshFilters(meshFilters, false);
            return deformationUtility.Serialize();
        }

        /// <summary>
        /// Update the vertices of the mesh. Coordinates will be automatically converted from the ones used by the Didimo generation pipeline, into the the ones used by Unity.
        /// </summary>
        /// <param name="data"></param>
        public void SetDeformedMeshData(byte[] data)
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            DeformationUtility deformationUtility = GetDeformationUtility();
            deformationUtility.Deserialize(data);
            deformationUtility.ApplyToMeshFilters(meshFilters);
        }
    }
}