
using UnityEngine;

public class Circle
{
    public Vector2 center;
    public float radius;

    public Circle(Vector2 c, float r) 
    {
        center = c;
        radius = r;
    }

    public bool Check(Vector2 point)
    {
        if ((point - center).magnitude <= radius)
        {
            return true;
        }
        return false;
    }
}
