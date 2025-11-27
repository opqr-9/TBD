using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Map : MonoBehaviour
{
    public static List<Vector2> points;
    public static List<TriangleMesh> triangleMeshs;
    public static HashSet<Line> lines;
    public static Dictionary<Line,List<TriangleMesh>> lineTriangleDictionary;
    public static Dictionary<int,List<TriangleMesh>> pointIndexTriangleDictionary;
    public GameObject trianglePrefab;
    public Material material;
    public Material stopMaterial;
    public int num;
    public int size;
    public float edgeWidth;
    private Camera camera;

    private float time = 0;
    
    private static Building largeBuilding = new Building(5, Vector2Int.zero, 1.5f);
    public Canvas canvas;
    private List<GameObject> pointPrefabs = new List<GameObject>();
    public GameObject pointPrefab;
    public Button clear;
    public Button startTriangulation;
    public bool flag = false;

    void InstantiateTriangleMesh(int v0, int v1, int v2)
    {
        GameObject newTriangle = Instantiate(trianglePrefab,transform);
        newTriangle.GetComponent<TriangleMesh>().Init(v0,v1,v2);
    }

    List<Line> ConstrainedLines(List<int> pointsIndices)
    {
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
        return  constrainedLines;
    }

    void ConstrainedTriangulation(List<int> pointsIndices,List<Line> constrainedLines,bool keepInside)
    {
        for (int i = 0; i < pointsIndices.Count; i++)
        {
            Triangulation(pointsIndices[i]);
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
                        if (indices[k] != lineTriangleDictionary[conflitLines.Peek()][0].triangle.verticesIndices[j])
                        {
                            indices.Add(lineTriangleDictionary[conflitLines.Peek()][0].triangle.verticesIndices[j]);
                        }

                        if (indices[k] != lineTriangleDictionary[conflitLines.Peek()][1].triangle.verticesIndices[j])
                        {
                            indices.Add(lineTriangleDictionary[conflitLines.Peek()][1].triangle.verticesIndices[j]);
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
                InstantiateTriangleMesh(indices[0], indices[2], indices[3]);
                InstantiateTriangleMesh(indices[1], indices[2], indices[3]);
                conflitLines.Dequeue();
            }
        }
    }

    void DeleteLastPoints(int length)
    {
        for (int i = 0; i < length; i++) 
        {
            DeletePoint(points.Count-1);
        }
    }

    void DeletePoint(int pointIndex)
    {
        HashSet<int> hashIndices=new HashSet<int>();
        List<int> emptyHolePointsIndices=new List<int>();
        for (int i = 0; i < pointIndexTriangleDictionary[pointIndex].Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                hashIndices.Add(pointIndexTriangleDictionary[pointIndex][i].triangle.verticesIndices[j]);
            }
            pointIndexTriangleDictionary[pointIndex][i].Remove();
            i--;
        }
        hashIndices.Remove(pointIndex);
        foreach (int vindices in hashIndices)
        {
            emptyHolePointsIndices.Add(vindices);
        }
        EmptyHoleTriangulation(emptyHolePointsIndices);
        points.RemoveAt(pointIndex);
    }

    void EmptyHoleTriangulation(List<int> pointsIndices)
    {
        Vector2 leftdown=points[pointsIndices[0]], rightup=points[pointsIndices[0]];
        for (int i = 0; i < pointsIndices.Count; i++)
        {
            leftdown = Vector2.Min((points[pointsIndices[i]]), leftdown);
            rightup = Vector2.Max((points[pointsIndices[i]]), rightup);
        }
        leftdown-=Vector2.one;
        rightup+=Vector2.one;
        List<Vector2> tmpPoints = new List<Vector2>();
        List<int> tmpPointsIndices = new List<int>();
        tmpPoints.Add(leftdown);
        tmpPoints.Add(new Vector2(rightup.x, leftdown.y));
        tmpPoints.Add(rightup);
        tmpPoints.Add(new Vector2(leftdown.x, rightup.y));
        List<Triangle> triangles = new List<Triangle>();
        Triangle triangle1 = new Triangle(0,1,2,tmpPoints);
        Triangle triangle2 = new Triangle(0,2,3,tmpPoints);
        triangles.Add(triangle1);
        triangles.Add(triangle2);
        for (int i = 0; i < pointsIndices.Count; i++)
        {
            tmpPoints.Add(points[pointsIndices[i]]);
            tmpPointsIndices.Add(i+4);
            OutTriangulation(tmpPoints,triangles,tmpPointsIndices[i]);
        }

        for (int i = 0; i < triangles.Count; i++)
        {
            bool flag = true;
            for (int j = 0; j < 3; j++)
            {
                if (triangles[i].verticesIndices[j] < 4)
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                InstantiateTriangleMesh(pointsIndices[triangles[i].verticesIndices[0] - 4],
                    pointsIndices[triangles[i].verticesIndices[1] - 4],
                    pointsIndices[triangles[i].verticesIndices[2] - 4]);
            }
        }
    }

    void OutTriangulation(List<Vector2> points,List<Triangle> triangles, int pointIndex)
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
                triangles.RemoveAt(i);
                i--;
            }
        }

        foreach (int vindices in hashIndices)
        {
            newPointsIndices.Add(vindices);
        }
        
        Clockwise(points,newPointsIndices,pointIndex);
        
        for (int i = 0; i < newPointsIndices.Count; i++)
        {
            if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
                    points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
            {
                Triangle triangle = new Triangle(newPointsIndices[i],
                    newPointsIndices[(i + 1) % newPointsIndices.Count],
                    pointIndex, points);
                triangles.Add(triangle);
            }
        }
    }

    void Clockwise(List<Vector2> points,List<int> newPointsIndices,int pointIndex)
    {
        bool flag=true;
        Vector2 basevector = points[newPointsIndices[0]]-points[pointIndex];
        while (flag)
        {
            flag = false;
            for (int i = 1; i < newPointsIndices.Count-1; i++)
            {
                Vector2 curVector = points[newPointsIndices[i]]-points[pointIndex];
                Vector2 nextVector = points[newPointsIndices[i+1]]-points[pointIndex];
                Vector3 curCross = -Vector3.Cross(curVector, basevector);
                Vector3 nextCross = -Vector3.Cross(nextVector, basevector);
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
    }
    
    void Triangulation(int pointIndex)
    {
        HashSet<int> hashIndices=new HashSet<int>();
        List<int> newPointsIndices=new List<int>();
        for (int i = 0; i < triangleMeshs.Count; i++)
        {
            if (triangleMeshs[i].triangle.circumcircle.Check(points[pointIndex]))
            {
                for (int j = 0; j < 3; j++)
                {
                    hashIndices.Add(triangleMeshs[i].triangle.verticesIndices[j]);
                }
                triangleMeshs[i].Remove();
                i--;
            }
        }

        foreach (int vindices in hashIndices)
        {
            newPointsIndices.Add(vindices);
        }
        
        Clockwise(points,newPointsIndices,pointIndex);
        
        for (int i = 0; i < newPointsIndices.Count; i++)
        {
            if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
                    points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
            {
                InstantiateTriangleMesh(newPointsIndices[i], newPointsIndices[(i + 1) % newPointsIndices.Count],
                    pointIndex);
            }
        }
    }


    void EarCut()
    {
        List<int> pointIndices = new List<int>();
        for (int i = 0; i < points.Count; i++)
        {
            pointIndices.Add(i);
        }
        int cnt = 0;
        for (int i = 0; pointIndices.Count>2; i++)
        {
            i%=pointIndices.Count;
            Vector2 a=points[pointIndices[i]];
            Vector2 b=points[pointIndices[(i+1)%pointIndices.Count]];
            Vector2 c=points[pointIndices[(i+2)%pointIndices.Count]];
            if (Vector3.Cross(b - a, c - b).z < 0)
            {
                continue;
            }
            Triangle triangle = new Triangle(pointIndices[i],pointIndices[(i+1)%pointIndices.Count],pointIndices[(i+2)%pointIndices.Count],points);
            bool flag = true;
            for (int j = i + 3; j%pointIndices.Count != i; j++)
            {
                j%=pointIndices.Count;
                if (triangle.Check(points[pointIndices[j%pointIndices.Count]]))
                {
                    flag = false;
                    break;
                }
            }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
            if (flag)
            {
                InstantiateTriangleMesh(pointIndices[i],pointIndices[(i+1)%pointIndices.Count],pointIndices[(i+2)%pointIndices.Count]);
                pointIndices.RemoveAt((i+1)%pointIndices.Count);
                i--;
            }
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
        // InstantiateTriangleMesh(0,1,2);
        // InstantiateTriangleMesh(0,1,3);
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
        List<Line> constrainedLines = ConstrainedLines(verticesIndices);
        ConstrainedTriangulation(verticesIndices,constrainedLines,true);
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
        for (int i = 0; i < triangleMeshs.Count; i++)
        {
            if (triangleMeshs[i].triangle.circumcircle.Check(center))
            {
                triangleMeshs[i].GetComponent<MeshRenderer>().material=stopMaterial;
            }
        }
    }

    private void Awake()
    {
        points=new List<Vector2>();
        lines=new HashSet<Line>();
        triangleMeshs=new List<TriangleMesh>();
        lineTriangleDictionary=new Dictionary<Line,List<TriangleMesh>>();
        pointIndexTriangleDictionary=new Dictionary<int, List<TriangleMesh>>();
        pointPrefabs = new List<GameObject>();
        camera=Camera.main;
    }

    void Start()
    {
        camera.transform.position = new Vector3(size / 2, size / 2, -10);
        camera.orthographicSize=size/2;
        
        InittrianglePrefab();

        // InitMesh();
        
        startTriangulation.onClick.AddListener(() =>
        {
            flag = true;
            InitMesh();
        });
        clear.onClick.AddListener(() =>
        {
            points.Clear();
            lines.Clear();
            while(triangleMeshs.Count > 0)
            {
                Destroy(triangleMeshs[^1].gameObject);
                triangleMeshs.RemoveAt(triangleMeshs.Count - 1);
            }
            lineTriangleDictionary.Clear();
            pointIndexTriangleDictionary.Clear();
            while(pointPrefabs.Count > 0)
            {
                Destroy(pointPrefabs[^1]);
                pointPrefabs.RemoveAt(pointPrefabs.Count - 1);
            }

            flag = false;
        });
    }

    void Update()
    {
        if (!flag)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Vector3 mousePosition = UnityEngine.Input.mousePosition;
                    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    GameObject gameObject = Instantiate(pointPrefab,canvas.transform);
                    pointPrefabs.Add(gameObject);
                    gameObject.transform.position = (Vector2)mousePosition;
                    points.Add(worldPosition);
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    points.RemoveAt(points.Count - 1);
                    Destroy(pointPrefabs[^1]);
                    pointPrefabs.RemoveAt(pointPrefabs.Count - 1);
                }
            }
        }
        else
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Vector3 mousePosition = UnityEngine.Input.mousePosition;
                    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    Vector2Int pointer=new Vector2Int((int)worldPosition.x, (int)worldPosition.y);
                    // UpdateMesh(worldPosition);
                    largeBuilding.center = pointer;
                    UpdateMesh(largeBuilding.Vertices());
                }
;
            }
            
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    // DeletePoint(points.Count-1);
                    DeleteLastPoints(largeBuilding.Vertices().Count);
                }
            }
        }
    }
}
