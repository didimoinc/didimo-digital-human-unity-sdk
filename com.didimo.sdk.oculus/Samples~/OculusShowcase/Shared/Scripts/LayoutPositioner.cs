using System.Collections.Generic;
using UnityEngine;
using Didimo.Core.Inspector;
using System.Linq;
/// <summary>
/// Class to position the didimos in the correct layout, in Multi User scenes.
/// </summary>
public class LayoutPositioner : MonoBehaviour
{
    [Range(0.1f, 5)] public float radius = 1;
    [Range(-2, 2)] public float offsetY = 0;
    public Order order = Order.Sequential;

    [Button("Layout")]
    public void Layout()
    {
        Transform[] positions;
        switch (order)
        {
            default:
            case Order.Sequential:
                positions = GetPositions().ToArray();
                break;
            case Order.Random:
                positions = GetPositions().OrderBy(a => UnityEngine.Random.value).ToArray();
                break;
        }

        Quaternion[] rotations = GetRotations(positions.Length).ToArray();
        Vector3 forward = transform.forward * radius;
        Vector3 offset = new Vector3(0, offsetY, 0);
        Quaternion _180 = Quaternion.Euler(0, 180, 0);

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i].transform.rotation = rotations[i] * _180;
            positions[i].transform.position = rotations[i] * forward + offset;
        }
    }

    private IEnumerable<Transform> GetPositions()
    {
        foreach (Transform t in transform)
            yield return t;
    }

    private IEnumerable<Quaternion> GetRotations(int count)
    {
        Quaternion rotation = Quaternion.identity, step = Quaternion.Euler(0, 360f / count, 0);
        for (int i = count; i > 0; --i, rotation *= step)
            yield return rotation;
    }

    public enum Order
    {
        Sequential = 0,
        Random = 1,
    }
}