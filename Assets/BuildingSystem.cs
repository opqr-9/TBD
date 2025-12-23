using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingSystem : MonoBehaviour
{
    public Map map;
    private DigitalMesh digitalMesh;
    
    private Dictionary<Vector2Int, Building> positionBuildingDictionary;
    
    public Dictionary<Vector2, int> repeatPointNumDictionary;
    
    private List<BuildingData> buildingDatas = new List<BuildingData>
    {
        new Octagon(5, 1.5f),
        new Octagon(5, 1.5f),
        new Octagon(5, 1.5f),
        new Octagon(5, 1.5f)
        
    };

    public GameObject buildingPrefab;
    
    private BuildingData curBuildingData;
    private GameObject curBuildingPrefab;
    private LineRenderer curLineRenderer;

    public Material material;

    private void InitBuildingPrefab()
    {
        LineRenderer lineRenderer = buildingPrefab.GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = lineRenderer.endColor = Color.black;
        lineRenderer.material = material;
    }
        

    private void AssignlineRenderer(int index,LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = buildingDatas[index].Contour(Vector2Int.zero).Count;
        lineRenderer.SetPositions(buildingDatas[index].Contour(Vector2Int.zero).ToArray());
    }

    private void UpdateBuilding(int index)
    {
        Destroy(curBuildingPrefab);
        if (index == -1)
        {
            return;
        }
        curBuildingPrefab = Instantiate(buildingPrefab);
        curLineRenderer=curBuildingPrefab.GetComponent<LineRenderer>();
        AssignlineRenderer(index,curLineRenderer);
    }
    
    private void ConnectPositionToBuilding(List<Vector2Int> positions,Building building)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            positionBuildingDictionary.Add(positions[i], building);
        }
    }

    private void RemovePositionToBuilding(Building building)
    {
        for (int i = 0; i < building.area.Count; i++)
        {
            positionBuildingDictionary.Remove(building.area[i]);
        }
    }

    private bool CheckPlacable(Vector2Int position)
    {
        List<Vector2Int> positions = curBuildingData.Area(position);
        for (int i = 0; i < positions.Count; i++)
        {
            if (positionBuildingDictionary.ContainsKey(positions[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void Clear()
    {
        positionBuildingDictionary.Clear();
    }

    public void ChooseBuildingCallBack(int index)
    {
        curBuildingData=buildingDatas[index];
        UpdateBuilding(index);
    }

    public void Start()
    {
        positionBuildingDictionary=new Dictionary<Vector2Int, Building>();
        repeatPointNumDictionary=new Dictionary<Vector2, int>();
        InitBuildingPrefab();
        digitalMesh = map.digitalMesh;
    }

    public void Update()
    {
        if (curBuildingPrefab != null)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                UpdateBuilding(-1);
            }
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 position = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                curBuildingPrefab.transform.position = new Vector3Int((int)position.x, (int)position.y, 0);
                Vector2Int pointer = new Vector2Int((int)position.x, (int)position.y);
                if (CheckPlacable(pointer))
                {
                    curLineRenderer.startColor = curLineRenderer.endColor = Color.green;
                    if (UnityEngine.Input.GetMouseButtonDown(0))
                    {
                        List<Vector2> vertices=curBuildingData.Vertices(pointer);
                        for (int i = 0; i < vertices.Count; i++)
                        {
                            if (digitalMesh.points.ContainsValue(vertices[i]))
                            {
                                if (!repeatPointNumDictionary.ContainsKey(vertices[i]))
                                {
                                    repeatPointNumDictionary.Add(vertices[i],1);
                                }
                                else
                                {
                                    repeatPointNumDictionary[vertices[i]]++;
                                }
                                vertices.RemoveAt(i);
                                i--;
                            }
                        }
                        Building building = new Building(digitalMesh.ConstrainedTriangulation(vertices),curBuildingData.Area(pointer));
                        ConnectPositionToBuilding(curBuildingData.Area(pointer),building);
                        map.Present();
                    }
                }
                else
                {
                    curLineRenderer.startColor = curLineRenderer.endColor = Color.red;
                    // if (UnityEngine.Input.GetMouseButtonDown(1))
                    // {
                    //     RemovePositionToBuilding(positionBuildingDictionary[pointer]);
                    //     // map.digitalMesh.re
                    //     map.Present();
                    // }
                }
            }
        }
    }
    
    
}
