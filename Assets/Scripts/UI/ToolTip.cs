using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using HPTM;

public class ToolTip : MonoBehaviour
{
    [SerializeField] GameObject _benefit;
    [SerializeField] TMP_Text _production;
    [SerializeField] Image _productionIcon;
    [Header("Resources")]
    [SerializeField] TMP_Text _wood;
    [SerializeField] TMP_Text _stone;
    [SerializeField] TMP_Text _people;

    [SerializeField] private Color _notEnoughResources = Color.red;
    [SerializeField] private Color _enoughResources = Color.white;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Show()
    {
        _animator.ResetTrigger("Close");
        _animator.SetTrigger("Open");
    }

    public void Hide()
    {
        _animator.ResetTrigger("Open");
        _animator.SetTrigger("Close");
    }

    public bool IsShowing()
    {
        return _animator.GetBool("Open");
    }

    public void UpdateOverlay(Building _building, BuildingData buildingData, int level, ResourceManager resourceManager)
    {  
        _benefit.SetActive(buildingData._placementType != Builder.PlacementType.Edge);

        BuildingLevelData bld = buildingData._buildingLevelData[level + 1];

        _production.gameObject.SetActive(false);
        if (buildingData._isProducer)
        {
            _production.gameObject.SetActive(true);
            float multiplier = 4f / 24.0f;
            if(!buildingData._endlessProduction)
            {
                multiplier = 1;
            }
            _production.text = Mathf.RoundToInt(bld._amountProduced * _building.GetComponent<ResourceGenerator>().ResourceEfficiency * multiplier).ToString();
            _productionIcon.sprite = buildingData._productionResourceIcon;
        }


        _wood.text = bld._woodCost.ToString();
        CheckIfPlayerHasEnoughResources(resourceManager.WoodAmount, bld._woodCost, _wood);

        _stone.text = bld._stoneCost.ToString();
        CheckIfPlayerHasEnoughResources(resourceManager.StoneAmount, bld._stoneCost, _stone);

        _people.text = bld._peopleCost.ToString();
        CheckIfPlayerHasEnoughResources(resourceManager.PeopleAmount, bld._peopleCost, _people);
    }

    private void CheckIfPlayerHasEnoughResources(int PlayerAmount, int needed, TMP_Text text)
    {
        if (PlayerAmount < needed)
        {
            text.color = _notEnoughResources;
        }
        else
        {
            text.color = _enoughResources;
        }
    }
}
