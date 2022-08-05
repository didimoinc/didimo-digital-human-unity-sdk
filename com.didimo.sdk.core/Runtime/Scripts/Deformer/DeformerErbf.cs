using System;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Utility;
using Didimo.Utils;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
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

        public const int JOB_SYSTEM_INNER_LOOP_BATCH_COUNT = 64;

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

        /// <summary>
        /// Compute the vertex deformation from the given deformation file.
        /// </summary>
        /// <param name="deformableVertices">Vertices to deform</param>
        /// <returns>Array with the new positions of the deformed vertices.</returns>
        public override Vector3[] DeformVertices(IEnumerable<Vector3> deformableVertices)
        {
            return DeformVerticesParallel(deformableVertices);
        }

        public override bool IsAbleToDeform() => true;

        /// <summary>
        /// Compute the vertex deformation from the given deformation file.
        /// It uses Unity's Job and Burst compile systems.
        /// </summary>
        /// <param name="deformableVertices">Vertices to deform</param>
        /// <returns>Array with the new positions of the deformed vertices.</returns>
        public Vector3[] DeformVerticesParallel(IEnumerable<Vector3> deformableVertices)
        {
            // Convert Coordinate system of vertices
            NativeArray<Vector3> deformableVerticesArray = new NativeArray<Vector3>(deformableVertices.ToArray(), Allocator.TempJob);
            TransformVertexCoordinatesJob transformVertexCoordinatesJob = new()
            {
                Positions = deformableVerticesArray,
                Scale = SCALE
            };
            JobHandle transformVertexCoordinatesJobHandle = transformVertexCoordinatesJob.Schedule(deformableVerticesArray.Length, JOB_SYSTEM_INNER_LOOP_BATCH_COUNT);

            // Compute ERBF Kernel
            float[] sourceVertices1D = new float[sourceVertices.GetLength(0) * sourceVertices.GetLength(1)];
            Buffer.BlockCopy(sourceVertices, 0, sourceVertices1D, 0, Buffer.ByteLength(sourceVertices1D));
            NativeArray<float> sourceVerticesArray = new NativeArray<float>(sourceVertices1D, Allocator.TempJob);
            NativeArray<float> expKernelMatrix = new NativeArray<float>( deformableVerticesArray.Length * sourceVertices.GetLength(0), Allocator.TempJob);

            ComputeExpKernelMatrixJob computeExpKernelMatrixJob = new()
            {
                DeformableVertices = deformableVerticesArray,
                SourceVertices = sourceVerticesArray,
                ExpKernelMatrix = expKernelMatrix,

                Sigma2 = sigma * sigma,
                Gamma2 = gamma / 2f
            };
            JobHandle computeExpKernelMatrixJobHandle = computeExpKernelMatrixJob. Schedule(expKernelMatrix.Length, JOB_SYSTEM_INNER_LOOP_BATCH_COUNT, transformVertexCoordinatesJobHandle);

            // Compute deformation deltas
            float[] coefficients1D = new float[coefficients.GetLength(0) * coefficients.GetLength(1)];
            Buffer.BlockCopy(coefficients, 0, coefficients1D, 0, Buffer.ByteLength(coefficients1D));
            NativeArray<float> coefficientsMatrix = new NativeArray<float>(coefficients1D, Allocator.TempJob);
            NativeArray<float> deformationDeltas = new NativeArray<float>(deformableVerticesArray.Length * 3, Allocator.TempJob);

            MatrixMultiplicationJob matrixMultiplicationJob = new()
            {
                MatrixA = expKernelMatrix,
                MatrixB = coefficientsMatrix,
                MatrixC = deformationDeltas,

                MatrixACols = sourceVertices.GetLength(0),
                MatrixBCols = coefficients.GetLength(1)
            };
            JobHandle matrixMultiplicationJobHandle = matrixMultiplicationJob.Schedule(deformationDeltas.Length, JOB_SYSTEM_INNER_LOOP_BATCH_COUNT, computeExpKernelMatrixJobHandle);

            // Apply Deformation Deltas
            AddDeformationToVerticesJob addDeformationToVerticesJob = new()
            {
                DeformableVertices = deformableVerticesArray,
                DeformationDeltas = deformationDeltas
            };
            JobHandle addDeformationToVerticesJobHandle = addDeformationToVerticesJob.Schedule(deformableVerticesArray.Length, JOB_SYSTEM_INNER_LOOP_BATCH_COUNT, matrixMultiplicationJobHandle);

            // Revert to Unity Coordinate System
            UndoTransformVertexCoordinatesJob undoTransformVertexCoordinatesJob = new()
            {
                Positions = deformableVerticesArray,
                Scale = SCALE
            };
            JobHandle undoTransformVertexCoordinatesJobHandle = undoTransformVertexCoordinatesJob.Schedule(deformableVerticesArray.Length, JOB_SYSTEM_INNER_LOOP_BATCH_COUNT, addDeformationToVerticesJobHandle);

            undoTransformVertexCoordinatesJobHandle.Complete();
            Vector3[] deformedVertices = deformableVerticesArray.ToArray();

            deformableVerticesArray.Dispose();
            sourceVerticesArray.Dispose();
            expKernelMatrix.Dispose();
            coefficientsMatrix.Dispose();
            deformationDeltas.Dispose();

            return deformedVertices;
        }


        [BurstCompile]
        public struct MatrixMultiplicationJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> MatrixA;
            [ReadOnly] public NativeArray<float> MatrixB;

            public int MatrixACols;  // Same as MatrixBRows to allow multiplication
            public int MatrixBCols;

            [WriteOnly] public NativeArray<float> MatrixC; // dims: (MatrixARows x MatrixBCols)


            public void Execute(int index)
            {
                // c_ij = sum_k a_ik * b_kj

                // initial indices are a_i0, b_0j (start of ith row and jth col)
                int matrixAIndex = index / MatrixBCols * MatrixACols;
                int matrixBIndex = index % MatrixBCols;

                float result = 0;
                for (int k = 0; k < MatrixACols; k++)
                {
                    result += MatrixA[matrixAIndex] * MatrixB[matrixBIndex];

                    // Update indices. a -> goes to next element on the same row of matrix. b goes to the next row of matrix.
                    matrixAIndex += 1;
                    matrixBIndex += MatrixBCols;
                }

                MatrixC[index] = result;
            }
        }


        [BurstCompile]
        public struct TransformVertexCoordinatesJob : IJobParallelFor
        {
            public NativeArray<Vector3> Positions;

            public float Scale;

            public void Execute(int index)
            {
                Positions[index] = new Vector3(-Positions[index].x, Positions[index].y, Positions[index].z) * Scale;
            }
        }

        [BurstCompile]
        public struct UndoTransformVertexCoordinatesJob : IJobParallelFor
        {
            public NativeArray<Vector3> Positions;

            public float Scale;

            public void Execute(int index)
            {
                Positions[index] = new Vector3(-Positions[index].x, Positions[index].y, Positions[index].z) / Scale;
            }
        }

        [BurstCompile]
        public struct AddDeformationToVerticesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> DeformationDeltas;

            public NativeArray<Vector3> DeformableVertices;

            public void Execute(int index)
            {
                NativeSlice<float> deformationDelta = DeformationDeltas.Slice(index * 3, 3);

                DeformableVertices[index] = new Vector3(
                    DeformableVertices[index].x + deformationDelta[0],
                    DeformableVertices[index].y + deformationDelta[1],
                    DeformableVertices[index].z + deformationDelta[2]);
            }
        }


        [BurstCompile]
        public struct ComputeExpKernelMatrixJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> DeformableVertices;
            [ReadOnly] public NativeArray<float>   SourceVertices;  // Vector3s

            [WriteOnly] public NativeArray<float> ExpKernelMatrix;  // output

            public float Sigma2; // sigma * sigma
            public float Gamma2; // gamma / 2f


            // res[i,j] = e^(-(||deformable_i - source_j||)^gamma / sigma)
            //          = e^(-(||deformable_i - source_j||^2 / sigma^2)^(gamma/2))
            public void Execute(int index)
            {
                int sourceVertexCount = SourceVertices.Length / 3;

                int deformableVertexIndex = index / sourceVertexCount; // row
                int sourceVertexIndex = index % sourceVertexCount; // col

                Vector3 deformableVertex = DeformableVertices[deformableVertexIndex];
                NativeSlice<float> sourceVertex = SourceVertices.Slice(3 * sourceVertexIndex, 3);

                float distX = deformableVertex.x - sourceVertex[0];
                float distY = deformableVertex.y - sourceVertex[1];
                float distZ = deformableVertex.z - sourceVertex[2];
                float sqrdDistance = distX * distX + distY * distY + distZ * distZ;

                ExpKernelMatrix[index] = Mathf.Exp(-Mathf.Pow(sqrdDistance / Sigma2, Gamma2));
            }
        }


        /// <summary>
        /// Compute the vertex deformation from the given deformation file.
        /// The computation is done single threaded, which may block the application for a while.
        /// </summary>
        /// <param name="deformableVertices">Vertices to deform</param>
        /// <returns>Array with the new positions of the deformed vertices.</returns>
        public Vector3[] DeformVerticesSingleThreaded(IEnumerable<Vector3> deformableVertices)
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