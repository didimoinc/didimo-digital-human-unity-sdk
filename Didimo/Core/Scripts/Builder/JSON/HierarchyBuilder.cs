using System;
using UnityEngine;

namespace Didimo.Builder.JSON
{
    public class HierarchyBuilder
    {
        private float unitsPerMeter;

        public bool TryBuild(DidimoBuildContext context, DidimoModelDataObject data)
        {
            unitsPerMeter = data.unitsPerMeter;
            context.MeshHierarchyRoot = BuildNode(data.RootNode, context.RootTransform);
            return true;
        }

        public void UpdateHierarchy(DidimoModelDataObject.Node node, Transform parent)
        {
            GameObject go = parent.Find(node.Name).gameObject;
            if (go == null)
            {
                throw new Exception("Failed to get transform named " + node.Name + ", when creating the didimo hierarchy.");
            }

            SetTransformValues(node, go.transform);

            if (node.Children != null)
            {
                foreach (DidimoModelDataObject.Node child in node.Children)
                {
                    UpdateHierarchy(child, go.transform);
                }
            }
        }

        protected void SetTransformValues(DidimoModelDataObject.Node node, Transform t)
        {
            t.localPosition = node.Position / unitsPerMeter;
            t.localRotation = node.Rotation;
            t.localScale = node.Scale;
        }

        private Transform BuildNode(DidimoModelDataObject.Node node, Transform parent)
        {
            GameObject go = new GameObject();
            go.name = node.Name;
            go.transform.parent = parent;

            SetTransformValues(node, go.transform);

            if (node.Children != null)
            {
                foreach (DidimoModelDataObject.Node child in node.Children)
                {
                    BuildNode(child, go.transform);
                }
            }

            return go.transform;
        }
    }
}