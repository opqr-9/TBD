using System.Collections.Generic;
using System.Data;
using UnityEngine;

public struct Triangle
{
    public List<int> verticesIndices;
    private Vector2 a;
    private Vector2 b;
    private Vector2 c;
    
    private Vector2 ab;
    private Vector2 bc;
    private Vector2 ca;

    public Circle circumcircle;

    public Triangle(int v0, int v1, int v2,List<Vector2> points)
    {
        verticesIndices=new List<int>();
        verticesIndices.Add(v0);
        verticesIndices.Add(v1);
        verticesIndices.Add(v2);
        a = points[v0];
        b = points[v1];
        c = points[v2];
        ab = b - a;
        bc = c - b;
        ca = a - c;
        
        Ray l1=new Ray((a+b)/2,Vector3.Cross(ab,Vector3.forward));
        Ray l2=new Ray((b+c)/2,Vector3.Cross(bc,Vector3.forward));
        Vector2 center=l1.CrossPoint(l2);
        circumcircle=new Circle(center, (center - a).magnitude);
    }
    
    public bool Check(Vector2 p)
    {
        Vector2 ap = p - a;
        Vector2 bp = p - b;
        Vector2 cp = p - c;
        if (-Vector3.Cross(ap, ab).z * -Vector3.Cross(bp, bc).z>0)
        {
            if (-Vector3.Cross(ap, ab).z * -Vector3.Cross(cp, ca).z>0)
            {
                return true;
            }
        }
        return false;
    }
}

public class TriangleMesh:MonoBehaviour
{
    // public List<Vector2> vertices=new List<Vector2>(3);
    public Triangle triangle;
    // public List<int> verticesIndices=new List<int>(3);
    // private Vector2 a;
    // private Vector2 b;
    // private Vector2 c;
    //
    // private Vector2 ab;
    // private Vector2 bc;
    // private Vector2 ca;
    //
    // public Circle circumcircle;

    private MeshFilter meshFilter;
    private LineRenderer lineRenderer;
    private CircleCollider2D circleCollider2D;

    public void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        lineRenderer = GetComponent<LineRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    public void Init(int v0, int v1, int v2)
    {
        // verticesIndices.Add(v0);
        // verticesIndices.Add(v1);
        // verticesIndices.Add(v2);
        // this.a = Map.points[v0];
        // this.b = Map.points[v1];
        // this.c = Map.points[v2];
        // ab = b - a;
        // bc = c - b;
        // ca = a - c;
        //
        // Ray l1=new Ray((a+b)/2,Vector3.Cross(ab,Vector3.forward));
        // Ray l2=new Ray((b+c)/2,Vector3.Cross(bc,Vector3.forward));
        // Vector2 center=l1.CrossPoint(l2);
        // circumcircle=new Circle(center, (center - a).magnitude);
        triangle=new Triangle(v0, v1, v2,Map.points);
        Map.triangleMeshs.Add(this);
        ConnectLine();
        ConnectPoint();
        
        Mesh mesh = new Mesh();
        mesh.name = "Mesh";
        Vector3[] vertices= new Vector3[3]
        {
            Map.points[triangle.verticesIndices[0]], Map.points[triangle.verticesIndices[1]],
            Map.points[triangle.verticesIndices[2]]
        };
        mesh.vertices = vertices;
        int[] index = new int[3] { 0, 1, 2 };
        mesh.triangles = index;
        meshFilter.sharedMesh = mesh;
        lineRenderer.SetPositions(vertices);
        circleCollider2D.radius = triangle.circumcircle.radius;
        circleCollider2D.offset = triangle.circumcircle.center;
    }

    void ConnectLine()
    {
        for (int i = 0; i < 3; i++)
        {
            Line l=new Line();
            l.minpointIndex = Mathf.Min(triangle.verticesIndices[i], triangle.verticesIndices[(i + 1) % 3]);
            l.maxpointIndex = Mathf.Max(triangle.verticesIndices[i], triangle.verticesIndices[(i + 1) % 3]);
            Map.lines.Add(l);
            if (!Map.lineTriangleDictionary.ContainsKey(l))
            {
                List<TriangleMesh> tmp = new List<TriangleMesh>(){this};
                Map.lineTriangleDictionary.Add(l,tmp);
            }
            else
            {
                Map.lineTriangleDictionary[l].Add(this);
            }
        }
    }

    void ConnectPoint()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!Map.pointIndexTriangleDictionary.ContainsKey(triangle.verticesIndices[i]))
            {
                List<TriangleMesh> tmp = new List<TriangleMesh>(){this};
                Map.pointIndexTriangleDictionary.Add(triangle.verticesIndices[i],tmp);
            }
            else
            {
                Map.pointIndexTriangleDictionary[triangle.verticesIndices[i]].Add(this);
            }
        }
    }

    void DisConnectLine()
    {
        for (int i = 0; i < 3; i++)
        {
            Line l=new Line();
            l.minpointIndex = Mathf.Min(triangle.verticesIndices[i], triangle.verticesIndices[(i + 1) % 3]);
            l.maxpointIndex = Mathf.Max(triangle.verticesIndices[i], triangle.verticesIndices[(i + 1) % 3]);
            Map.lineTriangleDictionary[l].Remove(this);
            if (Map.lineTriangleDictionary[l].Count==0)
            {
                Map.lineTriangleDictionary.Remove(l);
                Map.lines.Remove(l);
            }
        }
    }

    void DisConnectPoint()
    {
        for (int i = 0; i < 3; i++)
        {
            Map.pointIndexTriangleDictionary[triangle.verticesIndices[i]].Remove(this);
        }
    }
    
    // public bool Check(Vector2 p)
    // {
    //     Vector2 ap = p - Map.points[verticesIndices[0]];
    //     Vector2 bp = p - Map.points[verticesIndices[1]];
    //     Vector2 cp = p - Map.points[verticesIndices[2]];
    //     if (Vector3.Cross(ap, ab).z * Vector3.Cross(bp, bc).z>0)
    //     {
    //         if (Vector3.Cross(ap, ab).z * Vector3.Cross(cp, ca).z>0)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    
    public void Remove()
    {
        Map.triangleMeshs.Remove(this);
        DisConnectLine();
        DisConnectPoint();
        Destroy(gameObject);
    }
}
