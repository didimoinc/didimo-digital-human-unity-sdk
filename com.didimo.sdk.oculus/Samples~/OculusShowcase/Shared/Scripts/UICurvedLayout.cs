using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using UnityEngine;

public class UICurvedLayout : MonoBehaviour
{
    [SerializeField]
    private float radius = 1;
    [SerializeField]
    private float startAngle = -90;
    [SerializeField]
    private float endAngle = 90;
    [SerializeField]
    private List<Transform> children;

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
