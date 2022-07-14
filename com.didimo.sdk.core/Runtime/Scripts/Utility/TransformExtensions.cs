using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace Didimo.Core.Utility
{
    public static class TransformExtensions
    {
        public static Transform[] GetChildrenArray(this Transform transform)
        {
            Transform[] childArray = new Transform[transform.childCount];
            int index = 0;
            foreach (Transform c in transform)
            {
                childArray[index++] = c;
            }
            return childArray;
        }

        //! Extension provided to enable foreach through transform and all decendants, rather than just children.
        public static IEnumerable<Transform> TransformAndAllDecendants(this Transform transform)
        {
            yield return transform;
            foreach (Transform decendant in transform.AllDecendants())
            {
                yield return decendant;
            }

        }

        //! Extension provided to enable foreach through all decendants, rather than just children.
        public static IEnumerable<Transform> AllDecendants(this Transform transform)
        {
            // yield return transform;
            foreach (Transform c in transform)
            {
                yield return c;

                if (c.childCount > 0)
                {
                    foreach (Transform decendant in c.AllDecendants())
                    {
                        yield return decendant;
                    }
                }
            }
        }

        private static Transform currentTransform;
        private static int currentChildIndex;
        private static List<Transform> stack;
        private static List<int> Istack;
        private static List<Transform[]> childArrayStack;
        private static Transform[] currentTransformChildrenArray;
        private static Transform child;
        public static IEnumerable<Transform> AllDecendantsNonRecursive(this Transform transform)
        {
            stack = new List<Transform>();
            Istack = new List<int>();
            childArrayStack = new List<Transform[]>();
            currentTransformChildrenArray = transform.GetChildrenArray();
            currentTransform = transform;
            currentChildIndex = 0;
            while (true)
            {
                if (currentChildIndex < currentTransform.childCount)
                {
                    child = currentTransformChildrenArray[currentChildIndex++];
                    yield return child;
                    if (child.childCount > 0)
                    {
                        stack.Add(currentTransform);
                        Istack.Add(currentChildIndex);
                        childArrayStack.Add(currentTransformChildrenArray);
                        currentTransform = child;
                        currentChildIndex = 0;
                        currentTransformChildrenArray = currentTransform.GetChildrenArray();
                    }//current child has children
                }
                else //reached beyond last child, pop up.
                {
                    //pup up
                    int lastIndex = stack.Count - 1;
                    if (lastIndex >= 0)
                    {
                        currentTransform = stack[lastIndex];
                        currentChildIndex = Istack[lastIndex];
                        currentTransformChildrenArray = childArrayStack[lastIndex];
                        stack.RemoveAt(lastIndex);
                        Istack.RemoveAt(lastIndex);
                        childArrayStack.RemoveAt(lastIndex);
                    }
                    else
                    {
                        // top node, stop popping up: all done
                        yield break;
                    }
                }// if done with all children for current transform


            }// end loop


        }// end AllDecendantsNonRecursive

    }

}