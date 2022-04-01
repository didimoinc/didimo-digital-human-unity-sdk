using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POIMove : MonoBehaviour
{
    public int usageCounter;

    public Vector3 position => transform.position;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(position, 0.1f);
    }
}
