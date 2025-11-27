using UnityEngine;

public class Ray
{
    Vector2 start;
    Vector2 v;

    public Ray(Vector2 start, Vector2 v)
    {
        this.start = start;
        this.v = v;
    }

    public Vector2 CrossPoint(Ray r)
    {
        float b;
        b = ((start.x - r.start.x) * v.y + (r.start.y - start.y) * v.x) / (r.v.x * v.y - r.v.y * v.x);
        return r.start + b * r.v;
    }
}
public struct Line
{
    public int minpointIndex;
    public int maxpointIndex;

    public bool CheckLine(Line otherline)
    {
        Vector2 AB=Map.points[maxpointIndex]-Map.points[minpointIndex];
        Vector2 AC=Map.points[maxpointIndex]-Map.points[otherline.maxpointIndex];
        Vector2 AD=Map.points[maxpointIndex]-Map.points[otherline.minpointIndex];
        Vector2 CD=Map.points[otherline.maxpointIndex]-Map.points[otherline.minpointIndex];
        Vector2 CA=Map.points[otherline.maxpointIndex]-Map.points[maxpointIndex];
        Vector2 CB=Map.points[otherline.maxpointIndex]-Map.points[minpointIndex];

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
