using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using UnityEngine;

public class UICurvedLayout : MonoBehaviour
{
    [SerializeField] float radius = 1;
    [SerializeField] float startAngle = -90;
    [SerializeField] float endAngle = 90;
    [SerializeField] List<Transform> children;

    [Button]
    private void Layout()
    {
        if (children == null || children.Count == 0) return;
        children.ForEach(c => c.position = transform.position);

        Vector3 f = Vector3.forward * radius;

        Quaternion[] rotations = Enumerable.Range(0, children.Count).Select(i =>
          Quaternion.Euler(0, Mathf.Lerp(startAngle, endAngle, (float)i / (children.Count - 1)), 0)).ToArray();

        for (int i = 0; i < children.Count; i++)
        {
            children[i].localRotation = rotations[i];
            children[i].localPosition = rotations[i] * f - f;
        }

    }
}
