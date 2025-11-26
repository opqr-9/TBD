using System.Numerics;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class BoundingBox
{
    Vector3 min;
    Vector3 max;
    BoundingBox(Vector<Vector3> triangle)
    {
        min.X=Mathf.Min(triangle[0].X,triangle[1].X,triangle[2].X);
        min.Y=Mathf.Min(triangle[0].Y,triangle[1].Y,triangle[2].Y);
        min.Z=Mathf.Min(triangle[0].Z,triangle[1].Z,triangle[2].Z);
        max.X=Mathf.Max(triangle[0].X,triangle[1].X,triangle[2].X);
        max.Y=Mathf.Max(triangle[0].Y,triangle[1].Y,triangle[2].Y);
        max.Z=Mathf.Max(triangle[0].Z,triangle[1].Z,triangle[2].Z);
    }
}
