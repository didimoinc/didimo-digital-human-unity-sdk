using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Didimo.Builder.JSON
{
    public class ConstraintBuilder
    {
        public bool TryBuild(DidimoBuildContext context, DidimoModelDataObject dataObject)
        {
            foreach (DidimoModelDataObject.Constraint constraint in dataObject.Constraints)
            {
                if (!context.MeshHierarchyRoot.TryFindRecursive(constraint.ConstrainedObj, out Transform constrainedObj))
                {
                    Debug.LogWarning($"Could not find hierarchy: {constraint.ConstrainedObj}");
                    continue;
                }

                ConstraintSource constraintSource = new ConstraintSource();

                if (!context.MeshHierarchyRoot.TryFindRecursive(constraint.ConstraintSrc, out Transform constraintSrcObj))
                {
                    Debug.LogWarning($"Could not find hierarchy: {constraint.ConstraintSrc}");
                    continue;
                }

                constraintSource.sourceTransform = constraintSrcObj;
                constraintSource.weight = 1;

                switch (constraint.Type)
                {
                    case "parent":
                    {
                        ParentConstraint unityConstraint = null;
                        unityConstraint = constrainedObj.gameObject.AddComponent<ParentConstraint>();

                        unityConstraint.weight = 1;

                        Vector3 positionOffset = constraintSource.sourceTransform.InverseTransformPoint(constrainedObj.position);
                        Quaternion rotationOffset = Quaternion.Inverse(constraintSource.sourceTransform.rotation) * constrainedObj.rotation;

                        List<ConstraintSource> sources = new List<ConstraintSource>();
                        sources.Add(constraintSource);
                        unityConstraint.SetSources(sources);

                        unityConstraint.SetTranslationOffset(0, positionOffset);
                        unityConstraint.SetRotationOffset(0, rotationOffset.eulerAngles);

                        unityConstraint.constraintActive = true;
                        unityConstraint.locked = true;
                    }
                        break;
                    case "position":
                    {
                        PositionConstraint unityConstraint = null;
                        unityConstraint = constrainedObj.gameObject.AddComponent<PositionConstraint>();

                        unityConstraint.weight = 1;

                        Vector3 positionOffset = constraintSource.sourceTransform.InverseTransformPoint(constrainedObj.position);

                        List<ConstraintSource> sources = new List<ConstraintSource>();
                        sources.Add(constraintSource);
                        unityConstraint.SetSources(sources);

                        unityConstraint.translationOffset = positionOffset;

                        unityConstraint.constraintActive = true;
                        unityConstraint.locked = true;
                    }
                        break;
                    default:
                        Debug.LogError("Constraint type not supported yet: " + constraint.Type);
                        break;
                }
            }

            return true;
        }
    }
}