using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
public class GizmoDrawingFunctions             
{    
    static public void DrawPoly(Vector3[] pts)
    {
        for (int i = 0; i < pts.Length - 1; ++i)
        {
            Gizmos.DrawLine(pts[i], pts[i + 1]);
        }
        Gizmos.DrawLine(pts[pts.Length - 1], pts[0]);
    } 
    static public void DrawMatrixBasis(Matrix4x4 basis, float length)
    {
        Vector3 pos = basis.GetColumn(3);
        Vector3 i = basis.GetColumn(0);
        Vector3 j = basis.GetColumn(1);
        Vector3 k = basis.GetColumn(2);
        Color oldcol = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + i * length);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + j * length);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + k * length);
        Gizmos.color = oldcol;
    }

    static public void DrawMatrixBasis(Matrix4x4 basis,Vector3 scale)
    {
        Vector3 pos = basis.GetColumn(3);
        Vector3 i = basis.GetColumn(0);
        Vector3 j = basis.GetColumn(1);
        Vector3 k = basis.GetColumn(2);
        Color oldcol = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + i * scale.x);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + j * scale.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + k * scale.z);
        Gizmos.color = oldcol;
    }
}