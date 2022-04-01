using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POILook : MonoBehaviour
{
    public Vector3 offset;
    public Vector3 position => transform.position + offset;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(position, 0.1f);
    }
}
