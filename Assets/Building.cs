
using System.Collections.Generic;
using UnityEngine;

public class Building
{
    private int size;
    public Vector2Int center;
    private float cutLength;

    public Building(int size, Vector2Int center, float cutLength)
    {
        this.size = size;
        this.center = center;
        this.cutLength = cutLength;
    }

    public List<Vector2> Vertices()
    {
        Vector2 leftdown=center-new Vector2Int(size/2,size/2);
        List<Vector2> vertices = new List<Vector2>();
        vertices.Add(leftdown+new Vector2(cutLength,0));
        vertices.Add(leftdown+new Vector2(size-cutLength,0));
        vertices.Add(leftdown+new Vector2(size,cutLength));
        vertices.Add(leftdown+new Vector2(size,size-cutLength));
        vertices.Add(leftdown+new Vector2(size-cutLength,size));
        vertices.Add(leftdown+new Vector2(cutLength,size));
        vertices.Add(leftdown+new Vector2(0,size-cutLength));
        vertices.Add(leftdown+new Vector2(0,cutLength));
        return vertices;
    }
}
