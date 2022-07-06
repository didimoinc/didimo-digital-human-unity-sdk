using System;

namespace Didimo.Core.Utility
{
    public static class ArrayUtility
    {
        public static T[,] To2DArray<T>(this T[] array, int sizeX, int sizeY, bool usingFortranOrder=false)
        {
            T[,] result = new T[sizeX, sizeY];

            // Already in C order, we can do a direct memory block copy
            if (!usingFortranOrder)
            {
                int byteCount = Buffer.ByteLength(array);
                Buffer.BlockCopy(array, 0, result, 0, byteCount);
            }

            else  // transpose, because Fortran is column major
            {
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        result[i, j] = array[j * sizeX + i];
                    }
                }
            }
            return result;

        }
    }
}