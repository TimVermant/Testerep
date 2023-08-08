using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : MonoBehaviour
{
    public enum ResourceType
    {
        Wood,
        Stone,
        Food,
        People,
        Capacity,

    }
    // ALL POSSIBLE RESOURCES

    // Wood
    public int WoodAmount
    {
        get { return _woodAmount; }
    }
    public int WoodMaxAmount
    {
        get { return _woodMaxAmount; }
    }

    // Stone
    public int StoneAmount
    {
        get { return _stoneAmount; }
    }
    public int StoneMaxAmount
    {
        get { return _stoneMaxAmount; }
    }

    // Food
    public int FoodAmount
    {
        get { return _foodAmount; }
    }
    public int FoodMaxAmount
    {
        get { return _foodMaxAmount; }
    }

    // People
    public int PeopleAmount
    {
        get { return _peopleAmount; }
    }

    public int PeopleMaxAmount
    {
        get { return _peopleMaxAmount; }
    }

    // Starving
    public int StarveCounter
    {
        get { return _starveCounter; }
    }




    public UnityEvent OnResourceChange
    {
        get { return _onResourceChange; }
    }


    private int _woodAmount;
    private int _stoneAmount;
    private int _foodAmount;
    private int _peopleAmount;

    // Variables used to see if the game needs to end
    private int _starveCounter = 0;
    private int _maxStarveCounter = 2;

    [Header("FoodConsumption")]
    [SerializeField] private bool _enableConsumption = true;
    [SerializeField] private float _consumptionMultiplier = 1f;
    [SerializeField] private float _consumptionMultiplierMin = 0.6f;
    [SerializeField] private float _consumptionMultiplierMax = 3f;
    public float ConsumptionMultiplier 
    {
        set 
        { 
            _consumptionMultiplier = value; 
            _consumptionMultiplier = Mathf.Clamp(_consumptionMultiplier, _consumptionMultiplierMin, _consumptionMultiplierMax);
        }
        get { return _consumptionMultiplier; }
    }

    [Header("Starter amounts")]
    [SerializeField] private int _woodStarterAmount = 100;
    [SerializeField] private int _stoneStarterAmount = 20;
    [SerializeField] private int _peopleStarterAmount = 20;
    [SerializeField] private int _foodStarterAmount = 100;

    [Header("Max amounts")]
    [SerializeField] private int _woodMaxAmount = 200;
    [SerializeField] private int _stoneMaxAmount = 200;
    [SerializeField] private int _foodMaxAmount = 200;
    [SerializeField] private int _peopleMaxAmount = 80;


    public int GetConsumption()
    {
        return (int)(_peopleMaxAmount * _consumptionMultiplier);
    }


    private UnityEvent _onResourceChange = new();

    public void SetStartingAmount(BuildingData buildingData)
    {
        if (buildingData._buildingName != "MainHall")
        {
            return;
        }
        _woodAmount = _woodStarterAmount;
        _stoneAmount = _stoneStarterAmount;
        _foodAmount = _foodStarterAmount;
        _peopleAmount = _peopleStarterAmount;

        // At the beginning the amount of available people == the starting amount of people
        _peopleMaxAmount = _peopleAmount;
        _starveCounter = 0;

        _onResourceChange.Invoke();
        FindObjectOfType<Builder>()._onBuildingPlace.RemoveListener(SetStartingAmount);
    }


    // RESOURCE FUNCTIONALITY
    public bool HasResourcesToBuild(BuildingLevelData buildingLevelData)
    {

        return buildingLevelData._woodCost <= _woodAmount &&
            buildingLevelData._stoneCost <= _stoneAmount &&
            buildingLevelData._peopleCost <= _peopleAmount;
    }

    public bool HasResourcesToRepair(BuildingLevelData buildingLevelData)
    {

        return buildingLevelData._woodCost * 0.5f<= _woodAmount &&
            buildingLevelData._stoneCost * 0.5f <= _stoneAmount;
    }


    public bool ConsumeResourcesToBuild(BuildingLevelData buildingLevelData)
    {
        if (HasResourcesToBuild(buildingLevelData))
        {
            _woodAmount -= buildingLevelData._woodCost;
            _stoneAmount -= buildingLevelData._stoneCost;
            _peopleAmount -= buildingLevelData._peopleCost;


            _onResourceChange.Invoke();
            return true;
        }
        else
        {
            Debug.LogError("Not enough available resources!");
            return false;
        }
    }

    public bool ConsumeResourcesToRepair(BuildingLevelData buildingLevelData)
    {
        if (HasResourcesToBuild(buildingLevelData))
        {
            _woodAmount -= Mathf.FloorToInt(buildingLevelData._woodCost * 0.5f);
            _stoneAmount-= Mathf.FloorToInt(buildingLevelData._stoneCost * 0.5f);
            _peopleAmount -= Mathf.FloorToInt(buildingLevelData._peopleCost * 0.5f);
          


            _onResourceChange.Invoke();
            return true;
        }
        else
        {
            Debug.LogError("Not enough available resources!");
            return false;
        }
    }


    public void FeedPeople()
    {
        if(!_enableConsumption)
        {
            return;
        }
        _foodAmount -= GetConsumption();
        if (HasFoodToFeed(_peopleMaxAmount))
        {
            _starveCounter = 0;
        }
        else
        {
            _foodAmount = 0;
            ++_starveCounter;
        }
        _onResourceChange.Invoke();

        if (_starveCounter >= _maxStarveCounter)
        {
            GameLoop.GameOver();
            Debug.Log("Your people starved");
        }
    }

    public void AddResources(int woodAmount, int stoneAmount, int foodAmount, int peopleAmount)
    {
        // Wood
        _woodAmount += woodAmount;
        if (_woodAmount > _woodMaxAmount)
        {
            _woodAmount = _woodMaxAmount;
        }

        // Stone
        _stoneAmount += stoneAmount;
        if (_stoneAmount > _stoneMaxAmount)
        {
            _stoneAmount = _stoneMaxAmount;
        }

        // Food
        _foodAmount += foodAmount;
        if (_foodAmount > _foodMaxAmount)
        {
            _foodAmount = _foodMaxAmount;
        }
        if(foodAmount > 0)
        {
            _starveCounter = 0;
        }
        
        // People
        _peopleAmount += peopleAmount;
        if (_peopleAmount > _peopleMaxAmount)
        {
            _peopleAmount = _peopleMaxAmount;
        }

        _onResourceChange.Invoke();
    }



    public void ReturnPeople(int amount)
    {
        _peopleAmount += amount;
        if (amount < 0)
        {
            _peopleMaxAmount += amount;
        }
        _onResourceChange?.Invoke();
    }

    public void AddResources(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                _woodAmount += amount;
                if (_woodAmount > _woodMaxAmount)
                {
                    _woodAmount = _woodMaxAmount;
                }
                else if(_woodAmount <= 0)
                {
                    _woodAmount = 0;
                }

                break;
            case ResourceType.Stone:
                _stoneAmount += amount;
                if (_stoneAmount > _stoneMaxAmount)
                {
                    _stoneAmount = _stoneMaxAmount;
                }
                else if(_stoneAmount <= 0)
                {
                    _stoneAmount = 0;
                }
                break;
            case ResourceType.Food:
                _foodAmount += amount;
                if (_foodAmount > _foodMaxAmount)
                {
                    _foodAmount = _foodMaxAmount;
                }
                else if (_foodAmount <= 0)
                {
                    _foodAmount = 0;
                }
                break;
            case ResourceType.People:
                _peopleAmount += amount;
                _peopleMaxAmount += amount;
                if (_peopleAmount <= 0)
                {
                    _peopleAmount = 0;
                }
                break;
            case ResourceType.Capacity:
                IncreaseCapacity(amount);
                break;

        }

        _onResourceChange.Invoke();
    }

    public void RemoveResources(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                _woodAmount -= amount;
                if (_woodAmount <= 0)
                {
                    _woodAmount = 0;
                }

                break;
            case ResourceType.Stone:
                _stoneAmount -= amount;
                if (_stoneAmount <= 0)
                {
                    _stoneAmount = 0;
                }
                break;
            case ResourceType.Food:
                _foodAmount -= amount;
                if (_foodAmount <= 0)
                {
                    _foodAmount = 0;
                }
                break;
            case ResourceType.People:
                _peopleAmount -= amount;
                _peopleMaxAmount -= amount;
                if (_peopleAmount <= 0)
                {
                    _peopleAmount = 0;
                }
                if (_peopleMaxAmount <= 0)
                {
                    _peopleMaxAmount = 0;
                }
                break;
            case ResourceType.Capacity:
                IncreaseCapacity(amount);
                break;

        }

        _onResourceChange.Invoke();
    }

    public int GetResources(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return _woodAmount;
            case ResourceType.Stone: return _stoneAmount;
            case ResourceType.Food: return _foodAmount;
            case ResourceType.People: return _peopleAmount;
            default: throw new System.Exception($"{type} not implemented in GetResources");
        }
    }

    private void IncreaseCapacity(int amount)
    {
        _foodMaxAmount += amount;
        _stoneMaxAmount += amount;
        _woodMaxAmount += amount;

    }

    private void DecreaseCapacity(int amount)
    {
        _foodMaxAmount -= amount;
        _stoneMaxAmount -= amount;
        _woodMaxAmount -= amount;
        _peopleMaxAmount -= amount;
    }






    private void Awake()
    {
        // Start with no resources since you don't have a main hall yet
        _woodAmount = 0;
        _stoneAmount = 0;
        _foodAmount = 0;
        _peopleAmount = 0;

        // At the beginning the amount of available people == the starting amount of people
        _peopleMaxAmount = 0;

        FindObjectOfType<Builder>()._onBuildingPlace.AddListener(SetStartingAmount);

    }


    private void Start()
    {
        _onResourceChange.Invoke();
    }


    // SAFETY CHECKS
    public bool HasFoodToFeed()
    {
        return _foodAmount - _peopleMaxAmount >= 0;
    }

    public bool HasFoodToFeed(int foodCost)
    {
        return _foodAmount >= 0;
    }
}
