using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class MathUtils
{
    static public Vector3[] CalculateRectPoints(Matrix4x4 mattransform, Vector3 offset, Rect bounds)
    {
        Vector3[] ret =
        {
            mattransform.MultiplyPoint(new Vector3(offset.x + bounds.xMin, offset.y + bounds.yMin, offset.z)),
            mattransform.MultiplyPoint(new Vector3(offset.x + bounds.xMax, offset.y + bounds.yMin, offset.z)),
            mattransform.MultiplyPoint(new Vector3(offset.x + bounds.xMax, offset.y + bounds.yMax, offset.z)),
            mattransform.MultiplyPoint(new Vector3(offset.x + bounds.xMin, offset.y + bounds.yMax, offset.z))
        };
        return ret;
    }

    static public Vector3 GetAveragePoint(Vector3[] pts)
    {
        Vector3 av = new Vector3(0, 0, 0);
        for (int i = 0; i < pts.Length; ++i)
        {
            av.x += pts[i].x;
            av.y += pts[i].y;
            av.z += pts[i].z;
        }

        float recip = 1.0f / pts.Length;
        av.x *= recip;
        av.y *= recip;
        av.z *= recip;
        return av;
    }

    static public void CalculateLeastSquares(out Vector3 vector, out Vector3 offset, in Transform[] transforms, in Transform MainTransform, int noffset)
    {
        float sumx = 0, sumy = 0, sumz = 0;
        float sumxy = 0, sumxz = 0, sumxx = 0;
        int nodeCount = transforms.Length;

        for (int i = 0; i < nodeCount; ++i)
        {
            Vector3 p = transforms[noffset + i].position;
            p = MainTransform.InverseTransformPoint(p);
            sumx += p.x;
            sumy += p.y;
            sumz += p.z;

            sumxy += p.x * p.y;
            sumxz += p.x * p.z;
            sumxx += p.x * p.x;
        }

        float slopexy = (nodeCount * sumxy - sumx * sumy) / (nodeCount * sumxx - sumx * sumx);
        float slopexz = (nodeCount * sumxz - sumx * sumz) / (nodeCount * sumxx - sumx * sumx);
        vector = new Vector3(1, slopexy, -slopexz);
        vector.Normalize();
        float recip = 1.0f / (float) nodeCount;
        offset = new Vector3(sumx * recip, sumy * recip, sumz * recip);
    }
}