using System.Collections.Generic;
using System.Data;
using UnityEngine;

public struct Triangle
{
    public List<int> verticesIndices;
    private DigitalMesh digitalMesh;
    public Vector2 a;
    public Vector2 b;
    public Vector2 c;
    
    private Vector2 ab;
    private Vector2 bc;
    private Vector2 ca;

    public Circle circumcircle;
    
    public override bool Equals(object obj)
    {
        return obj is Triangle triangle&&this.Equals(triangle);
    }

    public bool Equals(Triangle triangle)
    {
        return verticesIndices == triangle.verticesIndices && digitalMesh == triangle.digitalMesh;
    }

    public Triangle(int v0, int v1, int v2,DigitalMesh digitalMesh)
    {
        verticesIndices=new List<int>();
        this.digitalMesh = digitalMesh;
        verticesIndices.Add(v0);
        verticesIndices.Add(v1);
        verticesIndices.Add(v2);
        a = digitalMesh.points[v0];
        b = digitalMesh.points[v1];
        c = digitalMesh.points[v2];
        ab = b - a;
        bc = c - b;
        ca = a - c;
        
        Ray l1=new Ray((a+b)/2,Vector3.Cross(ab,Vector3.forward));
        Ray l2=new Ray((b+c)/2,Vector3.Cross(bc,Vector3.forward));
        Vector2 center=l1.CrossPoint(l2);
        circumcircle=new Circle(center, (center - a).magnitude);
    }
    
    //检查点p是否在三角内
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
    public Triangle triangle;

    private MeshFilter meshFilter;
    private LineRenderer lineRenderer;
    private CircleCollider2D circleCollider2D;

    public void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        lineRenderer = GetComponent<LineRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    public void Init(Triangle triangle)
    {
        this.triangle = triangle;
        Map.triangleMeshs.Add(this);
        
        Mesh mesh = new Mesh();
        mesh.name = "Mesh";
        Vector3[] vertices= new Vector3[3]
        {
            triangle.a, triangle.b, triangle.c
        };
        mesh.vertices = vertices;
        int[] index = new int[3] { 0, 1, 2 };
        mesh.triangles = index;
        meshFilter.sharedMesh = mesh;
        lineRenderer.SetPositions(vertices);
        circleCollider2D.radius = triangle.circumcircle.radius;
        circleCollider2D.offset = triangle.circumcircle.center;
    }

    
    
    public void Remove()
    {
        Map.triangleMeshs.Remove(this);
        Destroy(gameObject);
    }
}
