using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnResourceTekst : MonoBehaviour
{
    [SerializeField] private List<ResourceView>ProduceCost = new List<ResourceView> ();
    [SerializeField] private List<Sprite> _sprites = new List<Sprite> ();
    

    [SerializeField] Color _colorpayed;
    [SerializeField] Color _colorGained;
    public enum Resources
    {
        wood,
        stone,
        people,
        food
    }

    public void ShowCostBuilding()
    {
       Building build = GetComponent<Building>();

        int StoneCost = build._buildingData._buildingLevelData[build.BuildingLevel]._stoneCost;
        int WoodCost = build._buildingData._buildingLevelData[build.BuildingLevel]._woodCost;
        int PeopleCost = build._buildingData._buildingLevelData[build.BuildingLevel]._peopleCost;

        if (StoneCost > 0)
        {
            SpawnUI("-"+ StoneCost.ToString(), _colorpayed, Resources.stone);
        }
        if (WoodCost > 0)
        {
            SpawnUI("-" + WoodCost.ToString(), _colorpayed, Resources.wood);
        }
        if (PeopleCost > 0)
        {
            SpawnUI("-" + PeopleCost.ToString(), _colorpayed, Resources.people);
        }
    }
    public void ShowrepairBuilding()
    {
        Building build = GetComponent<Building>();

        int StoneCost = build._buildingData._buildingLevelData[build.BuildingLevel]._stoneCost /2;
        int WoodCost = build._buildingData._buildingLevelData[build.BuildingLevel]._woodCost/2;
        int PeopleCost = build._buildingData._buildingLevelData[build.BuildingLevel]._peopleCost/2;

        if (StoneCost > 0)
        {
            SpawnUI("-" + StoneCost.ToString(), _colorpayed, Resources.stone);
        }
        if (WoodCost > 0)
        {
            SpawnUI("-" + WoodCost.ToString(), _colorpayed, Resources.wood);
        }
        if (PeopleCost > 0)
        {
            SpawnUI("-" + PeopleCost.ToString(), _colorpayed, Resources.people);
        }
    }
    public void ShowProduce( int amount, ResourceManager.ResourceType rs)
    {
        switch (rs)
        {
            case ResourceManager.ResourceType.Wood:
                SpawnUI("+" +amount.ToString(), _colorGained, Resources.wood);
                break;

            case ResourceManager.ResourceType.Stone:
                SpawnUI("+" + amount.ToString(), _colorGained, Resources.stone);
                break;

            case ResourceManager.ResourceType.People:
                SpawnUI("+" + amount.ToString(), _colorGained, Resources.people);
                break;

                case ResourceManager.ResourceType.Food:
                SpawnUI("+" + amount.ToString(), _colorGained, Resources.food);
                break;
        }

    }

    public void SpawnUI(string amount, Color color, Resources res)
    {
        
        foreach(var ui in ProduceCost)
        {
            if(!ui.gameObject.activeInHierarchy)
            {
                ui._lifeTime = 0;
                ui.SetText(amount.ToString());
                ui.SetColor(color);
                ui.SetImage(_sprites[(int)res]);
                ui.gameObject.SetActive(true);
                break;
            }
        }
    }


}
