using System.Collections;
using System.Collections.Generic;
using System.Text;
using Didimo.Utils;
using UnityEngine;

using ArrayData = Didimo.Utils.NpyParser.ArrayData;

namespace Didimo.Core.Deformer
{
    public static class DeformerFactory
    {
        public const string NPY_TAG_FILE = "tag.npy";
        public const string ERBF_TAG     = "ERBF";

        /// <summary>
        /// Creates a new adequate Deformer object from the associated deformation file
        /// </summary>
        /// <param name="deformerFilePath">Path to deformer file</param>
        /// <returns>Created Deformer object. Fallback deformer if not supported or invalid.</returns>
        public static Deformer BuildDeformer(string deformerFilePath)
        {
            NpzParser parser = new NpzParser(deformerFilePath);
            return BuildCorrectDeformerFromNpzData(parser);
        }

        /// <summary>
        /// Creates a new adequate Deformer object from the associated deformation file
        /// </summary>
        /// <param name="deformerFile">Deformer file</param>
        /// <returns>Created Deformer object. Fallback deformer if not supported or invalid.</returns>
        public static Deformer BuildDeformer(TextAsset deformerFile)
        {
            NpzParser parser = new NpzParser(deformerFile);
            return BuildCorrectDeformerFromNpzData(parser);
        }

        /// <summary>
        /// Build the fallback deformer that does not perform any deformation.
        /// </summary>
        /// <returns>Fallback default Deformer</returns>
        public static Deformer BuildDefaultDeformer()
        {
            return new Deformer();
        }

        private static Deformer BuildCorrectDeformerFromNpzData(NpzParser parser)
        {
            Dictionary<string, ArrayData> npzData = parser.Parse();

            // Return fallback deformer
            if (!npzData.ContainsKey(NPY_TAG_FILE))
            {
                Debug.LogWarning("Could not find deformer tag. Returning fallback deformer (does nothing) as local deformation is not supported for older didimo versions");
                return new Deformer();
            }

            // Map tag to proper deformer. Currently, there is only ERBF but more can be added in the future
            ArrayData npyTagData = npzData[NPY_TAG_FILE];
            string deformerTag = npyTagData.AsString();
            switch (deformerTag)
            {
                case ERBF_TAG:
                    return new DeformerErbf(npzData);
                default:
                    Debug.LogWarning($"Unsupported deformation tag {deformerTag}. Returning fallback deformer (does nothing)");
                    return BuildDefaultDeformer();
            }
        }
    }
}