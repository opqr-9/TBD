using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Map map;
    public BuildingSystem buildingSystem;
        
    public Button clear;
    public Button startTriangulation;
    
    public List<Button> buttons;

    public void Start()
    {
        startTriangulation.onClick.AddListener(map.StartTrianglationCallBack);
        clear.onClick.AddListener(map.ClearTrianglationCallBack);
        
        for (int i = 0; i<buttons.Count; i++)
        {
            int tmp = i;
            buttons[tmp].onClick.AddListener(() =>
            {
                buildingSystem.ChooseBuildingCallBack(tmp);
            });
        }
    }
}
