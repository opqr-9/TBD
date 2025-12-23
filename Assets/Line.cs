using UnityEngine;

//直线
public class Ray
{
    Vector2 start;
    Vector2 v;

    public Ray(Vector2 start, Vector2 v)
    {
        this.start = start;
        this.v = v;
    }

    //给出r与这条直线的交点
    public Vector2 CrossPoint(Ray r)
    {
        float b;
        b = ((start.x - r.start.x) * v.y + (r.start.y - start.y) * v.x) / (r.v.x * v.y - r.v.y * v.x);
        return r.start + b * r.v;
    }
}

//线段
public struct Line
{
    public DigitalMesh digitalMesh;
    public int minpointIndex;
    public int maxpointIndex;

    public Line(int pointIndex1, int pointIndex2,DigitalMesh digitalMesh)
    {
        this.digitalMesh = digitalMesh;
        if (pointIndex1 < pointIndex2)
        {
            minpointIndex = pointIndex1;
            maxpointIndex = pointIndex2;
        }
        else
        {
            minpointIndex = pointIndex2;
            maxpointIndex = pointIndex1;
        }
    }

    public override bool Equals( object obj)
    {
        return obj is Line l&&this.Equals(l);
    }

    public bool Equals(Line l)
    {
        return digitalMesh==l.digitalMesh&&minpointIndex == l.minpointIndex && maxpointIndex == l.maxpointIndex;
    }

    //检查otherLine是否与这条线段相交
    public bool CheckLine(Line otherLine)
    {
        Vector2 AB=digitalMesh.points[maxpointIndex]-digitalMesh.points[minpointIndex];
        Vector2 AC=digitalMesh.points[maxpointIndex]-digitalMesh.points[otherLine.maxpointIndex];
        Vector2 AD=digitalMesh.points[maxpointIndex]-digitalMesh.points[otherLine.minpointIndex];
        Vector2 CD=digitalMesh.points[otherLine.maxpointIndex]-digitalMesh.points[otherLine.minpointIndex];
        Vector2 CA=digitalMesh.points[otherLine.maxpointIndex]-digitalMesh.points[maxpointIndex];
        Vector2 CB=digitalMesh.points[otherLine.maxpointIndex]-digitalMesh.points[minpointIndex];

        if (-Vector3.Cross(AB, AC).z * -Vector3.Cross(AB, AD).z < 0)
        {
            if (-Vector3.Cross(CD, CA).z * -Vector3.Cross(CD, CB).z < 0)
            {
                return true;
            }
        }
        return false;
    }
}
