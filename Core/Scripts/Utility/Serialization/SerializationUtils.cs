using System;
using System.Collections;
using System.Collections.Generic;

namespace Didimo
{
    public class SerializationUtils
    {
        public static void FlattenList(IList list, Type elementType, out IList flattenedList)
        {
            Type listType = typeof(List<>).MakeGenericType(typeof(object));
            flattenedList = (IList) Activator.CreateInstance(listType);

            FlattenList(list, ref flattenedList);
        }

        /// <summary>
        /// Get the dimensions of a list. Assumes the list was created from a multidimentional array.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The dimensions of the list.</returns>
        public static List<int> GetDimensionsOfList(IList list)
        {
            List<int> dimensions = new List<int>();

            IList currentList = list;
            do
            {
                dimensions.Add(currentList.Count);
                bool nextIsList = typeof(IList).IsAssignableFrom(currentList[0].GetType());
                currentList = nextIsList ? (IList) currentList[0] : null;
            }
            while (currentList != null);

            return dimensions;
        }

        public static List<int> GetDimensionsOfArray(Array array)
        {
            List<int> dimensions = new List<int>();
            for (int rank = 0; rank < array.Rank; rank++)
            {
                dimensions.Add(array.GetLength(rank));
            }

            return dimensions;
        }

        public static void UpdateAtIndex(List<int> indexPath, ref IList list, object value)
        {
            IList theListToUpdate = list;

            for (int i = 0; i < indexPath.Count; i++)
            {
                if (i == indexPath.Count - 1)
                {
                    theListToUpdate[indexPath[i]] = value;
                }
                else
                {
                    theListToUpdate = (IList) theListToUpdate[indexPath[i]];
                }
            }
        }

        public static void IncrementIndexPath(ref List<int> indexPath, List<int> dimensions)
        {
            int index = indexPath.Count - 1;

            while (true)
            {
                if (index < 0)
                {
                    return;
                }

                indexPath[index]++;
                if (indexPath[index] >= dimensions[index])
                {
                    indexPath[index] = 0;
                    index--;
                }
                else { return; }
            }
        }

        public static List<int> CreateIndexPathForArray(Array array)
        {
            List<int> indexPath = new List<int>(array.Rank);
            for (int i = 0; i < array.Rank; i++)
            {
                indexPath.Add(0);
            }

            return indexPath;
        }

        public static IList ConvertMultiDimensionalArrayIntoList(Array array)
        {
            Type elementType = array.GetType().GetElementType();
            List<int> dimensions = GetDimensionsOfArray(array);
            IList theList = CreateListWithDimensions(dimensions, elementType);
            List<int> indexPath = CreateIndexPathForArray(array);

            foreach (object obj in array)
            {
                UpdateAtIndex(indexPath, ref theList, obj);
                IncrementIndexPath(ref indexPath, dimensions);
            }

            return theList;
        }

        public static IList CreateListWithDimensions(List<int> dimensions, Type leafType)
        {
            if (dimensions.Count == 1)
            {
                Type type = typeof(List<>).MakeGenericType(leafType);
                IList leafList = (IList) Activator.CreateInstance(type);

                for (int i = 0; i < dimensions[0]; i++)
                {
                    leafList.Add(Activator.CreateInstance(leafType));
                }

                return leafList;
            }

            IList list = new List<IList>();

            int dimensionLength = dimensions[0];

            dimensions = new List<int>(dimensions);
            dimensions.RemoveAt(0);

            for (int dimension = 0; dimension < dimensionLength; dimension++)
            {
                list.Add(CreateListWithDimensions(dimensions, leafType));
            }

            return list;
        }

        private static void FlattenList(IList list, ref IList flattenedList)
        {
            foreach (object obj in list)
            {
                if (typeof(IList).IsAssignableFrom(obj.GetType()))
                {
                    FlattenList((IList) obj, ref flattenedList);
                }
                else
                {
                    flattenedList.Add(obj);
                }
            }
        }
    }
}