using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Map : MonoBehaviour
{
    public DigitalMesh digitalMesh;
    public List<TriangleMesh> triangleMeshes;
    public BuildingSystem buildingSystem;
    public GameObject trianglePrefab;
    public Material material;
    public int num;
    public int size;
    public float edgeWidth;
    
    public Canvas canvas;
    private List<GameObject> pointPrefabs = new List<GameObject>();
    public GameObject pointPrefab;
    public bool haveCreateMap = false;
    
    public List<Vector2> pointsBuffer;

    void InstantiateTriangleMesh(Triangle triangle)
    {
        GameObject newTriangle = Instantiate(trianglePrefab,transform);
        TriangleMesh newTriangleMesh=newTriangle.GetComponent<TriangleMesh>();
        newTriangleMesh.Init(triangle);
        triangleMeshes.Add(newTriangleMesh);
    }

    public void Present()
    {
        while (triangleMeshes.Count > 0)
        {
            Destroy(triangleMeshes[^1].gameObject);
            triangleMeshes.RemoveAt(triangleMeshes.Count - 1);
        }

        foreach (Triangle triangle in digitalMesh.triangles)
        {
            InstantiateTriangleMesh(triangle);
        }
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

    public void StartTrianglationCallBack()
    {
        buildingSystem.enabled = haveCreateMap = true;
        digitalMesh.Init(pointsBuffer);
        Present();
    }

    public void ClearTrianglationCallBack()
    {
        digitalMesh=new DigitalMesh();
        while(triangleMeshes.Count > 0)
        {
            Destroy(triangleMeshes[^1].gameObject);
            triangleMeshes.RemoveAt(triangleMeshes.Count - 1);
        }
        while(pointPrefabs.Count > 0)
        {
            Destroy(pointPrefabs[^1]);
            pointPrefabs.RemoveAt(pointPrefabs.Count - 1);
        }

        buildingSystem.Clear();
        buildingSystem.enabled = haveCreateMap = false;
    }

    private void Awake()
    {
        digitalMesh=new DigitalMesh();
        triangleMeshes=new List<TriangleMesh>();
        pointPrefabs = new List<GameObject>();
        buildingSystem.enabled = haveCreateMap;
    }

    void Start()
    {
        Camera.main.transform.position = new Vector3(size / 2, size / 2, -10);
        Camera.main.orthographicSize=size/2;
        
        InittrianglePrefab();
    }

    void Update()
    {
        if (!haveCreateMap)
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
                }
            }
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Destroy(pointPrefabs[^1]);
                    pointPrefabs.RemoveAt(pointPrefabs.Count - 1);
                    pointsBuffer.RemoveAt(pointsBuffer.Count-1);
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
            //         digitalMesh.ConstrainedTriangulation(buildingSystem.curBuildingData.Vertices(pointer));
            //         // Building building = new Building(digitalMesh.ConstrainedTriangulation(buildingSystem.curBuildingData.Vertices(pointer)));
            //         // ConnectPositionToBuilding(buildingSystem.curBuildingData.Area(pointer),building);
            //         Present();
            //     }
            // }
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
