using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Map : MonoBehaviour
{
    public static DigitalMesh digitalMesh;
    public static List<TriangleMesh> triangleMeshs;
    public GameObject trianglePrefab;
    public Material material;
    public Material stopMaterial;
    public int num;
    public int size;
    public float edgeWidth;
    private Camera camera;

    private float time = 0;
    
    private static Building largeBuilding = new Building(5, Vector2Int.zero, 1.5f);
    private List<Vector2> pointsBuffer = new List<Vector2>();
    public Canvas canvas;
    private List<GameObject> pointPrefabs = new List<GameObject>();
    public GameObject pointPrefab;
    public Button clear;
    public Button startTriangulation;
    public bool flag = false;

    void InstantiateTriangleMesh(Triangle triangle)
    {
        GameObject newTriangle = Instantiate(trianglePrefab,transform);
        newTriangle.GetComponent<TriangleMesh>().Init(triangle);
    }

    // List<Line> ConstrainedLines(List<int> pointsIndices)
    // {
    //     List<Line> constrainedLines = new List<Line>();
    //     for (int i = 0; i < pointsIndices.Count; i++)
    //     {
    //         Line l = new Line(pointsIndices[i],pointsIndices[(i + 1)%pointsIndices.Count]);
    //         if (!lines.Contains(l))
    //         {
    //             constrainedLines.Add(l);
    //         }
    //     }
    //     return  constrainedLines;
    // }
    //
    // void ConstrainedTriangulation(List<int> pointsIndices,List<Line> constrainedLines,bool keepInside)
    // {
    //     for (int i = 0; i < pointsIndices.Count; i++)
    //     {
    //         Triangulation(pointsIndices[i]);
    //     }
    //
    //     Queue<Line> conflitLines = new Queue<Line>();
    //     for (int cnt = 0; cnt < constrainedLines.Count; cnt++)
    //     {
    //         foreach (var l in lines)
    //         {
    //             if (l.CheckLine(constrainedLines[cnt]))
    //             {
    //                 conflitLines.Enqueue(l);
    //             }
    //         }
    //
    //         while (conflitLines.Count > 0)
    //         {
    //             List<int> indices = new List<int>();
    //             indices.Add(conflitLines.Peek().minpointIndex);
    //             indices.Add(conflitLines.Peek().maxpointIndex);
    //             for (int j = 0; j < 3; j++)
    //             {
    //                 for (int k = 0; k < indices.Count; k++)
    //                 {
    //                     if (indices[k] != lineTriangleDictionary[conflitLines.Peek()][0].triangle.verticesIndices[j])
    //                     {
    //                         indices.Add(lineTriangleDictionary[conflitLines.Peek()][0].triangle.verticesIndices[j]);
    //                     }
    //
    //                     if (indices[k] != lineTriangleDictionary[conflitLines.Peek()][1].triangle.verticesIndices[j])
    //                     {
    //                         indices.Add(lineTriangleDictionary[conflitLines.Peek()][1].triangle.verticesIndices[j]);
    //                     }
    //                 }
    //             }
    //
    //             Line line = new Line(indices[2], indices[3]);
    //             if (line.CheckLine(constrainedLines[cnt]))
    //             {
    //                 conflitLines.Enqueue(line);
    //             }
    //
    //             lineTriangleDictionary[conflitLines.Peek()][0].Remove();
    //             lineTriangleDictionary[conflitLines.Peek()][1].Remove();
    //             InstantiateTriangleMesh(indices[0], indices[2], indices[3]);
    //             InstantiateTriangleMesh(indices[1], indices[2], indices[3]);
    //             conflitLines.Dequeue();
    //         }
    //     }
    // }
    //
    // void DeleteLastPoints(int length)
    // {
    //     for (int i = 0; i < length; i++) 
    //     {
    //         DeletePoint(points.Count-1);
    //         points.RemoveAt(points.Count-1);
    //     }
    // }
    //
    // void DeletePoint(int pointIndex)
    // {
    //     HashSet<int> hashIndices=new HashSet<int>();
    //     List<int> emptyHolePointsIndices=new List<int>();
    //     for (int i = 0; i < pointIndexTriangleDictionary[pointIndex].Count; i++)
    //     {
    //         for (int j = 0; j < 3; j++)
    //         {
    //             hashIndices.Add(pointIndexTriangleDictionary[pointIndex][i].triangle.verticesIndices[j]);
    //         }
    //         pointIndexTriangleDictionary[pointIndex][i].Remove();
    //         i--;
    //     }
    //     hashIndices.Remove(pointIndex);
    //     foreach (int vindices in hashIndices)
    //     {
    //         emptyHolePointsIndices.Add(vindices);
    //     }
    //     EmptyHoleTriangulation(emptyHolePointsIndices);
    // }
    //
    // List<Triangle> CreateSurroundBox(List<Vector2> points)
    // {
    //     Vector2 leftdown=points[0], rightup=points[0];
    //     for (int i = 0; i < points.Count; i++)
    //     {
    //         leftdown = Vector2.Min((points[i]), leftdown);
    //         rightup = Vector2.Max((points[i]), rightup);
    //     }
    //     leftdown-=Vector2.one;
    //     rightup+=Vector2.one;
    //     List<Vector2> tmpPoints = new List<Vector2>();
    //     tmpPoints.Add(leftdown);
    //     tmpPoints.Add(new Vector2(rightup.x, leftdown.y));
    //     tmpPoints.Add(rightup);
    //     tmpPoints.Add(new Vector2(leftdown.x, rightup.y));
    //     List<Triangle> triangles = new List<Triangle>();
    //     Triangle triangle1 = new Triangle(0,1,2,tmpPoints);
    //     Triangle triangle2 = new Triangle(0,2,3,tmpPoints);
    //     triangles.Add(triangle1);
    //     triangles.Add(triangle2);
    //     return triangles;
    // }
    //
    // void ConstrainedTriangulation(List<Vector2> points, List<Line> constrainedLines, int pointIndex,
    //     List<Triangle> triangles = null)
    // {
    //     if (triangles == null)
    //     {
    //         triangles=CreateSurroundBox(points);
    //     }
    //     
    //     HashSet<int> hashIndices=new HashSet<int>();
    //     List<int> newPointsIndices=new List<int>();
    //     for (int i = 0; i < triangles.Count; i++)
    //     {
    //         if (triangles[i].circumcircle.Check(points[pointIndex]))
    //         {
    //             for (int j = 0; j < 3; j++)
    //             {
    //                 hashIndices.Add(triangles[i].verticesIndices[j]);
    //             }
    //             triangles.RemoveAt(i);
    //             i--;
    //         }
    //     }
    //     
    //     foreach (int vindices in hashIndices)
    //     {
    //         newPointsIndices.Add(vindices);
    //     }
    //     
    //     Clockwise(points,newPointsIndices,pointIndex);
    //     
    //     for (int i = 0; i < newPointsIndices.Count; i++)
    //     {
    //         if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
    //                 points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
    //         {
    //             Triangle triangle = new Triangle(newPointsIndices[i],
    //                 newPointsIndices[(i + 1) % newPointsIndices.Count],
    //                 pointIndex, points);
    //             triangles.Add(triangle);
    //         }
    //     }
    // }
    //
    // void EmptyHoleTriangulation(List<int> pointsIndices)
    // {
    //     Vector2 leftdown=points[pointsIndices[0]], rightup=points[pointsIndices[0]];
    //     for (int i = 0; i < pointsIndices.Count; i++)
    //     {
    //         leftdown = Vector2.Min((points[pointsIndices[i]]), leftdown);
    //         rightup = Vector2.Max((points[pointsIndices[i]]), rightup);
    //     }
    //     leftdown-=Vector2.one;
    //     rightup+=Vector2.one;
    //     List<Vector2> tmpPoints = new List<Vector2>();
    //     tmpPoints.Add(leftdown);
    //     tmpPoints.Add(new Vector2(rightup.x, leftdown.y));
    //     tmpPoints.Add(rightup);
    //     tmpPoints.Add(new Vector2(leftdown.x, rightup.y));
    //     List<Triangle> triangles = new List<Triangle>();
    //     Triangle triangle1 = new Triangle(0,1,2,tmpPoints);
    //     Triangle triangle2 = new Triangle(0,2,3,tmpPoints);
    //     triangles.Add(triangle1);
    //     triangles.Add(triangle2);
    //     for (int i = 0; i < pointsIndices.Count; i++)
    //     {
    //         tmpPoints.Add(points[pointsIndices[i]]);
    //         SeparatedTriangulation(tmpPoints,triangles,i+4);
    //     }
    //
    //     for (int i = 0; i < triangles.Count; i++)
    //     {
    //         bool flag = true;
    //         for (int j = 0; j < 3; j++)
    //         {
    //             if (triangles[i].verticesIndices[j] < 4)
    //             {
    //                 flag = false;
    //                 break;
    //             }
    //         }
    //
    //         if (flag)
    //         {
    //             InstantiateTriangleMesh(pointsIndices[triangles[i].verticesIndices[0] - 4],
    //                 pointsIndices[triangles[i].verticesIndices[1] - 4],
    //                 pointsIndices[triangles[i].verticesIndices[2] - 4]);
    //         }
    //     }
    // }
    //
    // void SeparatedTriangulation(List<Vector2> points,List<Triangle> triangles, int pointIndex)
    // {
    //     HashSet<int> hashIndices=new HashSet<int>();
    //     List<int> newPointsIndices=new List<int>();
    //     for (int i = 0; i < triangles.Count; i++)
    //     {
    //         if (triangles[i].circumcircle.Check(points[pointIndex]))
    //         {
    //             for (int j = 0; j < 3; j++)
    //             {
    //                 hashIndices.Add(triangles[i].verticesIndices[j]);
    //             }
    //             triangles.RemoveAt(i);
    //             i--;
    //         }
    //     }
    //
    //     foreach (int vindices in hashIndices)
    //     {
    //         newPointsIndices.Add(vindices);
    //     }
    //     
    //     Clockwise(points,newPointsIndices,pointIndex);
    //     
    //     for (int i = 0; i < newPointsIndices.Count; i++)
    //     {
    //         if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
    //                 points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
    //         {
    //             Triangle triangle = new Triangle(newPointsIndices[i],
    //                 newPointsIndices[(i + 1) % newPointsIndices.Count],
    //                 pointIndex, points);
    //             triangles.Add(triangle);
    //         }
    //     }
    // }

    
    void InitMesh()
    {
        
        // EarCut();
        // Vector2 min = new Vector2(0, 0);
        // Vector2 max = new Vector2(size, size);
        // points.Add(min);
        // points.Add(max);
        // points.Add(new Vector2(0, size));
        // points.Add(new Vector2(size, 0));
        // InstantiateTriangleMesh(0,1,2);
        // InstantiateTriangleMesh(0,1,3);
    }

    void Present()
    {
        while (triangleMeshs.Count > 0)
        {
            Destroy(triangleMeshs[^1].gameObject);
            triangleMeshs.RemoveAt(triangleMeshs.Count - 1);
        }

        foreach (Triangle triangle in digitalMesh.triangles)
        {
            InstantiateTriangleMesh(triangle);
        }
    }

    void UpdateMesh(Vector2 point)
    {
        // digitalMesh.Triangulation(point);
        // for (int i = 0; i < points.Count; i++)
        // {
        //     if (Mathf.Abs(points[i].x - point.x) < 0.01f && Mathf.Abs(points[i].y - point.y) < 0.01f)
        //     {
        //         return;
        //     }
        // }
        // points.Add(point);
        // Triangulation(points.Count-1);
    }

    // void UpdateMesh(List<Vector2> pointSet)
    // {
    //     List<int> verticesIndices = new List<int>();
    //     for (int i = 0; i < pointSet.Count; i++)
    //     {
    //         bool flag = true;
    //         for (int j = 0; j < points.Count; j++)
    //         {
    //             if (Mathf.Abs(points[j].x - pointSet[i].x) < 0.01f && Mathf.Abs(points[j].y - pointSet[i].y) < 0.01f)
    //             {
    //                 verticesIndices.Add(j);
    //                 flag = false;
    //                 break;
    //             }
    //         }
    //         if (flag)
    //         {
    //             points.Add(pointSet[i]);
    //             verticesIndices.Add(points.Count-1);
    //         }
    //     }
    //     List<Line> constrainedLines = ConstrainedLines(verticesIndices);
    //     ConstrainedTriangulation(verticesIndices,constrainedLines,true);
    // }

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

    private void Awake()
    {
        digitalMesh=new DigitalMesh();
        triangleMeshs=new List<TriangleMesh>();
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
            // digitalMesh.EarCut();
            digitalMesh.Init(pointsBuffer);
            pointsBuffer.Clear();
            Present();
        });
        clear.onClick.AddListener(() =>
        {
            digitalMesh=new DigitalMesh();
            // points.Clear();
            // lines.Clear();
            while(triangleMeshs.Count > 0)
            {
                Destroy(triangleMeshs[^1].gameObject);
                triangleMeshs.RemoveAt(triangleMeshs.Count - 1);
            }
            // lineTriangleDictionary.Clear();
            // pointIndexTriangleDictionary.Clear();
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
                    GameObject gameObject = Instantiate(pointPrefab,(Vector2)mousePosition,new Quaternion(0,0,0,0),canvas.transform);
                    pointPrefabs.Add(gameObject);
                    pointsBuffer.Add(worldPosition);
                    // digitalMesh.points.Add(worldPosition);
                }
            }

            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Destroy(pointPrefabs[^1]);
                    pointPrefabs.RemoveAt(pointPrefabs.Count - 1);
                    pointsBuffer.RemoveAt(pointsBuffer.Count-1);
                    // digitalMesh.points.RemoveAt(digitalMesh.points.Count-1);
                }
            }
        }
        else
        {
             // if (UnityEngine.Input.GetMouseButtonDown(0))
             // {
             //     if (!EventSystem.current.IsPointerOverGameObject())
             //     {
             //         Vector3 mousePosition = UnityEngine.Input.mousePosition;
             //         Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
             //         Vector2Int pointer=new Vector2Int((int)worldPosition.x, (int)worldPosition.y);
             //         // UpdateMesh(worldPosition);
             //         largeBuilding.center = pointer;
             //         digitalMesh.points.Add(pointer);
             //         digitalMesh.Triangulation(digitalMesh.points.Count-1);
             //         Present();
             //     }
             //
             // }
             
             // if (UnityEngine.Input.GetMouseButtonDown(1))
             // {
             //     if (!EventSystem.current.IsPointerOverGameObject())
             //     {
             //         DeletePoint(points.Count-1);
             //         // DeleteLastPoints(largeBuilding.Vertices().Count);
             //         
             //         Present();
             //     }
             // }
        }
    }
}
