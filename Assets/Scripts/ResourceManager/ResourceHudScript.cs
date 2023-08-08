using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ResourceHudScript : MonoBehaviour
{


    [SerializeField] private GameObject _woodUI;
    [SerializeField] private GameObject _stoneUI;
    [SerializeField] private GameObject _foodUI;
    [SerializeField] private GameObject _peopleUI;
    [SerializeField] private GameObject _warningUI;
    [SerializeField] private GameObject _warningUIText;

    private TextMeshProUGUI _woodUIText;
    private TextMeshProUGUI _stoneUIText;
    private TextMeshProUGUI _foodUIText;
    private TextMeshProUGUI _peopleUIText;
    private Image _warningImage;


    private ResourceManager _resourceManager;


    // Warning logic
    private bool _flashingUp = true;
    private float _currentOpacity = 0.0f;
    private float _maxOpacity = 0.6f;
    private float _minOpacity = 0.2f;
    private float _flashSpeed = 0.5f;


    public void UpdateHud()
    {
        string woodText = _resourceManager.WoodAmount + "/" + _resourceManager.WoodMaxAmount;
        string stoneText = _resourceManager.StoneAmount + "/" + _resourceManager.StoneMaxAmount;
        string foodText = _resourceManager.FoodAmount + "/" + _resourceManager.FoodMaxAmount;
        string peopleText = _resourceManager.PeopleAmount + "/" + _resourceManager.PeopleMaxAmount;



        _woodUIText?.SetText(woodText);
        _stoneUIText?.SetText(stoneText);
        _foodUIText?.SetText(foodText);

        _peopleUIText?.SetText(peopleText);


    }

    private void Awake()
    {
        _woodUIText = _woodUI?.GetComponentInChildren<TextMeshProUGUI>();
        _stoneUIText = _stoneUI?.GetComponentInChildren<TextMeshProUGUI>();
        _foodUIText = _foodUI?.GetComponentInChildren<TextMeshProUGUI>();
        _peopleUIText = _peopleUI?.GetComponentInChildren<TextMeshProUGUI>();
        _warningImage = _warningUI?.GetComponentInChildren<Image>();

        _resourceManager = FindObjectOfType<ResourceManager>();
        _resourceManager.OnResourceChange.AddListener(UpdateHud);
    }

    private void Update()
    {
        if(GameLoop.CurrentGamestate != GameLoop.GameState.Game)
        {
            return;
        }
        if (!_resourceManager.HasFoodToFeed())
        {
            _foodUIText.color = Color.red;
            _warningUI.SetActive(true);
            _warningUIText.SetActive(true);
           
            if (_currentOpacity > _maxOpacity && _flashingUp)
            {
                _currentOpacity = _maxOpacity;
                _flashingUp = false;
            }
            else if (_currentOpacity < _minOpacity && !_flashingUp)
            {

                _currentOpacity = _minOpacity;
                _flashingUp = true;
            }

            if (_flashingUp)
            {

                _currentOpacity += Time.deltaTime * _flashSpeed;
            }
            else
            {
                _currentOpacity -= Time.deltaTime * _flashSpeed;

            }
            Color newColor = _warningImage.color;
            newColor.a = _currentOpacity;
            _warningImage.color = newColor;
        }
        else
        {

            _foodUIText.color = Color.white;
            _warningUIText.SetActive(false);
            _warningUI.SetActive(false);
        }
    }

}
