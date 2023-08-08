using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolTipDestroy : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] TMP_Text _wood;
    [SerializeField] TMP_Text _stone;
    [SerializeField] TMP_Text _people;

    public void UpdateOverlay(string name, BuildingData buildingData, int level, ResourceManager resourceManager)
    {
        BuildingLevelData bld = buildingData._buildingLevelData[level];

        _wood.text = "+" + (bld._woodCost / 2).ToString();
        _stone.text = "+" + (bld._stoneCost / 2).ToString();
        _people.text = "+" + bld._peopleCost.ToString();
    }

    public void SetResourceValue(ResourceManager.ResourceType resourceType, int amount)
    {
        switch (resourceType)
        {
            case ResourceManager.ResourceType.Wood:
                _wood.text = amount.ToString(); break;
            case ResourceManager.ResourceType.Stone:
                _stone.text = amount.ToString(); break;
            case ResourceManager.ResourceType.People:
                _people.text = amount.ToString(); break;
        }
    }
}
