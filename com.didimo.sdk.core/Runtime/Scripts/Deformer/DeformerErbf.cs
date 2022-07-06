using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Utility;
using Didimo.Utils;
using UnityEngine;
using ArrayData = Didimo.Utils.NpyParser.ArrayData;

namespace Didimo.Core.Deformer
{
    /// <summary>
    /// ERBF deformation model
    /// sourceVertices: set of Nx3 shaped matrix with the control points
    /// coefficients: will be an Nx3 shaped matrix for coefficients associated to the control points
    /// sigma: scalar positive value
    /// gamma: scalar positive value
    /// </summary>
    public class DeformerErbf : Deformer
    {
        public const string SOURCE_VERTICES_NPY_FILE = "source_points.npy";
        public const string COEFFICIENTS_NPY_FILE    = "coef.npy";
        public const string SIGMA_NPY_FILE           = "sigma.npy";
        public const string GAMMA_NPY_FILE           = "gamma.npy";

        protected float[,] sourceVertices;
        protected float[,] coefficients;
        protected float    sigma;
        protected float    gamma;

        public DeformerErbf(string deformationFilePath)
        {
            NpzParser npzParser = new NpzParser(deformationFilePath);
            Dictionary<string, ArrayData> npzData = npzParser.Parse();
            BuildDeformerArrays(npzData);
        }

        public DeformerErbf(TextAsset deformationFile)
        {
            NpzParser npzParser = new NpzParser(deformationFile);
            Dictionary<string, ArrayData> npzData = npzParser.Parse();
            BuildDeformerArrays(npzData);
        }

        public DeformerErbf(Dictionary<string, ArrayData> npzData)
        {
            BuildDeformerArrays(npzData);
        }

        public DeformerErbf(ArrayData sourceVertices, ArrayData coefficients, ArrayData sigma, ArrayData gamma)
        {
            BuildDeformerArrays(sourceVertices, coefficients, sigma, gamma);
        }

        public DeformerErbf(float[,] sourceVertices, float[,] coefficients, float sigma, float gamma)
        {
            this.sourceVertices = sourceVertices;
            this.coefficients = coefficients;
            this.sigma = sigma;
            this.gamma = gamma;
        }



        protected void BuildDeformerArrays(Dictionary<string, ArrayData> npzData)
        {
            ArrayData sourceVerticesData = npzData[SOURCE_VERTICES_NPY_FILE];
            ArrayData coefficientsData = npzData[COEFFICIENTS_NPY_FILE];
            ArrayData sigmaData = npzData[SIGMA_NPY_FILE];
            ArrayData gammaData = npzData[GAMMA_NPY_FILE];

            BuildDeformerArrays(sourceVerticesData, coefficientsData, sigmaData, gammaData);
        }

        protected void BuildDeformerArrays(ArrayData sourceVerticesData, ArrayData coefficientsData, ArrayData sigmaData, ArrayData gammaData)
        {
            sourceVertices = sourceVerticesData.AsArray<float>().To2DArray(sourceVerticesData.Info.Shape[0], sourceVerticesData.Info.Shape[1], sourceVerticesData.Info.IsFortranOrder);
            coefficients = coefficientsData.AsArray<float>().To2DArray(coefficientsData.Info.Shape[0], coefficientsData.Info.Shape[1], coefficientsData.Info.IsFortranOrder);
            sigma = (float) sigmaData.AsArray<double>()[0];
            gamma = (float) gammaData.AsArray<double>()[0];
        }

        public override Vector3[] DeformVertices(IEnumerable<Vector3> deformableVertices)
        {
            // Note: Source Vertices come in a different coordinate system
            Vector3[] transformedDeformableVertices = TransformVertexCoordinates(deformableVertices).ToArray();
            float sigma2 = sigma * sigma;
            float gamma2 = gamma / 2f;

            float[,] expKernelMatrix = new float[transformedDeformableVertices.GetLength(0), sourceVertices.GetLength(0)];
            for (int col = 0; col < expKernelMatrix.GetLength(1); col++)
            {
                for (int row = 0; row < expKernelMatrix.GetLength(0); row++)
                {
                    float sqrdDistance = Mathf.Pow(transformedDeformableVertices[row].x - sourceVertices[col, 0], 2) + Mathf.Pow(transformedDeformableVertices[row].y - sourceVertices[col, 1], 2) + Mathf.Pow(transformedDeformableVertices[row].z - sourceVertices[col, 2], 2);

                    expKernelMatrix[row, col] = Mathf.Exp(-Mathf.Pow(sqrdDistance / sigma2, gamma2));
                }
            }


            float[,] deformationDelta = DotProduct(expKernelMatrix, coefficients);

            List<Vector3> deformedVertices = new List<Vector3>(transformedDeformableVertices.Length);
            for (int vertexIndex = 0; vertexIndex < transformedDeformableVertices.Length; vertexIndex++)
            {
                deformedVertices.Add(new Vector3(transformedDeformableVertices[vertexIndex].x + deformationDelta[vertexIndex, 0], transformedDeformableVertices[vertexIndex].y + deformationDelta[vertexIndex, 1], transformedDeformableVertices[vertexIndex].z + deformationDelta[vertexIndex, 2]));
            }

            return RevertVertexCoordinates(deformedVertices).ToArray();
        }


        protected static float[,] DotProduct(float[,] matrixA, float[,] matrixB)
        {
            float[,] result = new float[matrixA.GetLength(0), matrixB.GetLength(1)];

            for (int matrixARow = 0; matrixARow < matrixA.GetLength(0); matrixARow++)
            {
                for (int matrixBCol = 0; matrixBCol < matrixB.GetLength(1); matrixBCol++)
                {
                    for (int matrixACol = 0; matrixACol < matrixA.GetLength(1); matrixACol++)
                    {
                        result[matrixARow, matrixBCol] += matrixA[matrixARow, matrixACol] * matrixB[matrixACol, matrixBCol];
                    }
                }
            }

            return result;
        }
    }
}