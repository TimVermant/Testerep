using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradePopUp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ToolTip _toolTip;
    public BuildingData _upgradeData { set; private get; }
    public Building _building {set; private get;}
    public int _level { set; private get; }
    private ResourceManager _resourceManager;
    private Button _button;
    public ToolTip GetToolTip()
    {
        return _toolTip;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _resourceManager = FindObjectOfType<ResourceManager>();
    }

    private void Start()
    {
        _toolTip.Hide();
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        if (!_upgradeData)
        {
            return;
        }

        _button.interactable = _resourceManager.HasResourcesToBuild(_upgradeData._buildingLevelData[_level + 1]);
        
        if (_toolTip.IsShowing())
            _toolTip.UpdateOverlay(_building, _upgradeData, _level, _resourceManager);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (!_upgradeData)
        {
            return;
        }

        _toolTip.Show();
        _toolTip.UpdateOverlay(_building, _upgradeData, _level, _resourceManager);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        _toolTip.Hide();
    }
}
