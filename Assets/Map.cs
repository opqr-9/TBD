using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Map : MonoBehaviour
{
    public static List<Vector2> points;
    public static List<Triangle> triangles;
    public static HashSet<Line> lines;
    public static Dictionary<Line,List<Triangle>> lineTriangleDictionary;
    public static Dictionary<int,List<Triangle>> pointIndexTriangleDictionary;
    public GameObject trianglePrefab;
    public Material material;
    public Material stopMaterial;
    public int num;
    public int size;
    public float edgeWidth;
    private Camera camera;

    private float time = 0;
    
    private static Building largeBuilding = new Building(5, Vector2Int.zero, 1.5f);
    public Button button;
    public bool flag = false;

    Triangle InstantiateTriangle(int v0, int v1, int v2)
    {
        GameObject newTriangle = Instantiate(trianglePrefab,transform);
        Triangle triangle = newTriangle.GetComponent<Triangle>();
        triangle.Init(v0,v1,v2);
        return triangle;
    }

    void ConstrainedTriangulation(List<int> pointsIndices)
    {
        for (int i = 0; i < pointsIndices.Count; i++)
        {
            Triangulation(pointsIndices[i]);
        }
        List<Line> constrainedLines = new List<Line>();
        for (int i = 0; i < pointsIndices.Count; i++)
        {
            Line l = new Line();
            l.minpointIndex=Mathf.Min(pointsIndices[i],pointsIndices[(i + 1)%pointsIndices.Count]);
            l.maxpointIndex=Mathf.Max(pointsIndices[i],pointsIndices[(i + 1)%pointsIndices.Count]);
            if (!lines.Contains(l))
            {
                constrainedLines.Add(l);
            }
        }

        Queue<Line> conflitLines = new Queue<Line>();
        for (int cnt = 0; cnt < constrainedLines.Count; cnt++)
        {
            foreach (var l in lines)
            {
                if (l.CheckLine(constrainedLines[cnt]))
                {
                    conflitLines.Enqueue(l);
                }
            }

            while (conflitLines.Count > 0)
            {
                List<int> indices = new List<int>();
                indices.Add(conflitLines.Peek().minpointIndex);
                indices.Add(conflitLines.Peek().maxpointIndex);
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < indices.Count; k++)
                    {
                        if (indices[k] != lineTriangleDictionary[conflitLines.Peek()][0].verticesIndices[j])
                        {
                            indices.Add(lineTriangleDictionary[conflitLines.Peek()][0].verticesIndices[j]);
                        }

                        if (indices[k] != lineTriangleDictionary[conflitLines.Peek()][1].verticesIndices[j])
                        {
                            indices.Add(lineTriangleDictionary[conflitLines.Peek()][1].verticesIndices[j]);
                        }
                    }
                }

                Line line = new Line();
                line.minpointIndex = Mathf.Min(indices[2], indices[3]);
                line.maxpointIndex = Mathf.Max(indices[2], indices[3]);
                if (line.CheckLine(constrainedLines[cnt]))
                {
                    conflitLines.Enqueue(line);
                }

                lineTriangleDictionary[conflitLines.Peek()][0].Remove();
                lineTriangleDictionary[conflitLines.Peek()][1].Remove();
                InstantiateTriangle(indices[0], indices[2], indices[3]);
                InstantiateTriangle(indices[1], indices[2], indices[3]);
                conflitLines.Dequeue();
            }
        }
    }
    
    void Triangulation(int pointIndex)
    {
        HashSet<int> hashIndices=new HashSet<int>();
        List<int> newPointsIndices=new List<int>();
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].circumcircle.Check(points[pointIndex]))
            { 
                for (int j = 0; j < 3; j++)
                {
                    hashIndices.Add(triangles[i].verticesIndices[j]);
                }
                triangles[i].Remove();
                i--;
            }
        }

        foreach (int vindices in hashIndices)
        {
            newPointsIndices.Add(vindices);
        }
        bool flag=true;
        Vector2 basevector = points[newPointsIndices[0]]-points[pointIndex];
        while (flag)
        {
            flag = false;
            for (int i = 1; i < newPointsIndices.Count-1; i++)
            {
                Vector2 curVector = points[newPointsIndices[i]]-points[pointIndex];
                Vector2 nextVector = points[newPointsIndices[i+1]]-points[pointIndex];
                Vector3 curCross = Vector3.Cross(basevector, curVector);
                Vector3 nextCross = Vector3.Cross(basevector, nextVector);
                float curAngle = Vector2.Angle(basevector, curVector);
                float nextAngle = Vector2.Angle(basevector, nextVector);
                if (curCross.z < 0 && nextCross.z >= 0)
                {
                    (newPointsIndices[i], newPointsIndices[i + 1]) = (newPointsIndices[i + 1], newPointsIndices[i]);
                    flag = true;
                }
                else if (curCross.z >= 0 && nextCross.z >= 0 && curAngle > nextAngle)
                {
                    (newPointsIndices[i], newPointsIndices[i + 1]) = (newPointsIndices[i + 1], newPointsIndices[i]);
                    flag = true;
                }
                else if (curCross.z < 0 && nextCross.z < 0 && curAngle < nextAngle)
                {
                    (newPointsIndices[i], newPointsIndices[i + 1]) = (newPointsIndices[i + 1], newPointsIndices[i]);
                    flag = true;
                }
            }
        }
        for (int i = 0; i < newPointsIndices.Count; i++)
        {
            if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
                    points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
            {
                InstantiateTriangle(newPointsIndices[i], newPointsIndices[(i + 1) % newPointsIndices.Count],
                    pointIndex);
            }
        }
        
    }


    void EarCut()
    {
        List<Vector2> pointSet = new List<Vector2>(points);
        for (int i = 0; i < pointSet.Count; i++)
        {
            //////////////////////////////////////////////////////////将三角形和三角网格分开实现，避免单纯检测点是否在三角形内要生成三角网格
        }
    }
    
    void InitMesh()
    {
        EarCut();
        // Vector2 min = new Vector2(0, 0);
        // Vector2 max = new Vector2(size, size);
        // points.Add(min);
        // points.Add(max);
        // points.Add(new Vector2(0, size));
        // points.Add(new Vector2(size, 0));
        // InstantiateTriangle(0,1,2);
        // InstantiateTriangle(0,1,3);
    }
    
    void UpdateMesh(Vector2 point)
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (Mathf.Abs(points[i].x - point.x) < 0.01f && Mathf.Abs(points[i].y - point.y) < 0.01f)
            {
                return;
            }
        }
        points.Add(point);
        Triangulation(points.Count-1);
    }

    void UpdateMesh(List<Vector2> pointSet)
    {
        List<int> verticesIndices = new List<int>();
        for (int i = 0; i < pointSet.Count; i++)
        {
            bool flag = true;
            for (int j = 0; j < points.Count; j++)
            {
                if (Mathf.Abs(points[j].x - pointSet[i].x) < 0.01f && Mathf.Abs(points[j].y - pointSet[i].y) < 0.01f)
                {
                    verticesIndices.Add(j);
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                points.Add(pointSet[i]);
                verticesIndices.Add(points.Count-1);
            }
        }
        ConstrainedTriangulation(verticesIndices);
    }

    void InittrianglePrefab()
    {
        trianglePrefab.GetComponent<MeshRenderer>().material = material;
        LineRenderer lineRenderer = trianglePrefab.GetComponent<LineRenderer>();
        lineRenderer.material = material;
        lineRenderer.positionCount = 3;
        lineRenderer.loop = true;
        lineRenderer.startWidth = edgeWidth;
        lineRenderer.endWidth = edgeWidth;
        lineRenderer.startColor=Color.black;
        lineRenderer.endColor=Color.black;
    }

    void Lock(Vector2 center)
    {
        HashSet<int> hashIndices=new HashSet<int>();
        List<int> newPointsIndices=new List<int>();
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].circumcircle.Check(center))
            {
                triangles[i].GetComponent<MeshRenderer>().material=stopMaterial;
            }
        }
    }

    private void Awake()
    {
        points=new List<Vector2>();
        lines=new HashSet<Line>();
        triangles=new List<Triangle>();
        lineTriangleDictionary=new Dictionary<Line,List<Triangle>>();
        pointIndexTriangleDictionary=new Dictionary<int, List<Triangle>>();
        camera=Camera.main;
    }

    void Start()
    {
        camera.transform.position = new Vector3(size / 2, size / 2, -10);
        camera.orthographicSize=size/2;
        
        InittrianglePrefab();

        // InitMesh();
        
        button.onClick.AddListener(() =>
        {
            flag = true;
            InitMesh();
        });
    }

    void Update()
    {
        if (!flag)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // List<Vector2> pointSet=new List<Vector2>();
                Vector3 mousePosition = UnityEngine.Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                points.Add(worldPosition);
                // pointSet.Add(worldPosition);
            }
        }
        else
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = UnityEngine.Input.mousePosition;
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                Vector2Int pointer=new Vector2Int((int)worldPosition.x, (int)worldPosition.y);
                largeBuilding.center = pointer;
                UpdateMesh(largeBuilding.Vertices());
            }
        }
        
        
    }
}
