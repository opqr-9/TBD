using System.Collections.Generic;
using UnityEngine;

public class Building
{
    public List<Vector2Int> area;
    public List<int> pointsIndices;

    public Building(List<int> indeces,List<Vector2Int> area)
    {
        pointsIndices = indeces;
        this.area = area;
    }
}

public abstract class BuildingData
{
    public abstract List<Vector2> Vertices(Vector2Int center);
    
    public abstract List<Vector2Int> Area(Vector2Int center);
    
    public abstract List<Vector3> Contour(Vector2Int center);
}

public class Octagon: BuildingData
{
    private int size;
    private float cutLength;

    public Octagon(int size, float cutLength)
    {
        this.size = size;
        this.cutLength = cutLength;
    }

    public override List<Vector2> Vertices(Vector2Int center)
    {
        List<Vector2> vertices = new List<Vector2>();
        Vector2Int leftdown=center-new Vector2Int(size/2,size/2);
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

    public override List<Vector2Int> Area(Vector2Int center)
    {
        List<Vector2Int> areas = new List<Vector2Int>();
        Vector2Int leftdown =center-new Vector2Int(size/2,size/2);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                areas.Add(new Vector2Int(i, j)+leftdown);
            }
        }
        return areas;
    }

    public override List<Vector3> Contour(Vector2Int center)
    {
        List<Vector3> contour = new List<Vector3>();
        Vector3 leftdown =(Vector2)center-new Vector2Int(size/2,size/2);
        contour.Add((Vector2)leftdown);
        contour.Add(leftdown+new Vector3(size,0));
        contour.Add(leftdown+new Vector3(size,size));
        contour.Add(leftdown+new Vector3(0,size));
        return contour;
    }
}
